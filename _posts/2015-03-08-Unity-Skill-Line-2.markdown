---
layout: post
title:  "Unity3D学习笔记：2，编辑器相关"
date:   2015-03-08
categories: Unity
tags: Unity StudyLine
---

###编辑器相关

Unity提供了功能完整的编辑器，以帮助开发者更快更好地开发游戏。跟别的开发工具类似，Unity也有项目（Project）的概念，一般情况下，一个游戏对应一个项目。每个项目中又通常包含若干场景（Scene），而每个场景中包含若干游戏物体（GameObject）。每次只能打开一个场景进行操作。常用的几个窗口分别是：<br>
Scene（场景）：以“上帝视角”自由查看、管理和预览当前场景的布置。通过它，可以自由地拖动物体摆放位置，添加和删除物体，进行一些快捷操作等。<br>
Game（游戏）：查看游戏最终显示效果的窗口。随时都可以通过点击“Play”按钮测试场景的实际表现效果，通过Game窗口也可以调整不同的分辨率以测试多分辨率的适配情况，甚至可以连接到手机，实时在手机上预览效果。当然，还有各种状态信息帮助优化游戏，包括帧率、渲染参数、动画数、音频状态等。<br>
Hierarchy（层级）：查看和管理当前场景层级结构。可以添加删除物体，调整物体的层级结构，复制物体等操作。<br>
Project（项目）：管理当前项目资源的窗口。类似于文件管理器，列出了当前项目的Assets文件夹下的内容。可以将文件拖动到这个窗口来导入为资源，也可以直接将要导入的文件放在项目的Assets目录下，Unity会自动扫描并导入它。除了添加操作，别的如删除/移动等操作都不要通过操作系统的文件管理进行，而要通过Unity的Project窗口进行，这是因为资源通常会跟某些场景或别的资源关联，如果从外部更改，会造成这种关联关系的失效。<br>
Inspector（检视）：通过这里可以查看并修改选中的资源或游戏物体的属性，管理物体附加的组件，预览导入的资源等。<br>
<br>
Unity的编辑器自身就是运行在Unity引擎上的一个应用，因此，对Unity编辑器的界面扩展完全是基于Unity的GUI系统进行的，使用同样的脚本语言。熟悉了编辑器扩展的框架后，很容易就可以实现各种各样的扩展，打造出最适合自己的独一无二的编辑器。<br>
Unity对扩展代码的识别机制也很简单，只要把代码丢进特定的目录就可以。Unity有一套项目资源目录规范，简单介绍如下：<br>
1，所有资源都放在一个叫做Assets的根目录下（我们下文中没有特别说明的话，根目录即指这个目录）；<br>
2，位于根目录下的某个名为Editor的文件夹下面的脚本被认为是编辑器扩展脚本，不会打包到游戏中；<br>
3，位于根目录下的名为Editor Default Resources的文件夹的内容用来为扩展脚本提供素材；<br>
4，位于根目录下的某个名为Gizmos的文件夹的内容用来为扩展脚本提供图标；<br>
5，位于根目录下的某个名为Plugins的文件夹的内容用来扩展编辑器功能（一般为dll文件）；<br>
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

{% include img.html param="unity_skill_line_1.png" %}

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