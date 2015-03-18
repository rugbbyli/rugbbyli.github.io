---
layout: post
title:  "Unity3D学习笔记：总结"
date:   2015-03-08
categories: Unity
tags: Unity StudyLine
---

###背景

不知不觉又过了一年了，过去的一年中，断断续续的学了Unity开发的一些相关知识，也做过几个demo项目。有时候会有一种感觉，不知道该从哪一块切入继续提高。总觉得似乎都会，其实又什么都不太会。新年伊始，就写个Unity知识体系学习系列吧，希望可以借此整理下过去一年所学，发现自己知识体系和层次上的缺失，能够有所提高，找一份满意的Unity开发工作，(●'◡'●)<br>

###分析

Unity3d前端开发大致可以分为这么几块：<br>
**编辑器相关（Editor）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;涉及使用编辑器的一些细节，编辑器的一些设置项，以及编辑器扩展方面的内容；<br>
**图形图像（Graphics）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;涉及光照、相机、渲染、Shader等；<br>
**物理（Physics）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;涉及碰撞检测、物理模拟等；<br>
**脚本（Scripting）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;涉及脚本运行流程、脚本控制组件等；<br>
**粒子系统（ParticleSystem）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;涉及粒子发射、渲染和动画等；<br>
**声音（Audio）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;声音系统的工作过程、声音的播放控制、参数调整等；<br>
**动画（Animation）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;动画编辑器的使用、动画状态机编辑器的使用、动画的脚本控制等；<br>
**网络（Network）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;Unity内置网络组件的模型和用法；<br>
**UI相关（UI）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;游戏界面的制作、等；<br>
**平台相关（PlatformSpecific）**<br>
&nbsp;&nbsp;&nbsp;&nbsp;运行平台的判断、平台相关的接口、与原生交互的方式、打包部署等；<br>

###基本结构

Unity是基于组件驱动的体系结构。组件是提供具体功能的模块。游戏场景树中的基本对象是游戏物体（GameObject），它就像个容器，主要的作用就是挂载并驱动组件（Component）的运行。反过来，组件要想运行，一定要挂载在某个游戏物体上。<br>
比如，想在场景中显示一个球体，操作步骤大致如下：<br>

1，在场景中添加一个GameObject；<br>
2，在GameObject上挂载名为Mesh Filter的组件，并设置它的Mesh值为球体模型，它负责提供球体模型数据；<br>
3，在GameObject上挂载名为Mesh Renderer的组件，它负责渲染Mesh Filter提供的模型到场景中；<br>
<br>
每个组件都有若干属性可以设置，比如每个物体都有的基本组件——Transform组件，可以设置Position/Rotation/Scale等属性，以及上面提到的MeshFilter组件，可以设置要处理的网格模型等；<br>
除了使用系统内置的组件，我们还可以通过脚本扩展物体的行为。脚本也是一种组件，通过脚本代码，我们可以获取物体上挂载的其它组件（包括其它的脚本组件），通过与其它组件的交互来自定义物体的行为（比如改变Transform组件的Position属性来移动物体）。<br>
下面按照之前列举的几块内容分别分析:<br>

##编辑器相关
Unity的编辑器本身就是运行在Unity引擎上的一个应用，因此，对Unity编辑器的扩展完全是基于Unity的GUI系统进行的，使用同样的脚本语言。熟悉了编辑器扩展的框架后，很容易就可以实现各种各样的扩展，打造出最适合自己的独一无二的编辑器。<br>
Unity对扩展代码的识别机制也很简单，只要把代码丢进特定的目录就可以。Unity有一套项目资源目录规范，简单介绍如下：<br>
1，所有资源都放在一个叫做Assets的根目录下（我们下文中没有特别说明的话，根目录即指这个目录）；<br>
2，位于根目录下的某个名为Editor的文件夹下面的脚本被认为是编辑器扩展脚本，不会打包到游戏中；<br>
3，位于根目录下的名为Editor Default Resources的文件夹的内容用来为扩展脚本提供素材；<br>
4，位于根目录下的某个名为Gizmos的文件夹的内容用来为扩展脚本提供图标；<br>
5，位于根目录下的某个名为Plugins的文件夹的内容用来扩展编辑器功能（应该为dll文件）；<br>
6，位于根目录下的某个名为Resources的文件夹的内容允许运行时通过脚本加载到游戏中；<br>
7，Unity内置的一些辅助Package，导入时会自动放置在根目录下的Standard Assets文件夹中；<br>
8，位于根目录下的某个名为StreamingAssets的文件夹下的内容会原封不动地打包到游戏中；<br>
需要注意的是，每次修改过脚本，切回Unity编辑器时，Unity都会重新编译一次你的代码。这些目录会影响到编译顺序。具体顺序为：<br>



