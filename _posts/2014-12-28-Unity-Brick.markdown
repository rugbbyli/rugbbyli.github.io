---
layout: post
title:  "Unity学习笔记：“彩色砖块”"
date:   2014-12-28
categories: Unity
tags: Unity Demo
unityfile: "/files/Brick.unity3d"
unitywidth: 320
unityheight: 568
---

###背景：
彩色砖块最早流行于2010年左右，迄今为止也是我最爱玩的小游戏之一。上手容易高分不易，玩起来根本停不下来啊！！记得当年在QQ空间小游戏里面玩到，总是玩不了满分，后来还特意写了个辅（wai）助（gua）程序帮忙玩，终于拿到了满分……<br>
扯远了。前阵子初学U3D时曾经拿它下手，做了个简单的demo，后来就没管过了。今天偶然发现了它，想想还是发上来吧，希望能够帮到somebody~~<br>
PS，最近略忙，暂时没时间做详细教程了，就简单介绍下思路和结构吧，感兴趣的同学可以自己看下代码，挺简单的（毕竟初学时做的）……项目有点乱，凑合着看吧……
再PS，补上玩法介绍，盗个网上的介绍图吧<br>
![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/brick_tip.PNG)

###效果：
{% include unity.html %}

###实现：
下面是大致的程序结构图：<br>

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/brick_cm.PNG)

总体来讲，采用了分模块的结构，比如AudioManager/InputManager/StorageManager/TimeManager/BrickManager等，最后由GameManager总体协调和调度各个模块的工作。<br>

游戏流程大致如下：<br>

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/brick_gm.PNG)



###示例代码下载（Unity4.3）
[Brick.unitypackage](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/files/Brick.unitypackage "Brick.unitypackage")