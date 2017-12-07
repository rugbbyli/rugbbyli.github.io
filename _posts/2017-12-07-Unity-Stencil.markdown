---
layout: post
title:  "浅谈Unity uGUI Mask组件实现原理"
date:   2017-12-07
categories: Unity
tags: Unity ugui stencil
---


![image]({{ site.url }}/imgs/ui_stencil/1.png)

之前我曾经简单介绍过Mask的实现原理（见这里：[UnityGUI扩展实例：图片挖洞效果]({{ site.url }}/unity/2017-03/Unity-GUI-Extension-Image-Hole.html))，不过当时侧重点在其它地方，并没有完整探究Mask底层的处理过程。这几天项目中遇到一个小小需求，需要有一个“超级挖洞能手”，能够挖掉不属于自己子级的UI结点，由于之前的Hole是完全模拟Mask组件实现的，只能处理自己的子级，无法满足需求，于是趁此机会梳理了一下Mask的原理，整理在此与大家探讨。

上篇博客已经简单提到过：

> Masking is implemented using the stencil buffer of the GPU.

即Mask是利用了GPU的[模板缓冲](https://zh.wikipedia.org/wiki/%E6%A8%A1%E7%89%88%E7%B7%A9%E8%A1%9D)来实现的，关于模板，打个简单的比方，就像一个面具，可以挡住一部分“脸”的显示一样。Mask的关键代码其实只有一行，如下（为方便理解，对代码做了简化处理）：
```
var maskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always);
```
它的作用是为Mask对象生成一个特殊的材质，这个材质会将StencilBuffer的值置为1。同样的，在UI的基类 MaskableGraphic 中，有这样一行关键代码（为方便理解，对代码做了简化处理）：
```
var maskMat = StencilMaterial.Add(baseMaterial, 1, StencilOp.Keep, CompareFunction.Equal, 1, 0);
```
它的作用是为MaskableGraphic生成一个特殊的材质，这个材质在渲染时会取出StencilBuffer的值，判断**是否为1**，如果是才进行渲染。

注意上述对StencilBuffer的操作是逐像素的，这样即达到了Mask的效果。同样的，我们在上篇中简单将MaskableGraphic的逻辑反转为判断StencilBuffer是否**不为1**，即达到了挖洞的效果。

看起来好像挺简单的，那么背后的功臣——StencilBuffer，究竟是何方神圣呢？来看下[Unity官方文档](https://docs.unity3d.com/Manual/SL-Stencil.html)的说明：
> The stencil buffer can be used as a general purpose per pixel mask for saving or discarding pixels.

> The stencil buffer is usually an 8 bit integer per pixel. The value can be written to, increment or decremented. Subsequent draw calls can test against the value, to decide if a pixel should be discarded before running the pixel shader.

简单来说，gpu为每个像素点分配一个称之为stencil buffer的1字节大小的内存区域，这个区域可以用于保存或丢弃像素的目的。我们举个简单的例子来说明这个缓冲区的本质。

![image]({{ site.url }}/imgs/ui_stencil/1.png)

如上图所示，我们的场景中有1个红色图片和1个绿色图片，黑框范围内是它们重叠部分。一帧渲染开始，首先绿色图片将它覆盖范围的每个像素颜色“画”在屏幕上，然后红色图片也将自己的颜色画在屏幕上，就是图中的效果了。这种情况下，重叠区域内红色完全覆盖了绿色。接下来，我们为绿色图片添加Mask组件。于是变成了这样：

![image]({{ site.url }}/imgs/ui_stencil/2.png)

此时一帧渲染开始，首先绿色图片将它覆盖范围都涂上绿色，同时将每个像素的stencil buffer值设置为1，此时屏幕的stencil buffer分布如下：

![image]({{ site.url }}/imgs/ui_stencil/3.png)

然后轮到红色图片“绘画”，它在涂上红色前，会先取出这个点的stencil buffer值判断，在黑框范围内，这个值是1，于是继续画红色；在黑框范围外，这个值是0，于是不再画红色，最终达到了图中的效果。
所以从本质上来讲，stencil buffer是为了实现多个“绘画者”之间互相通信而存在的。由于gpu是流水线作业，它们之间无法直接通信，所以通过这种共享数据区的方式来传递消息，从而达到一些“不可告人”的目的。

<br><br>

理解了stencil的原理，我们再来看下它的语法。在unity shader中定义的语法格式如下（中括号内是可以修改的值，其余都是关键字）：

```
Stencil
{
    Ref [Value]
    Comp [CompFunction]
    Pass [PassOp]
    Fail [FailOp]
    ReadMask [Value]
    WriteMask [Value]
}
```

其中：

Ref表示要比较的值；
Comp表示比较方法（等于/不等于/大于/小于等）；
Pass/Fail表示当比较通过/不通过时对stencil buffer做什么操作（保留/替换/置0/增加/减少等）；
ReadMask/WriteMask表示取stencil buffer的值时用的mask（即可以忽略某些位）；

翻译一下就是：**将stencil buffer的值与ReadMask与运算，然后与Ref值进行Comp比较，结果为true时进行Pass操作，否则进行Fail操作，操作值写入stencil buffer前先与WriteMask与运算**。

最后，我们来看下Unity渲染UI组件时默认使用的Shader——UI/Default（略去了一些不相关内容）：

```
Shader "UI/Default"
{
    Properties
    {
        ……

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        ……
    }

    SubShader
    {
        ……

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ……

        Pass
        {
            ……
        }
    }
}

```

以及我们代码中调用的StencilMaterial.Add的内部实现（略去了一些不相关内容）：

```
public static Material Add(Material baseMat, int stencilID, StencilOp operation, CompareFunction compareFunction, ColorWriteMask colorWriteMask, int readMask, int writeMask)
{
    ……

    var newEnt = new MatEntry();
    newEnt.count = 1;
    newEnt.baseMat = baseMat;
    newEnt.customMat = new Material(baseMat);

    ……

    newEnt.customMat.SetInt("_Stencil", stencilID);
    newEnt.customMat.SetInt("_StencilOp", (int)operation);
    newEnt.customMat.SetInt("_StencilComp", (int)compareFunction);
    newEnt.customMat.SetInt("_StencilReadMask", readMask);
    newEnt.customMat.SetInt("_StencilWriteMask", writeMask);
    newEnt.customMat.SetInt("_ColorMask", (int)colorWriteMask);
    newEnt.customMat.SetInt("_UseAlphaClip", newEnt.useAlphaClip ? 1 : 0);
    m_List.Add(newEnt);
    return newEnt.customMat;
}
```

可以看到这个方法只是帮助我们生成了一个材质并填充了Stencil相关的参数。至于Mask只能作用于它的子级的限制，则完全是代码层面的限制。那么，如果我们理解没错，事实上我们可以用更简单的方法——使用自定义材质，来完成上文提到的“挖洞”效果或者系统自带的“遮罩”效果。来验证下是不是这样。

<br><br>

首先创建示例场景：

![image]({{ site.url }}/imgs/ui_stencil/4.png)

如图所示，我们的场景中依次放置了blue/stencil/white/green/red/yellow等6张图片，它们都是标准的Image组件设置了颜色，没有任何特别的设置（请注意它们的顺序）。

接下来我们创建3个材质，分别命名为UIStencil/UIMask/UIHole，为它们指定材质为UI/Default，并设置Stencil相关参数：

![image]({{ site.url }}/imgs/ui_stencil/5.png)

如果你读懂了前面的介绍，那么应该能够理解这几个材质的作用了。
接下来，我们将场景中的图片改用我们自定义的材质来渲染（我在每个结点后面增加了它使用的材质名称）：

![image]({{ site.url }}/imgs/ui_stencil/6.png)

可以发现，white/red图片与stencil重叠区域被挖空，而green/yellow则只保留了与stencil的重叠区域。blue没有受到影响。同时它们也不受结点关系的影响（只要保证渲染顺序即可）。这正与我们的猜测一致。

在这个示例中，我们的stencil材质模拟了uGUI中Mask组件的作用，mask材质模拟了MaskableGraphic组件的作用，而hole模拟了上一篇文章中HoleImage组件的作用。通过这个例子可以发现，事实上Mask组件只是标记了一处特定的区域，真正决定要“Mask”的行为是在Image的渲染中判断的。正因此，我们的例子中stencil图片可以同时起到Mask和Hole的作用。

您也可以尝试修改这几个材质的数值，观察场景的变化，可有助于更深刻理解这一模型的工作过程。

### 示例场景下载（使用Unity 2017.1.0f3 创建）

[UIStencil.unitypackage]({{ site.url }}/files/UIStencil.unitypackage "UIStencil.unitypackage")