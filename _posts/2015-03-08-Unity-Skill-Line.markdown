---
layout: post
title:  "Unity3D学习笔记：总结"
date:   2015-03-08
categories: Unity
tags: Unity StudyLine
---

###背景

<!-- begin_summary -->

不知不觉又过了一年了，过去的一年中，断断续续的学了Unity开发的一些相关知识，也做过几个demo项目。有时候会有一种感觉，不知道该从哪一块切入继续提高。总觉得似乎都会，其实又什么都不太会。新年伊始，就写个Unity知识体系学习系列吧，希望可以借此整理下过去一年所学，发现自己知识体系和层次上的缺失，能够有所提高，找一份满意的Unity开发工作，(●'◡'●)<br>

<!-- end_summary -->

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
<br>
下面按照之前列举的几块内容分别分析:<br>

###编辑器相关

Unity的编辑器本身就是运行在Unity引擎上的一个应用，因此，对Unity编辑器的界面扩展完全是基于Unity的GUI系统进行的，使用同样的脚本语言。熟悉了编辑器扩展的框架后，很容易就可以实现各种各样的扩展，打造出最适合自己的独一无二的编辑器。<br>
Unity对扩展代码的识别机制也很简单，只要把代码丢进特定的目录就可以。Unity有一套项目资源目录规范，简单介绍如下：<br>
1，所有资源都放在一个叫做Assets的根目录下（我们下文中没有特别说明的话，根目录即指这个目录）；<br>
2，位于根目录下的某个名为Editor的文件夹下面的脚本被认为是编辑器扩展脚本，不会打包到游戏中；<br>
3，位于根目录下的名为Editor Default Resources的文件夹的内容用来为扩展脚本提供素材；<br>
4，位于根目录下的某个名为Gizmos的文件夹的内容用来为扩展脚本提供图标；<br>
5，位于根目录下的某个名为Plugins的文件夹的内容用来扩展编辑器功能（应该为dll文件）；<br>
6，位于根目录下的某个名为Resources的文件夹的内容允许运行时通过脚本加载到游戏中；<br>
7，Unity内置的一些辅助Package，导入时会自动放置在根目录下的Standard Assets文件夹中；<br>
8，位于根目录下的某个名为StreamingAssets的文件夹下的内容会原封不动地打包到游戏中；<br>
<br>
需要注意的是，每次修改过脚本，切回Unity编辑器时，Unity都会重新编译一次你的代码。这些目录会影响到编译顺序。具体顺序为：<br>
1，编译Standard Assets, Pro Standard Assets 和 Plugins 文件夹下的脚本；<br>
2，编译以上文件夹下的名为Editor的文件夹下的脚本；<br>
3，编译不在Editor文件夹下的脚本；<br>
4，编译剩下的脚本；<br>
5，位于根目录下名为 WebPlayerTemplates 的文件夹的内容不会被编译；<br>
先编译的代码可以引用后编译的代码中的对象，反过来则是不行的。<br>
<br>
编辑器界面扩展主要分为两种方式，改变内置窗口（如Inspector/Scene）内容的显示，或者创建新的窗口。<br>
创建一个新的窗口是很简单的，只需要3步：<br>
1，定义一个类型，并继承UnityEditor.EditorWindow；<br>
2，调用EditorWindow.GetWindow显示窗口；<br>
3，在OnGUI方法中实现显示窗口的代码；<br>
仍然用一个简单的例子说明。新建一个脚本文件，命名为MyWindow.cs，放在Editor文件夹下。内容如下：<br>
{% highlight csharp %}
  public class MyWindow : EditorWindow {
  
  	[MenuItem ("Extension/Open MyWindow")]  //在编辑器菜单添加条目并映射处理方法
  	public static void  ShowWindow () {
  		EditorWindow.GetWindow(typeof(MyWindow));  //打开MyWindow实例
  	}
  
    //显示通过GUI方式绘制窗口内容
  	void OnGUI(){
  		GUILayout.BeginVertical();
  		GUILayout.Label("this is an extension window.");
  		GUILayout.EndVertical();
  	}
  }
{% endhighlight %}
然后切回Unity编辑器，会发现顶部菜单多了Extension项，点击里面的MyWindow条目，自定义的窗口就出现了，如图：<br>

