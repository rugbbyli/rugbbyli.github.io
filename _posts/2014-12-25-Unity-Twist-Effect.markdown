---
layout: post
title:  "Unity3D学习笔记：画面扭曲特效实现"
date:   2014-12-25
categories: Unity
tags: Unity Shader
unityfile: "/files/TwistEffect.unity3d"
unitywidth: 600
unityheight: 480
---

###背景：

<!-- begin_summary -->

Unity自带了多种高级图像效果（ImageEffects），比如各种模糊特效、扭曲效果等，但是只有在UnityPro中才能使用。
那我们使用Free Edition的同学们是不是就只能干瞪眼了呢？当然不是，今天我们就来看下，如何将高级特效移植过来。我拿扭曲特效（TwistEffect）作为演示例子。

<!-- end_summary -->

###分析：
扭曲特效的实现原理：从中心点（一般为图像中点）向周围发散，每个点的颜色都从图像中按一定角度旋转采样，离中心点越近，旋转角度越大，达到扭曲半径（一般为图像边缘）时旋转角度缩减为0，即可实现这种特效。<br>

###效果：
{% include unity.html %}

###实现：
由于要操作像素，需要用到片段着色器（FragmentShader）。不太懂shader的同学也不要害怕，毕竟今天我们是要移植而不是自己写。我们直接将UnityPro中的扭曲特效shader内容复制过来即可。当然，它的实现其实也不复杂，建议还是看一下实现过程。下面是具体的shader内容（关键地方我加了注释）。<br>

{% highlight csharp %}
Shader "Test/Twist Effect" {

SubShader
{
  Tags  {"Queue"="Overlay"}
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
    uniform float _Angle;
    uniform float4 _CenterRadius;

    struct v2f {
      float4 pos : POSITION;
      float2 origin : TEXCOORD0;
    };

    v2f vert (appdata_img v)
    {
      v2f o;
      o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
      float2 uv = v.texcoord.xy - _CenterRadius.xy;
      o.origin = uv; 
      return o;
    }

    float4 frag (v2f i) : COLOR
    {
      float2 offset = i.origin;
      //angle:点距离旋转中心的长度
      float angle = 1.0 - length(offset / _CenterRadius.zw);
      angle = max (0, angle);
      //angle:点旋转的角度
      angle = angle * angle * _Angle;
      float cosLength, sinLength;
      sincos (angle, sinLength, cosLength);
      //计算点采样颜色的uv坐标
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
{% endhighlight %}

然后新建一个Material，命名为TwistEffectMaterial，shader选择上面生成的shader。<br>

然后需要通过脚本控制shader的参数，达到动画效果。这也很简单，我们在脚本中通过
  
  material.SetFloat("_Angle", angle * Mathf.Deg2Rad);

即可改变角度。在update中更新angle值，即可实现动画效果。<br>
这里需要说明的一点是，如果你将脚本直接挂载物体上，它只会对当前物体的显示起作用，对它的子级是无效的。而很多情况下，我们想要的效果是对屏幕的某一区域，或者整个屏幕起效果。UnityPro里面是通过在OnRenderImage中对屏幕内容处理来工作的，这样自然就能实现全屏的效果。而普通版本是不支持OnRenderImage方法的。所以只能自己想办法实现。<br>
说到这里，大部分同学应该都能想到一种简单的实现方法了，对，就是对屏幕区域截图，然后对截图应用扭曲效果。下面使用一个具体的例子来说明。<br>
比如上一篇博文里面的“粉碎糖果”游戏，我们想实现一个效果，当屏幕上没有糖果可以消除时，对整个糖果层运行扭曲特效作为结束游戏的效果。我们的UI层次结构为：
  
{% highlight csharp %}
Backround（铺满屏幕）
|
|----GameArea（距屏幕边缘有一段距离）
      |
      |----Candys（若干糖果）
{% endhighlight %}

我们是要对GameArea区域施加效果。我的做法是，首先在GameArea同级添加一个结点TwistEffectLayer，大小调节为与GameArea保持一致，添加Image组件，将它的Material改为TwistEffectMaterial，用来作为特效层。首先它是未激活的，当它被激活时，会对自身位置和大小的屏幕空间进行截图，然后把截图赋给自身的Image组件，播放扭曲特效动画。<br>
其实这里面主要工作就在精确计算TwistEffectLayer的屏幕位置和尺寸了。由于我使用的是最新的uGUI系统，计算起来还是稍微有点麻烦的，需要考虑Canvas的设置，布局方式，锚点等诸多因素的影响。如果是全屏特效，或者使用NGUI等方式，应该会容易的多。下面是具体的脚本：


{% highlight csharp %}
[RequireComponent(typeof(Image))]
public class TwistEffect : MonoBehaviour {
  public Vector2  radius = new Vector2(0.5F,0.5F);
  public float    angle = 0;
  public Vector2  center = new Vector2(0.5F, 0.5F);

  public float speed = 500f;
  public float maxAngle = 1000;
  public float minAngle = -1000;

  Material material;

  void OnEnable () {
    material = GetComponent<Image>().material;
    material.SetVector("_CenterRadius", new Vector4(center.x, center.y, radius.x, radius.y));
    material.SetFloat("_Angle", angle * Mathf.Deg2Rad);
    StartCoroutine( SetTarget ());
  }
  
  void Update () {
    angle += Time.deltaTime*speed;

    if (angle > maxAngle || angle < minAngle) {
      speed = -speed;
      angle += Time.deltaTime * speed;
    }
     
    UpdateShader ();
  }

  IEnumerator SetTarget(){
    yield return new WaitForEndOfFrame();

    var rectTransform = transform as RectTransform;

    //计算自身的屏幕坐标和大小
    var width = (int)rectTransform.rect.width * Screen.width / 1000;
    var height = (int)rectTransform.rect.height * Screen.width / 1000;
    var pos = Camera.main.WorldToScreenPoint(transform.position);
    pos.x -= width / 2;
    pos.y -= height / 2;

    //生成屏幕区域的Texture
    var tex = new Texture2D (width, height);
    tex.ReadPixels (new Rect((int)pos.x, (int)pos.y, width, height), 0, 0);
    tex.Apply ();

    //将Texture赋值给Image
    Sprite sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));
    var img = GetComponent<Image>();
    img.sprite = sprite;
    img.enabled = true;
  }
  
  void UpdateShader(){
    material.SetFloat("_Angle", angle * Mathf.Deg2Rad);
  }
}

{% endhighlight %}

然后将脚本挂载到前面的TwistEffectLayer上面即可。<br>
最后，在CandyManager脚本中触发特效的启动。为了方面测试，我们每当点击糖果时，就触发特效。添加如下方法：

{% highlight csharp %}
public IEnumerator TestEffect()
{
    var twistEffect = transform.parent.Find("TwistEffectLayer").GetComponent<TwistEffect>();
    twistEffect.speed = Random.Range(0, 10) > 4 ? 300 : -300;
    twistEffect.gameObject.SetActive(true);
    yield return new WaitForSeconds(2);
    Application.LoadLevel(0);
}
{% endhighlight %}

然后在糖果的点击事件顶部添加代码：

{% highlight csharp %}
private void Candy_Click(object sender, System.EventArgs e)
{
    StartCoroutine(TestEffect());
    以前的代码....
}
{% endhighlight %}



###示例代码下载（Unity4.6）
[TwistEffect.unitypackage](/files/TwistEffect.unitypackage "TwistEffect.unitypackage")