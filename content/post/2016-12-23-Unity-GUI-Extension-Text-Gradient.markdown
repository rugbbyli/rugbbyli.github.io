---
layout: post
title:  "Unity GUI（uGUI）扩展实例：文本竖直三色渐变"
date:   2016-12-23
categories: Unity
tags: Unity 2D ugui
---

大家知道uGUI中的Text组件默认是单色显示的，而通过一些简单的扩展，我们是可以让它支持渐变色的，比如如下效果：

![image]({{ site.url }}/imgs/ugui_ext_text/unity_text_gradient_demo.png)

在正式开始之前，再来回顾下相关内容。在图形渲染阶段，像素点的颜色默认是根据顶点颜色插值得来的；另外在前面的文章中我们介绍过，uGUI会默认为每个元素生成两个三角形用于渲染（也就是说uGUI的元素形状都是矩形的）；对于Text，每个字符都由（6个顶点组成的）两个三角形来渲染的。

那么根据上面的原则，我们可以很直观地想到，如果可以改变Text的顶点颜色，是不是就能创造渐变效果呢？事实上，这正是uGUI提供给我们的扩展手段之一。来看下今天的主角：BaseMeshEffect。

> BaseMeshEffect
class in UnityEngine.UI/Inherits from:EventSystems.UIBehaviour
Implements interfaces:IMeshModifier
Description
Base class for effects that modify the generated Mesh.

这个类是提供给我们用于修改uGUI生成的网格数据，以自定义ui效果的。使用方式也很简单，首先我们自定义一个类来继承它，然后重写它的虚方法：

```
public override void ModifyMesh(VertexHelper vh);
```

其中VertexHelper是用于辅助网格修改的类，通过它提供的接口，可以很方便地操作ui网格数据。

这个方法会在每次网格更新时调用，我们可以在这里修改网格数据，即可达到想要的效果。根据上面的思路，一个简单的竖直双色渐变代码：

```
[RequireComponent(typeof(Text))]
public class TextVerticalGradientColor : BaseMeshEffect
{
    public Color colorTop = Color.white;
    public Color colorBottom = Color.black;

    protected TextVerticalGradientColor()
    {

    }

    private static void setColor(List<UIVertex> verts, int index, Color32 c)
    {
        UIVertex vertex = verts[index];
        vertex.color = c;
        verts[index] = vertex;
    }

    private void ModifyVertices(List<UIVertex> verts)
    {
        for (int i = 0; i < verts.Count; i += 6)
        {
            setColor(verts, i + 0, colorTop);
            setColor(verts, i + 1, colorTop);
            setColor(verts, i + 2, colorBottom);
            setColor(verts, i + 3, colorBottom);

            setColor(verts, i + 4, colorBottom);
            setColor(verts, i + 5, colorTop);
        }
    }

    #region implemented abstract members of BaseMeshEffect

    public override void ModifyMesh(VertexHelper vh)
    {
        if(!this.IsActive())
        {
            return;
        }
        List<UIVertex> verts = new List<UIVertex>(vh.currentVertCount);
        vh.GetUIVertexStream(verts);

        ModifyVertices(verts);

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }

    #endregion
}
```

整段代码中，我们所做的只是遍历每个顶点，修改它的颜色。将这个脚本添加到Text上，即可看到效果。

当然，直到目前为止，我们只是实现了双色渐变，而如果要实现三色（甚至更多）渐变呢？由于uGUI默认为每个字符生成顶部和底部的6个顶点，单纯靠修改顶点颜色似乎是达不到这个目标的。

如果你仔细阅读并理解了上面的思路，应该不难想到，我们是不是可以通过在字符中间插入顶点的方式来实现三色渐变呢？

![image]({{ site.url }}/imgs/ugui_ext_text/unity_text_gradient.png)

不如我们动手来试一下吧。根据上面的图片分析，我们需要把每个字符的顶点由默认的 <br>
【tl（左上）、tr（右上）、br（右下）、br、bl（左下）、tl】 改为 <br>
【tl、tr、cr（右中）、cr、cl（左中）、tl、cl、cr、br、br、bl、cl】， <br>
并把三角形由默认的 <br>
【tl->tr->br, br->bl->tl】改为 <br>
【tl->tr->cr, cr->cl->tl, cl->cr->br, br->bl->cl】， <br>
核心代码如下：

