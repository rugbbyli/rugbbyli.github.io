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
  Tags {"Queue" = "Overlay+1" }
  Pass
  {
    //ZTest Always Cull Off ZWrite Off
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
      o.uv = TRANSFORM_TEX(uv, _MainTex); //MultiplyUV (UNITY_MATRIX_TEXTURE0, uv);
      o.uvOrig = uv;
      return o;
    }

    float4 frag (v2f i) : COLOR
    {
      float2 offset = i.uvOrig;
      float angle = 1 - length(offset / _CenterRadius.zw);
      angle = max (0, angle);
      angle = angle * angle * _Angle;
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