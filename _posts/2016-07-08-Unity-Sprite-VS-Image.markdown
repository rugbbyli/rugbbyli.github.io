---
layout: post
title:  "Unity2D：Sprite和UI Image的区别"
date:   2016-07-08
categories: Unity
tags: Unity 2D
---

Unity3D最初是一个3D游戏引擎，而从4.3开始，系统加入了Sprite组件，Unity也终于有了2D游戏开发的官方解决方案。4.6更是增加了新的UI系统uGUI，使得使用Unity开发2D游戏效率非常高。

那么对于从事2D游戏开发的同学来说，想必都曾经遇到过2D元素渲染的选择问题。大家都知道，Unity可以将导入的图片分割为若干Sprite，然后通过SpriteRenderer组件或者uGUI的Image组件来渲染。一般情况下，两者的显示效果是一致的。那么究竟该使用哪个组件呢？

![image](/imgs/sp_vs_img_1.png)
（左侧为Image，右侧为Sprite，下同）

首先分析下两者的异同。

使用上，两者区别不大，都是使用一个Sprite源进行渲染，而Image需要位于某个Canvas下才能显示出来。场景中的Sprite可以像普通的3D游戏物体一样对待，通过Transform组件进行移动等操作，而Image则使用RectTransform进行布局，以便通过Canvas统一管理。由于RectTransform可以设置大小、对齐方式等，Image可以说更加方便一点，这也是很多人选择使用Image的原因。

渲染上，Sprite使用SpriteRenderer组件渲染，而Image则由CanvasRenderer组件渲染。两者在视觉上没有任何区别（都使用默认材质时）。它们默认的渲染也都是在Transparent Geometry队列中。

而在引擎的处理上，两者则有很大的不同。将Wireframe选项打开然后在场景中观察，就可以清楚地发现，Image会老老实实地为一个矩形的Sprite生成两个三角形拼成的矩形几何体，而Sprite则会根据显示内容，裁剪掉元素中的大部分透明区域，最终生成的几何体可能会有比较复杂的顶点结构。

![image](/imgs/sp_vs_img_2.png)

那么这种不同会造成什么结果呢？在继续之前，我们先回顾一下游戏中每帧的渲染过程。对任何物体的渲染，我们需要先准备好相关数据（顶点、UV、贴图数据和shader参数等等），然后调用GPU的渲染接口进行绘制，这个过程称作Draw Call。GPU接收到DrawCall指令后，通过一系列流程生成最终要显示的内容并进行渲染，其中大致的步骤包括：

1. CPU发送Draw Call指令给GPU；

2. GPU读取必要的数据到自己的显存；

3. GPU通过顶点着色器（vertex shader）等步骤将输入的几何体信息转化为像素点数据；

4. 每个像素都通过片段着色器（fragment shader）处理后写入帧缓存；

5. 当全部计算完成后，GPU将帧缓存内容显示在屏幕上。

通过上面的认知，我们可以推断：

1. Sprite由于顶点数据更加复杂，在第1/2步时会比Image效率更低；

2. Sprite会比Image执行较多的顶点着色器运算；

3. Image会比Sprite执行更多的片段着色器运算；

看起来似乎Image比Sprite有更大的好处，然而事实上，由于片段着色器是针对每个像素运算，Sprite通过增加顶点而裁剪掉的部分减少了相当多的运算次数，在绝大多数情况下，反而比Image拥有更好的效率 —— 尤其是场景中有大量的2D精灵时。

总结一下，SpriteRenderer会创建额外的几何体来裁剪掉多余的透明像素区域，从而减少了大量的片段着色器运算，并降低了overdraw；而Image则会创建简单的矩形几何体。随着2D元素数量的增加，这种差别会慢慢明显起来。

可以看出，SpriteRenderer确实是经过优化以显示更多的元素的。所以在2D游戏开发中，游戏场景中的元素，应该尽量使用它去渲染。而Image应该仅用于UI显示（实际上即使不考虑性能原因，由于屏幕分辨率的变化，Image可能会被Canvas改变显示位置和实际大小，如果用于游戏内元素的显示，可能会造成跟预期设计不一致的显示结果，也应该避免使用）。