![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/unity_skill_line_1.png)

可以用鼠标拖动位置和调整大小，它会自动记忆这些属性。<br>
<br>
一些情况下，我们可能更希望能够改变Unity编辑器内置窗口的显示，比如对Inspector中我们脚本属性的友好显示，或者在Scene窗口中显示一些对象的辅助内容，这些都是实际项目中经常碰到的需求。通过PropertyDrawer和Editor可以轻松实现这些。<br>
<br>
PropertyDrawer用来自定义某个属性在Inspector中的显示方式。比如，考虑我们的脚本中有个int类型的公共变量，默认情况下，它在Inspector中显示为一个文本框，可以输入一个数字来改变它的值；现在我们想要用一个滑块来改变它的值，来看看怎么做吧~<br>
{% highlight csharp %}
  [CustomPropertyDrawer(typeof(int))]
  public class PropertyDrawe : PropertyDrawer  {

  	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
  
  		EditorGUI.BeginProperty (position, label, property);

  		position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

  		var indent = EditorGUI.indentLevel;
  		EditorGUI.indentLevel = 0;

  		var amountRect = new Rect (position.x, position.y, position.width, position.height);

  		var input = EditorGUI.IntSlider(amountRect, property.intValue, int.MinValue, int.MaxValue);
  		property.intValue = input;

  		EditorGUI.indentLevel = indent;
  
  		EditorGUI.EndProperty ();
  	}
  }
{% endhighlight %}
  将上面的代码放入一个脚本文件，然后放在Editor文件夹下。切到Unity，选中某个物体，如果它挂载了某个脚本，且那个脚本有个Int类型的公共变量，那么Inspector窗口看起来应该类似这样：<br>
  
![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/unity_skill_line_2.png)
<br>
Editor则提供了更多的灵活性。通过继承并重写它的方法，可以完全定制Inspector窗口或者在Scene窗口中展示自定义的内容。我通过一个简单的例子来说明它的用法：<br>
考虑这样一个需求。我们的游戏中有许多角色，每个角色都有不同的视野范围（包括视距和视角）。我们定义一个脚本来实现这个，如下：<br>
{% highlight csharp %}
public class FarSeer : MonoBehaviour
{
	public float viewDistance = 5f;
	public float viewAngle = 60f;

  void Update()
	{
		transform.Rotate(0, 1, 0);
	}
}
{% endhighlight %}
然后给每个角色挂上这个脚本。接下来，通过扩展编辑器，我们将直观的看到每个角色的视野。<br>
新建一个脚本，命名随意，放在Editor文件夹下。内容如下：<br>
{% highlight csharp %}
[CustomEditor(typeof(FarSeer))] //标记这个类是FarSeer类型的自定义编辑器扩展。当场景中某个挂载有FarSeer组件的物体被选中时，会执行这里的代码。
public class CustomEditorDemo : Editor
{
	FarSeer current;
	
	void OnEnable()
	{
		current = target as FarSeer; //target储存了用户在场景中选中的物体
	}

  //在这里实现扩展Scene窗口的逻辑。
	public void OnSceneGUI()
	{
		var trans = current.transform;

    //计算视野范围的起点
		var from = Quaternion.Euler(0, -current.viewAngle / 2, 0) * trans.forward;

    //设置绘制的颜色
		Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    //绘制视野的扇形
		Handles.DrawSolidArc(trans.position, trans.up, from, current.viewAngle, current.viewDistance);
	}
}
{% endhighlight %}
切回Unity，选中某个角色，你应该会看到类似下面的画面：<br>
![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/unity_skill_line_3.png)
试着在Inspector窗口改变viewDstance和viewAngle的值，观察Scene中的扇形会不会同步更新。<br>
接下来，我将改变FarSeer组件在Inspector窗口的属性展示，通过滑块限制用户输入值的范围。<br>
首先在FarSeer中定义视距和视角的范围值：<br>

	public static int viewDistanceMax = 10;
	public static int viewDistanceMin = 1;
	public static int viewAngleMax = 180;
	public static int viewAngleMin = 5;

