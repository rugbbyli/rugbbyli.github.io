---
layout: post
title:  "Unity GUI（uGUI）扩展实例：自定义曲线控件"
date:   2016-07-25
categories: Unity
tags: Unity 2D ugui
---


Unity GUI（uGUI）是Unity官方内置的游戏UI系统，以良好的性能和易用性，以及与Unity的深度集成，受到了不少开发者的喜爱。然而许多同学想必也发现了，相比以NGUI为代表的一些第三方UI系统，uGUI的功能确实少了点。不过不要紧，uGUI提供了良好的可扩展性，可以自己实现一些自定义UI控件。今天通过一个实例来为大家讲解下uGUI扩展的相关内容。

我们要实现的实例是一个曲线控件，可以通过UI显示自定义的曲线，效果如下图所示：

![image](/imgs/ugui_ext_curve/1.png)

首先大概介绍下uGUI的体系结构。在uGUI中，所有的UI组件都要放置在Canvas组件下，由Canvas来管理它的渲染和自适应屏幕等。uGUI提供了Graphic基类，运行时，Canvas会使用CanvasRenderer来渲染它的子级中全部的Graphic组件。所以，如果要自定义外观的控件，从Graphic继承是一个不错的选择。

我们都知道，在unity中3d物体最终是转化成若干网格数据来渲染的，其实ui的渲染也是一样的方法。比如一个Image组件，内部其实是使用4个顶点构成的2个三角形网格外加一个Texture贴图来渲染的。那么要改变控件的外形，只要改变网格数据就可以了，对，就是这么个思路。

Graphic提供了一个虚方法

```
protected virtual void OnPopulateMesh (VertexHelper vh);
```

通过重写这个方法，即可修改或重新生成控件的网格数据，从而达到自定义控件显示外观的需求（Unity4为另一个接口OnFillVBO，不过原理是一致的）。而VertexHelper是unity提供的简化网格操作的辅助类，它提供的接口也很简单，诸如添加顶点、添加三角形、添加Quad等。

另外需要注意的一点是，顶点的坐标是由控件的位置、大小和锚点等决定的，计算时需要综合考虑这些因素。

ok，知道了原理，接下来我们分析下怎么生成一条曲线的网格结构。

首先第一步：我们的曲线数据如何存储？Unity提供了一个动画曲线类AnimationCurve，我决定使用它来存储曲线。好处很明显：无论通过编辑器或者通过代码都很容易生成和修改曲线数据；当然也有缺陷，就是曲线的类型是固定的，某一x轴只能对应一个y轴的点。当然了，作为示例，这个并不重要。

接下来，我们需要对曲线进行采样，以生成对应的网格结构。说白了就是需要把一条曲线分割成许多直线，因为屏幕不可能做到无限精度嘛。

分割后，剩下的工作就是生成一条直线的网格结构啦，这个终于简单了，因为有宽度的直线其实就是个长方形嘛，用4个顶点+两个三角形即可构成。

介绍完了实现思路，就可以开工来完成代码啦：

```
public class Curve : MaskableGraphic 
{
    public AnimationCurve m_Curve = new AnimationCurve();
    public float m_LineWidth = 1;
    [Range(1, 10)]
    public int Acc = 2;
    
	protected override void OnPopulateMesh(VertexHelper vh)
    {
        var rect = this.rectTransform.rect;
        vh.Clear();

        Vector2 pos_first = new Vector2(rect.xMin, CalcY(0) * rect.height);

        for (float x = rect.xMin + Acc; x < rect.xMax; x += Acc)
        {
            Vector2 pos = new Vector2(x, CalcY((x - rect.xMin) / rect.width) * rect.height);
            var quad = GenerateQuad(pos_first, pos);

            vh.AddUIVertexQuad(quad);

            pos_first = pos;
        }

        Vector2 pos_last = new Vector2(rect.xMax, CalcY(1) * rect.height);
        vh.AddUIVertexQuad(GenerateQuad(pos_first, pos_last));

        for (int i = 0; i < vh.currentVertCount - 4; i += 4)
        {
            vh.AddTriangle(i + 1, i + 2, i + 4);
            vh.AddTriangle(i + 1, i + 2, i + 7);
        }
        Debug.Log("PopulateMesh..." + vh.currentVertCount);
    }

    private float CalcY(float x)
    {
        return (m_Curve.Evaluate(x) - rectTransform.pivot.y);
    }

    private UIVertex[] GenerateQuad(Vector2 pos1, Vector2 pos2)
    {
        float dis = Vector2.Distance(pos1, pos2);
        float y = m_LineWidth * 0.5f * (pos2.x - pos1.x) / dis;
        float x = m_LineWidth * 0.5f * (pos2.y - pos1.y) / dis;

        if (y <= 0)
            y = -y;
        else
            x = -x;

        UIVertex[] vertex = new UIVertex[4];

        vertex[0].position = new Vector3(pos1.x + x, pos1.y + y);
        vertex[1].position = new Vector3(pos2.x + x, pos2.y + y);
        vertex[2].position = new Vector3(pos2.x - x, pos2.y - y);
        vertex[3].position = new Vector3(pos1.x - x, pos1.y - y);

        for(int i = 0; i < vertex.Length; i++)
        {
            vertex[i].color = color;
        }

        return vertex;
    }
}
```

先看下OnPopulateMesh的实现，我们首先清除掉网格数据，然后从xMin（控件最左侧）开始按照设定的精度采样算出曲线上的坐标点，然后通过前后两个点和给定的曲线宽度算出4个坐标点，通过VertexHelper.AddUIVertexQuad将它们添加进缓存。采样完毕后，设置三角形数据，完成计算过程。

然后是GenerateQuad的实现，也就是通过两点和宽度计算方形的4个坐标点的算法。由于曲线的角度是变化的，分割出的直线也会是角度不同，所以问题就成了如下图所示的数学问题了：

![image](/imgs/ugui_ext_curve/2.png)

由于楼主数学比较渣，就用了一种比较笨的方法：计算出4个点相对直线两个端点的偏移量（x、y），然后通过端点加减偏移量来计算4个包围点的坐标。如果你有更好的算法，也欢迎分享出来。

至此整个控件的核心工作就完成了。在场景中的Canvas下添加一个UI物体，并挂上我们的Curve脚本，然后通过Inspector修改下曲线，就能在场景中同步看到变化了，同时其它的一些诸如自适应缩放、ui事件响应等ui特性也自动拥有了，一颗赛艇！

![image](/imgs/ugui_ext_curve/3.png)

--注：代码在Unity5.3环境下测试通过