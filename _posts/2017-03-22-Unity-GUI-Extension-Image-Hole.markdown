---
layout: post
title:  "UnityGUI扩展实例：图片挖洞效果"
date:   2017-01-04
categories: Unity
tags: Unity ugui
---


我想大家在用uGUI做界面时，可能经常会碰到一种需求，就是在图片上“挖洞”。

![image]({{ site.url }}/imgs/ugui_ext_hole/1.png)

![image]({{ site.url }}/imgs/ugui_ext_hole/2.png)

说起来我们可以有几种实现方案，比如最简单的方式，直接导入带有“洞”的图片。这种方式简单，但不适合需要动态变化的场合。考虑有这种需求：当我们上线一个新功能时，可能希望在玩家第一次打开游戏时，将界面其它地方变暗，突出新增的功能，即所谓的“新手引导”功能。

![image]({{ site.url }}/imgs/ugui_ext_hole/3.png)

如果用黑色含透明区域图片来展示这种效果，也不是不可以，但是会有几个问题。首先需要处理UI遮挡问题，因为Image的透明区域依然会阻挡下层的点击事件；其次如果新手引导分若干步，每步要展示的区域大小和形状都不同，那可能需要针对每步都做图，这个过程会变得非常复杂。

反过来考虑，如果我们可以实现将图片上任意形状区域“剔除”的功能，不就刚好可以满足这种需求吗？这就是本文所讨论的重点：图片“挖洞”的一种实现手段。

首先明确下我们的需求：

1. 我们需要能将图片中某个形状区域隐藏显示；
2. 最好能够让点击事件穿过此区域；

此时熟悉uGUI的同学可能已经发现了，uGUI内置了一种组件叫做Mask，恰好实现了这两种需求（的大部分）。我们先来分析下Mask。

Mask的设计思路是这样的：它与Image组件配合工作，根据Image的覆盖区域来定位显示范围，所有此Image的子级UI元素，超出此区域的部分都会被隐藏（包括UI交互事件）。

于是我们发现，我们想要实现的功能与Mask组件似乎恰好相反：我们是想要此Image覆盖区域的子级UI元素**不显示**，而超出区域的部分照常显示，这样即可（初步）满足需求。

那么我们看下Mask的实现原理吧，看看是不是可以借鉴思路呢？[Unity官方文档](https://docs.unity3d.com/Manual/script-Mask.html)关于Mask的实现原理说明如下：

> Implementation
> 
> Masking is implemented using the stencil buffer of the GPU.
> 
> The first Mask element writes a 1 to the stencil buffer All elements below the mask check when rendering, and only render to areas where there is a 1 in the stencil buffer *Nested Masks will write incremental bit masks into the buffer, this means that renderable children need to have the logical & of the stencil values to be rendered.

可以简单理解为：Mask会将Image的渲染区域像素进行特别标记，稍后子级UI进行像素渲染时，判断如果存在此标记（说明渲染像素位于Mask区域内）就进行渲染，否则不渲染。可以发现，此功能的实现除了Mask组件，还需要子级UI元素的配合。实际上，Unity的内置UI组件都继承自MaskableGraphic，此类型正是Mask的配合实现者，它的相关代码实现如下：

```
    public virtual Material GetModifiedMaterial(Material baseMaterial)
    {
        var toUse = baseMaterial;

        if (m_ShouldRecalculateStencil)
        {
            var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
            m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
            m_ShouldRecalculateStencil = false;
        }

        // if we have a enabled Mask component then it will
        // generate the mask material. This is an optimisation
        // it adds some coupling between components though :(
        Mask maskComponent = GetComponent<Mask>();
        if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive()))
        {
            var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMat;
            toUse = m_MaskMaterial;
        }
        return toUse;
    }
```

知道了Mask的原理，那么我们就会想到一种可能的方案，如果重写MaskableGraphic的GetModifiedMaterial方法，将它的判断逻辑逆转，是否就可以了呢？来试一下吧！新建脚本HoleImage，内容如下：

```
public class HoleImage : Image {
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var toUse = baseMaterial;

        if (m_ShouldRecalculateStencil)
        {
            var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
            m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
            m_ShouldRecalculateStencil = false;
        }

        // if we have a enabled Mask component then it will
        // generate the mask material. This is an optimisation
        // it adds some coupling between components though :(
        Mask maskComponent = GetComponent<Mask>();
        if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive()))
        {
            var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.NotEqual, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMat;
            toUse = m_MaskMaterial;
        }
        return toUse;
    }
```
注意，我们唯一改动的地方在19行，将CompareFunction.Equal改为了CompareFunction.NotEqual，即只有没有被Mask标记的区域才进行渲染。回到Unity，在Canvas下新建一个较小的Image，添加Mask组件，取消勾选“Show Mask Graphic”，并添加一个较大的子级Image，可以发现子级Image已经正确地被Mask组件给挖出了一个洞。

至此，本文的核心问题已经被解决，这个简陋的东西已经可以解决一些问题。接下来我们对它进行进一步处理完善。第一个小问题很容易就暴露了，你会发现游戏运行中当你点击图片空洞时，UI事件并不会传递到下层，反而点击Mask外部区域却传递了UI事件，实际上这正是Mask期望的结果，但却不是我们期望的结果~

这个问题不难解决，我们来分析下uGUI的UI事件传递机制。uGUI通过ICanvasRaycastFilter接口来处理UI捕获，相关方法如下：

```
bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera);
```

UI对象需要实现此接口来自定义焦点捕获判断逻辑。当某个区域坐标被点击时，系统会对当前区域所有UI元素（从顶层到底层）调用此方法，第一个返回true的元素即认定为捕获点击。稍微复杂一点的是嵌套的UI结构。对于某个UI元素，如果它是被嵌套的，那么这个接口调用会从它自身开始，逐级向上调用它的父级UI元素，此过程中任意层级返回false系统都会立刻终止判断，认为此UI不能捕获点击。简单理解的话，就是某个UI元素想要响应某个点击，除了看它自身的意愿，还要看 ~~历史的进程~~ 它爹的意愿，而它爹同意后还要看它爷爷的意见……

所以我们直接从源头做起，干掉它爹——也就是Mask的判断逻辑即可。Mask原本是这样处理的：

```
public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
{
    if (!isActiveAndEnabled)
        return true;

    return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
}
```

嗯，跟我们预期的一样简单粗暴：不在我自身“势力范围”内的统统返回false。那么同样的，我们只需要反转此逻辑即可。新建脚本Hole，内容如下：

```
public class Hole : Mask
{
    public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (!isActiveAndEnabled)
            return true;

        return !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
    }
}
```

将场景中的Mask替换为Hole，运行测试，会发现UI事件已经按照新的逻辑执行了。

至此，通过Hole替代Mask，HoleImage替代Image，我们开头提到的需求已经能够完整解决了。其实还有一个小问题，我们的Hole完整的继承了Mask的逻辑，只是反转了UI事件检测，这也就意味着……对，它的其它子级UI元素依然会表现出Mask的作用。这在一些情形下可能并不是你想要的结果。那么，你能想到用什么方式来解决此问题吗？


###完整代码下载（Unity5.5测试通过）

[Hole.cs]({{ site.url }}/files/Hole/Hole.cs "Hole.cs")

[HoleImage.cs]({{ site.url }}/files/Hole/HoleImage.cs "HoleImage.cs")