然后在上面的CustomEditorDemo类中添加如下代码：<br>
{% highlight csharp %}
//在这里实现扩展Inspector窗口的逻辑。
public override void OnInspectorGUI()
{
	EditorGUILayout.BeginVertical();
	
	EditorGUILayout.LabelField("View Angle:");
	current.viewAngle = EditorGUILayout.IntSlider((int)current.viewAngle, FarSeer.viewAngleMin, FarSeer.viewAngleMax);
	
	EditorGUILayout.LabelField("View Distance:");
	current.viewDistance = EditorGUILayout.Slider(current.viewDistance, FarSeer.viewDistanceMin, FarSeer.viewDistanceMax);

	EditorGUILayout.EndVertical();
	
	EditorUtility.SetDirty(target);
}
{% endhighlight %}
EditorGUILayout用来在Editor中绘制GUI内容，用法跟GUILayout一样，这里不详述。<br>
然后切回Unity，可以看到FarSeer在Inspector中变成了这样：<br>
![image](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/imgs/unity_skill_line_4.png)
拖动滑块，可以看到Scene中的扇形在同步更新，且范围被限定在min和max之间。<br>

###物理
Unity内建的物理系统可以帮助游戏场景模拟现实中的物理（如碰撞检测和力学系统）。只需要简单地给物体挂载上相关的组件，物体即可拥有物理特征。需要注意的是，Unity内部的2D和3D物理引擎是互相独立工作的，虽然他们提供相似的接口，但彼此是独立存在的。为了明确区分它们，Unity的物理组件都分为两套，命名上遵循XXX和XXX2D的原则。<br>
<br>
最基本的物理组件是Rigidbody（刚体）。给物体添加Rigidbody组件，物体即会被重力影响，且在外力的作用下产生运动。这类物体由于被物理引擎控制运动，应当避免通过调用transform的属性和方法来控制它，以免两者冲突。取而代之，可以通过rigidbody.AddForce等方式，以物理的形式驱动物体运动。<br>任何能够“移动”的物体，如要接入物理系统，都应当添加Rigidbody组件，即使物体并不受外力影响（这种情况下，应该勾选Rigidbody组件的"Is Kinematic"属性，然后通过transform组件驱动物体运动）。<br>
<br>
Colliders组件提供了处理碰撞检测的功能。需要碰撞检测的物体，可以通过添加Colliders组件实现。Unity内置了几种不同形状的简单Collider，如Box Collider（方形）/Sphere Collider（圆形）/Capsule Collider（胶囊型）等，一般情况下，它们单独或者组合使用已经足够处理物体的碰撞。如果需要非常精确的碰撞，可以使用 Mesh Collider等高级碰撞组件。<br>
默认情况下，不同物体的Collider之间是互斥的（不能互相穿过对方）。如果只是需要检测碰撞，同时不对其他物体产生作用，需要勾选Collider的"Is Trigger"属性。<br>
可以通过脚本监听碰撞事件，只需要在附加了Collider组件的物体的任意脚本中实现特定名称的方法即可。有3种类型的事件，分别是碰撞开始，碰撞中和碰撞结束。对应的方法分别为<br>
OnCollisionEnter / OnCollisionStay / OnCollisionExit （对应非Trigger Collider）和<br>
OnTriggerEnter   / OnTriggerStay   / OnTriggerExit   （对应Trigger Collider）。<br>
对于2d物理引擎，对应方法后面需加上"2D"，如OnCollisionEnter2D等。<br>
<br>
Unity内置了一类称作Joint（关节）的物理组件，用来处理多个物体之间的相互作用。