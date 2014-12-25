---
layout: post
title:  "Unity学习笔记：画面扭曲特效实现"
date:   2014-12-25
categories: Unity
tags: Unity Shader
unityfile: "/files/Candy.unity3d"
unitywidth: 500
unityheight: 400
---

###背景：
Unity自带了多种高级图像效果（ImageEffects），比如各种模糊特效、扭曲效果等，但是只有在UnityPro中才能使用。
那我们使用Free Edition的同学们是不是就只能干瞪眼了呢？当然不是，今天我们就来看下，如何在Free Edition中自己实现Pro Edition中的一个特效：扭曲特效（TwistEffect）。

###分析：
扭曲特效的实现原理：从中心点（一般为图像中点）向周围发散，每个点的颜色都从图像中按一定角度旋转采样，离中心点越近，旋转角度越大，达到扭曲半径（一般为图像边缘）时旋转角度缩减为0，即可实现这种特效。<br>
由于要操作像素，需要用到片段着色器（FragmentShader）。 <br>


Shader "Test/Twist Effect" {

SubShader
{
  Pass
  {
    ZTest Always Cull Off ZWrite Off
    Fog { Mode off }

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma fragmentoption ARB_precision_hint_fastest 

    #include "UnityCG.cginc"

    uniform sampler2D _MainTex;

    uniform float4 _MainTex_ST;

    uniform float4 _MainTex_TexelSize;
    uniform float _Angle;
    uniform float4 _CenterRadius;

    struct v2f {
      float4 pos : POSITION;
      float2 uv : TEXCOORD0;
      float2 uvOrig : TEXCOORD1;
    };

    v2f vert (appdata_img v)
    {
      v2f o;
      o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
      float2 uv = v.texcoord.xy - _CenterRadius.xy;
      //o.uv = TRANSFORM_TEX(uv, _MainTex); //MultiplyUV (UNITY_MATRIX_TEXTURE0, uv);
      o.uvOrig = uv;
      return o;
    }

    float4 frag (v2f i) : COLOR
    {
      float2 offset = i.uvOrig;
      //angle:点距中心点的距离（0-1）
      float angle = 1.0 - length(offset / _CenterRadius.zw);
      angle = max (0, angle);
      //angle:点旋转角度（0 ~ _Angle)
      angle = angle * _Angle;

      float cosLength, sinLength;
      sincos (angle, sinLength, cosLength);
  
      float2 uv;
      uv.x = cosLength * offset[0] - sinLength * offset[1];
      uv.y = sinLength * offset[0] + cosLength * offset[1];
      uv += _CenterRadius.xy;
  
      return tex2D(_MainTex, uv);
    }
    ENDCG

  }
}

Fallback off

}




事实上，UnityPro中的实现也是通过shader，而它之所以不能在Free Edition中使用，主要原因不在于扭曲的实现，而在于图像源的采集。一般情况下，我们要扭曲整个屏幕的画面，那么通过UnityPro中的功能（OnRenderImage方法），可以很轻松的在所有渲染任务完成后，对渲染结果套用这个shader进行处理。而我们要想实现效果，就必须自己想办法捕获源图像。



###效果：
{% include unity.html %}

###游戏界面制作：
界面制作可以使用多种方式，本文使用Unity4.6新增的uGUI框架制作。<br>
首先创建一个Canvas，命名为Root，将RenderMode改为"Screen Space-Camera"，因为我们的UI将会和非UI元素共存。将UiScaleMode改为"Scale With Screen Size"，这样UI元素会随屏幕分辨率变化自动缩放。<br>
然后在Root下新建一个Image，命名为Bg，作为游戏背景图片。对齐方式选择水平和垂直均为stretch，铺满屏幕。<br>
然后在Bg下新建一个Image，命名为GameArea，作为游戏区域，产生糖果。对齐依然是stretch，Margin看喜好调整，我的数值是left 40，right 40，top 90， bottom 50。<br>
这样游戏场景就基本布置完了。接下来制作糖果。<br>
在GameArea下新建一个Image，命名为Candy，调整它的对齐方式为Left/Top，Pivot为（0,1），然后将它保存为Prefab。<br>
接下来，我们测一下糖果的生成。在GameArea上添加脚本，命名为CandyManager，打开它，添加以下代码：