```
for (int i = 0; i < verts.Count; i += step) {
	//6 point
	var tl = multiplyColor(verts[i+0], colorTop);
	var tr = multiplyColor (verts [i+1], colorTop);
	var bl = multiplyColor (verts [i+4], colorBottom);
	var br = multiplyColor (verts [i + 3], colorBottom);
	var cl = calcCenterVertex(verts[i+0], verts [i+4]);
	var cr = calcCenterVertex (verts [i+1], verts [i+2]);

	vh.AddVert (tl);
	vh.AddVert (tr);
	vh.AddVert (cr);
	vh.AddVert (cr);
	vh.AddVert (cl);
	vh.AddVert (tl);

	vh.AddVert (cl);
	vh.AddVert (cr);
	vh.AddVert (br);
	vh.AddVert (br);
	vh.AddVert (bl);
	vh.AddVert (cl);
}

for (int i = 0; i < vh.currentVertCount; i += 12) {
	vh.AddTriangle (i + 0, i + 1, i + 2);
	vh.AddTriangle (i + 3, i + 4, i + 5);
	vh.AddTriangle (i + 6, i + 7, i + 8);
	vh.AddTriangle (i + 9, i + 10, i + 11);
}
```

修改完成后切回Unity，可以发现一切都如预期的顺利，三色渐变效果已经正常实现，来看下此时的网格：

![image]({{ site.url }}/imgs/ugui_ext_text/unity_text_gradient_show.png)

可以发现，这种效果比默认的Text多生成了一倍的顶点和三角形（也就是每个字符变为12个顶点和4个三角形）。

按照这种思路，诸如水平渐变、径向渐变之类的效果也是不难实现的。通过这篇文章，我们也能发现uGUI的扩展性也是挺好的。

附完整代码（在Unity 5.3 测试通过）：

```
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Extension
{
    [AddComponentMenu ("UI/Effects/Text Vertical Gradient Color")]
    [RequireComponent(typeof(Text))]
    public class TextVerticalGradientColor : BaseMeshEffect
    {
        public Color colorTop = Color.white;
		public Color colorCenter = Color.grey;
        public Color colorBottom = Color.black;

		public bool MultiplyTextColor = false;

        protected TextVerticalGradientColor()
        {

        }

        public static Color32 Multiply(Color32 a, Color32 b)
        {
            a.r = (byte)((a.r * b.r) >> 8);
            a.g = (byte)((a.g * b.g) >> 8);
            a.b = (byte)((a.b * b.b) >> 8);
            a.a = (byte)((a.a * b.a) >> 8);
            return a;
        }

        private void ModifyVertices(VertexHelper vh)
        {
			List<UIVertex> verts = new List<UIVertex>(vh.currentVertCount);
			vh.GetUIVertexStream(verts);
			vh.Clear();

            int step = 6;

			for (int i = 0; i < verts.Count; i += step) {
				//6 point
				var tl = multiplyColor(verts[i+0], colorTop);
				var tr = multiplyColor (verts [i+1], colorTop);
				var bl = multiplyColor (verts [i+4], colorBottom);
				var br = multiplyColor (verts [i + 3], colorBottom);
				var cl = calcCenterVertex(verts[i+0], verts [i+4]);
				var cr = calcCenterVertex (verts [i+1], verts [i+2]);

				vh.AddVert (tl);
				vh.AddVert (tr);
				vh.AddVert (cr);
				vh.AddVert (cr);
				vh.AddVert (cl);
				vh.AddVert (tl);

				vh.AddVert (cl);
				vh.AddVert (cr);
				vh.AddVert (br);
				vh.AddVert (br);
				vh.AddVert (bl);
				vh.AddVert (cl);
			}

			for (int i = 0; i < vh.currentVertCount; i += 12) {
				vh.AddTriangle (i + 0, i + 1, i + 2);
				vh.AddTriangle (i + 3, i + 4, i + 5);
				vh.AddTriangle (i + 6, i + 7, i + 8);
				vh.AddTriangle (i + 9, i + 10, i + 11);
			}
        }

		private UIVertex multiplyColor(UIVertex vertex, Color color)
		{
			if (MultiplyTextColor)
				vertex.color = Multiply (vertex.color, color);
			else
				vertex.color = color;
			return vertex;
		}

		private UIVertex calcCenterVertex(UIVertex top, UIVertex bottom)
		{
			UIVertex center;
			center.normal = (top.normal + bottom.normal) / 2;
			center.position = (top.position + bottom.position) / 2;
			center.tangent = (top.tangent + bottom.tangent) / 2;
			center.uv0 = (top.uv0 + bottom.uv0) / 2;
			center.uv1 = (top.uv1 + bottom.uv1) / 2;

			if (MultiplyTextColor) {
				//multiply color
				var color = Color.Lerp(top.color, bottom.color, 0.5f);
				center.color = Multiply (color, colorCenter);
			} else {
				center.color = colorCenter;
			}

			return center;
		}

        #region implemented abstract members of BaseMeshEffect

        public override void ModifyMesh(VertexHelper vh)
        {
            if(!this.IsActive())
            {
                return;
            }
            

			ModifyVertices(vh);
        }

        #endregion
    }
}

```