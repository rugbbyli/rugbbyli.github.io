---
layout: post
title:  "Unity3D学习笔记：3，物理"
date:   2015-03-14
categories: Unity
tags: Unity StudyLine
---

###物理
Unity内建的物理系统可以帮助游戏场景模拟现实中的物理（如碰撞检测和力学系统）。只需要简单地给物体挂载上相关的组件，物体即可拥有物理特征。需要注意的是，Unity内部的2D和3D物理引擎是互相独立工作的，虽然他们提供相似的接口，但彼此是独立存在的。为了明确区分它们，Unity的物理组件都分为两套，命名上遵循XXX和XXX2D的原则。<br>
<br>
最基本的物理组件是Rigidbody（刚体）。给物体添加Rigidbody组件，物体即会被重力影响，且在外力的作用下产生运动。这类物体由于被物理引擎控制运动，应当避免通过调用transform的属性和方法来控制它，以免两者冲突。取而代之，可以通过rigidbody.AddForce等方式，以物理的形式驱动物体运动。<br>任何能够“移动”的物体，如要接入物理系统，都应当添加Rigidbody组件，即使物体并不受外力影响（这种情况下，应该勾选Rigidbody组件的"Is Kinematic"属性，然后通过transform组件驱动物体运动）。这是因为带有Collider组件又不带Rigidbody的物体，会被Unity的物理引擎作为静态碰撞体处理，Unity会缓存它的位置，而非每帧计算。如果强行移动这类物体，会触发引擎重新计算并缓存，从而造成显著的性能下降。<br>
<br>
Colliders组件提供了处理碰撞检测的功能。需要碰撞检测的物体，可以通过添加Colliders组件实现。Unity内置了几种不同形状的简单Collider，如Box Collider（方形）/Sphere Collider（圆形）/Capsule Collider（胶囊型）等，一般情况下，它们单独或者组合使用已经足够处理物体的碰撞。如果需要非常精确的碰撞，可以使用 Mesh Collider等高级碰撞组件。<br>
默认情况下，不同物体的Collider之间是互斥的（不能互相穿过对方）。如果只是需要检测碰撞，同时不对其他物体产生作用，需要勾选Collider的"Is Trigger"属性。<br>
可以通过脚本监听碰撞事件，只需要在附加了Collider组件的物体的任意脚本中实现特定名称的方法即可。有3种类型的事件，分别是碰撞开始，碰撞中和碰撞结束。对应的方法分别为<br>
OnCollisionEnter / OnCollisionStay / OnCollisionExit （对应非Trigger Collider）和<br>
OnTriggerEnter   / OnTriggerStay   / OnTriggerExit   （对应Trigger Collider）。<br>
对于2d物理引擎，对应方法后面需加上"2D"，如OnCollisionEnter2D等。<br>
<br>
Unity内置了一类称作Joint（关节）的物理组件，用来处理多个物体之间的相互作用。关节可以理解为将两个物体连接起来的意思。<br>
FixedJoint（固定）（对应2d为DistanceJoint2D）：限定关节两端的物体相对位置固定。如果不指定连接物体，则相对场景中的位置固定；<br>
HingeJoint（铰链）：限定物体绕着另一个物体运动（想象下门和钟表钟摆的运动）。如果不指定连接物体，则相对场景中的某个点运动；<br>
SpringJoint（弹簧）：限定两个物体像弹簧两端一样运动。如果不指定连接物体，则相对场景中的某个点运动；<br>
CharacterJoint（角色）：用于创建类似布娃娃的角色效果。一般要多个关节组合使用，来模拟人体的骨骼结构。可通过[Create]-->[3D Object]-->[Ragdoll]菜单打开创建向导窗口，将角色的各个主要部位拖动到窗口中，然后生成最终的关节链。<br>
ConfigurableJoint（可配置）：顾名思义，这个组件提供了各种可配置的选项，以帮助创建复杂的关节系统。通过它可以实现前面提到的所有关节组件的功能，但是相比之下，用起来也是相当的复杂。。<br>
SliderJoint2D（滑动，只针对2D）：限定两个物体的相对位置改变只能沿2D空间中某个方向进行。<br>
WheelJoint2D（车轮，只针对2D）：限定物体跟连接对象位置相对固定，物体可以旋转。通过这个组件可以方便地模拟2D游戏中的车轮。<br>
<br>
由于Unity的物理系统是模拟现实的，某些时候可能反而无法达到我们预期的效果。比如《Prototype》这样的游戏中，主角可以以很快的速度奔跑，然后迅速停下来并反向加速，这明显违背了物理学上的惯性。而很多情况下，我们游戏中的角色可能正需要这样的行为。为此，Unity提供了CharacterController组件帮助实现这样的需求。<br>
CharacterController（角色控制器），使得角色不受力学影响（除了重力），但受碰撞影响。它包含有一个CapsuleCollider来处理碰撞检测，并针对一些情况进行了优化表现（比如上下台阶）。它与KinematicRigidbodyCollider（运动学刚体碰撞器）有一些相似之处，比如都不受力的影响同时保留了碰撞检测的能力；区别在于，KinematicRigidbodyCollider在移动中可以对其他刚体施加力，而CharacterController则不会，另外CharacterController可以与静态碰撞体起作用，后者则不会。一般来讲，KinematicRigidbodyCollider适合用于不需要受力的影响，同时又需要碰撞检测的移动物体。<br>
<br>
此外，还有一些简单的实用组件，比如ConstantForce，可以给Rigidbody持续施加固定的力，适合那些行为简单的物体（如火箭等）。还有5.0版本新增的几个2D Effector组件：<br>
Area Effector 2D：在某个区域内起作用，任何接触到此区域的Rigidbody或Collider都会被施加力的效果；<br>
Surface Effector 2D：在物体表面起作用，任何接触到物体表面的Rigidbody2D都会被施加水平的作用力；<br>
Point Effector 2D：在某个点起作用，任何接触到物体的Rigidbody2D都会被施加水平的作用力，力的大小随距离中心点的距离而改变（想象一个磁铁）；<br>
Platform Effector 2D：模拟类似Platform（平台）的行为，主要作用是提供单侧碰撞检测的功能。比如经典的《魂斗罗》游戏中，角色从平台下方可以直接穿过平台跳到上方，而下落后会被平台挡住，就可以利用这个组件实现。<br>