{% highlight csharp %}
  public GameObject CandyPrefab;//糖果的Prefab
  public Sprite[] CandyImgs;//糖果的图片列表
  RectTransform transform2D;

    void Test(int rows, int columns)
    {
        var gridWidth = transform2D.rect.width / columns;
        var gridHeight = transform2D.rect.height / rows;
    
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var obj = Instantiate(CandyPrefab) as GameObject;
                obj.transform.SetParent(transform);
                var img = obj.GetComponent<Image>();
                img.sprite = CandyImgs[Random.Range(0, CandyImgs.Length)];
                var rtf = obj.transform as RectTransform;
                rtf.localScale = Vector3.one;
                rtf.sizeDelta = new Vector2(gridWidth, gridHeight);
        rtf.anchoredPosition3D = Vector3.zero;
                rtf.anchoredPosition = new Vector2(c * gridWidth, -r * gridHeight);
            }
        }
    }

  void Start () {
        transform2D = transform as RectTransform;
        Test(4, 6);
  }
{% endhighlight %}

完成后，切到Unity，将前面创建的prefab和糖果的图片拖动赋值。点击运行，4行6列随机的糖果就出现在了GameArea上，自动适应GameArea的大小。要点：操作UI元素，要通过RectTransform来进行。通过它的rect属性获取游戏区域的大小，通过anchoredPosition设置在父节点中的位置，通过sizedDelta设置大小。<br>
留心的话，你可能会发现上面的代码在设置糖果的位置时，涉及到两行代码，对anchoredPosition赋值前，还有一句<br>rtf.anchoredPosition3D = Vector3.zero<br>这是什么意思呢？我们可以试一下屏蔽掉这行代码，然后运行，很大可能性，你会发现屏幕上一个糖果都没有了，点击生成的糖果，可以看到它的Pos Z是很大或者很小的一个数字，超出了我们摄像机的显示范围。看来，当UI元素和世界元素共用同一个摄像机时，我们需要将UI元素的z坐标也进行设置。这就是上面那句代码的用途。<br>
当然，这里其实还有个问题。为何不直接通过类似<br>rtf.anchoredPosition3D = new Vector2(c * gridWidth, -r * gridHeight)<br>设置坐标，而是分为xy和z进行两次设置呢？其实我不太清楚这里是不是unity的一个bug，我的测试中，通过设置anchoredPosition3D属性，能生效的只有z坐标，xy坐标是无效的。<br>
  
###UI动画
现在已经可以正常生成若干行列的糖果了，但是界面看上去似乎总少了点东西，一点击运行，立马就出现了满屏幕的糖果，如果能通过动画效果，让糖果从天而降，看起来似乎会好得多。<br>
如果是通过NGUI制作界面，那么恭喜了，NGUI自带了ui动画系统，可以方便的进行诸如平移/缩放/旋转等动画。可是我们用的是uGUI呢，uGUI并没有自带动画系统，怎么办呢？自己写吗？<br>
等等，作为好的程（dai）序（ma）员（gou），不偷懒怎么行呢？既然NGUI有了，我们把它移植过来不就行了吗？<br>
说干就干。我们发现NGUI里面有个TweenPosition脚本，哈哈，要的就是它，果断拿过来。然后发现它是继承UITweener的，好，把UITweener也拿过来。然后，UITweener里面还用到了EventDelegate，这个我们似乎用不到，是去掉呢还是拿过来呢？想想减代码可能会徒增烦恼，算了，我们把EventDelegate也拿过来。最后的最后，我牵起你的手……阿呸，输入法太丧心病狂了，最后的最后，我们打开TweenPosition，改动以符合我们的需要。<br>
首先看一下它的实现，它是通过在update时给value赋值来移动物体的，value内部则是通过cachedTransform来改变物体位置。so，我们改动起来就很easy了。把cachedTransform类型改为RectTransform，value的实现改为：<br>
{% highlight csharp %}
public Vector2 value
{
  get
  {
    return cachedTransform.anchoredPosition;
  }
  set
  {
    cachedTransform.anchoredPosition = value;
  }
}
{% endhighlight %}
最后，我们测试下效果吧~把Test的返回类型改为IEnumerator，将<br>
  rtf.anchoredPosition = new Vector2(c * gridWidth, -r * gridHeight);
改为<br>
  rtf.anchoredPosition = new Vector2(c * gridWidth, (rows - r) * gridHeight);
  TweenPosition.Begin(obj, 0.5f, new Vector2(c * gridWidth, -r * gridHeight)).method = UITweener.Method.EaseIn;
  yield return null;
一开始把糖果位置设置为屏幕上方，通过TweenPosition动画让它落下到应该处于的位置上。<br>
将Start方法里面的<br>Test(4, 6)<br>调用改为<br>StartCoroutine(Test(4, 6));<br>
运行游戏，一堆糖果从天而降，enjoy it！

###示例代码下载（Unity4.6）
[Candy.unitypackage](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/files/Candy.unitypackage "Candy.unitypackage")