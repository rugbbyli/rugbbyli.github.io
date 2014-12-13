---
layout: post
title:  "Unity学习笔记：使用Shader实现‘别踩白块儿’游戏"
date:   2014-12-5
categories: Unity
tags: Unity Shader
unityfile: "/files/DoNotTapWhiteTile.unity3d"
unitywidth: 360
unityheight: 640
---

###背景：

‘别踩白块儿’是一款很有意思的小游戏，操作简单，十分考验反应速度。由于画面十分简单，只有黑白两种颜色，我们完全可以不需要堆图片，直接使用shader
就可以实现游戏画面。<br>

###分析：

基本思路如下：<br>
1，‘别踩白块儿’的画面是4*4的格子，每行有1个黑块和3个白块，据此，我们可以用4个数字分别代表每行黑块的位置（0~3）。<br>
2，unity允许在shader中指定属性（Properties），这个属性可以被shader和我们的代码访问，因此我们可以通过改变这个属性值来控制渲染结果的变化。<br>
3，利用fragment shader，判断输入点的坐标位置，根据上面的属性值，计算出对应的颜色（黑或白），并渲染结果。<br>

###效果：
{% include unity.html %}

###实现：

####1，新建一个shader，命名为tile，打开它，将内容改为：

{% highlight csharp %}
Shader "Tile/tile" {

}
{% endhighlight %}

这样我们就声明了一个空的shader，分类为Tile，命名为tile。

####2，新建一个Material，命名为TileMaterial，选中它，在Inspector面板中改变它的shader为Tile/tile。

####3，在场景中添加一个Panel，将它的Image组件的Material的内容改为我们上面新建的TileMaterial。注意Panel是unity4.6 UI系统内置的，如果你的版本低于4。6，也可以使用比如Cube之类的物体，原理是一样的。至此，场景就已经布置完成了，开始进入正题。

####4，打开tile.shader文件，在里面添加属性：<br>
    Properties {<br>
        _Data ("Data", Vector) = (0,0,0,0)<br>
    }<br>

我们添加了一个命名为_Data，类型为Vector的变量，默认值为0。这4个值分别用来表示每一行中黑块的位置索引。4个0意味着4个黑块都位于第一列，依此类推。

####5，接下来，在下面添加具体的shader代码，如下：

    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Data;

            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            struct fragmentInput{
                float4 position : SV_POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            fragmentInput vert(vertexInput i){
                fragmentInput o;
                o.position = mul (UNITY_MATRIX_MVP, i.vertex);
                o.texcoord0 = i.texcoord0;
                return o;
            }

            float4 frag(fragmentInput i) : COLOR {

                float4 color;
                int indexY = floor(i.texcoord0.y/0.25);
                int indexX = floor(i.texcoord0.x/0.25);

                if(_Data[indexY] == indexX){
                    color = float4(0,0,0,1);
                }else {
                    color = float4(1.0,1.0,1.0,1.0);
                }

                
                return color;
            }
            ENDCG
        }
    }

关于它的基本结构，这里就不再详述，不太理解的同学可以翻阅下文档  (http://docs.unity3d.com/Manual/SL-ShaderPrograms.html)


我们的工作都在frag函数中进行。首先，通过i.texcoord0.y，可以拿到像素点的y坐标，它的范围是0~1，通过floor(i.texcoord0.y/0.25)，可以将y坐标映射到0~4的区间内，同理计算出x坐标的区间。这样就得到了像素点所在的行列值。

然后，通过_Data[indexY]，拿到对应行的黑块的列索引。将它与indexX比较，如果一致，则点应显示为黑块，所以我们返回黑色float4（0,0,0,1）；反之，返回白色float4（1,1,1,1）。

就这样简单的几部，我们的主要工作就完成了，切回unity，应该看到类似这样的画面（这里我分别使用了Cube/Sphere/Quad来演示效果）：

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/shader1.PNG)


可以发现，物体表面被分成了4行4列，每行的第一列都显示为黑色，其余地方为白色。为加深理解，可以选中TileMaterial，改变它的Data的值，比如改为(0,1,2,3)，会变成这样：

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/shader2.PNG)

此时画面已经有了，但是我们还想更进一步，把分割线也画出来，这样看起来更直观点，要怎么做呢？很简单，只需要在frag函数前面加上这段代码：

    if(i.texcoord0.x % 0.25 <= 0.002 || i.texcoord0.y % 0.25 <= 0.001){
        return float4(0,0,0,1);
    }

这样当点的坐标足够接近0.25的整数倍时，我们直接返回黑色，这样即可画出4*4的黑色的分割线出来。保存代码，切到unity，画面应该变成这样：

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/shader3.PNG)


ok，关于shader的部分就已经完成了，接下来，我们看一下如何通过代码控制它改变。

####6，在物体上附加一个脚本，命名随意，然后打开它。

首先，在脚本中添加两个变量：

    Material _material;
    Vector4 _data;

在Start方法中，我们获取到物体material，并保存到前面的变量中：

    _material = renderer.material;

然后，添加如下方法：

    void UpdateTile()
    {
        _material.SetVector("_Data", RandomData());
    }

    Vector4 RandomData()
    {
        _data.x = _data.y;
        _data.y = _data.z;
        _data.z = _data.w;
        _data.w = Random.Range(0, 4);
        return _data;
    }

通过SetVector方法，我们可以改变shader中的属性值，通过RandomData方法，我们实现了"下落"的效果。

接下来，我们测试下效果，在Start方法中添加如下代码：
    InvokeRepeating("UpdateTile", 1, 1);

切回Unity，点击运行，等1秒钟后，你应该看到物体表面的格子会每隔1秒下落一层，每次顶层的黑块位置是随机生成的。



至此，大部分内容已经完成了，接下来，只需要通过判断用户的点击位置，计算对应位置方块的颜色，判断是”GameOver“还是”UpdateTile“咯~再加上诸如时间控制/分数控制等，一个完整的，没有任何UI素材，只使用shader进行渲染的游戏就诞生啦~<br>

示例代码下载：[TileDemo.unitypackage](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/files/TileDemo.unitypackage "TileDemo.unitypackage")
