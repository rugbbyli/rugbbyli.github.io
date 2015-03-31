---
layout: post
title:  "NGUI自定义控件：Sprite Button"
date:   2014-07-20
categories: Unity
tags: Unity NGUI
---

###背景：

Unity4.3新增了2D框架，我们可以导入一张拼图，并切割为若干Sprite。<br>
但是NGUI对Sprite的支持不是很好，NGUI的类型UIImageButton的Sprite只能从Atlas中选择。<br>
因此封装了一个新的控件ImageButton，可以直接选择Unity的Sprite作为按钮显示。<br>
借助于Unity和NGUI良好的扩展性，只需几步便可完成任务。<br>
<!-- more -->
###分析：

NGUI有对Sprite显示的支持类型，叫做UI2DSprite。同时我们要实现的功能类似于NGUI的UIImageButton，因此可以参照UIImageButton的实现，将它里面用到的Atlas图片源更换为Sprite即可。

###实现：

####1，ImageButton:<br>
实现较为简单，完全参照UIImageButton，声明缓存按钮4种状态下的Sprite的字段，并在监测到不同UI事件发生时将自身的sprite2D属性改为某种状态对应的Sprite即可。

{% highlight csharp %}
[AddComponentMenu("UI/ImageButton")]
public class ImageButton : UI2DSprite
{
    public Sprite NormalSprite;
    public Sprite HoverSprite;
    public Sprite PressedSprite;
    public Sprite DisabledSprite;

    public bool isEnabled
    {
        get
        {
            var col = collider;
            return col && col.enabled;
        }
        set
        {
            var col = collider;
            if (!col) return;

            if (col.enabled != value)
            {
                col.enabled = value;
                UpdateImage();
            }
        }
    }

    void OnEnable()
    {
        UpdateImage();
    }

    void OnValidate()
    {
        if (NormalSprite == null) NormalSprite = sprite2D;
        if (HoverSprite == null) HoverSprite = sprite2D;
        if (PressedSprite == null) PressedSprite = sprite2D;
        if (DisabledSprite == null) DisabledSprite = sprite2D;
    }

    void UpdateImage()
    {
        if (isEnabled) SetSprite(UICamera.IsHighlighted(gameObject) ? HoverSprite : NormalSprite);
        else SetSprite(DisabledSprite);
    }

    void OnHover(bool isOver)
    {
        if (isEnabled)
            SetSprite(isOver ? HoverSprite : NormalSprite);
    }

    void OnPress(bool pressed)
    {
        if (pressed) SetSprite(PressedSprite);
        else UpdateImage();
    }

    void SetSprite(Sprite sprite)
    {
        sprite2D = sprite;
    }
}
{% endhighlight %}

大功告成，现在来测试一发。在场景中添加一个GameObject，点击菜单Component-->UI-->ImageButton将脚本添加到物体，会发现物体自动转移到了NGUI根节点下。选中物体，给Inspector窗口的2D Sprite属性赋值。然后运行游戏，发现Sprite已经正常显示在窗口上。

但此时还有两个小问题，一是物体的属性窗口并没有出现脚本中声明的4个状态Sprite，导致我们赋值变得很麻烦；二是OnPress、OnHover等方法都是无效的，导致按钮的状态无法发生变化。

先看第二个问题，很明显，我们需要为控件添加一个Collider，这样才能触发NGUI的输入事件。考虑到Sprite都是方形的，我们为物体添加一个Box Collider，并调整它的大小到合适的尺寸。<br>
然而随后就会发现问题，我们可能需要经常拖动调整Button的大小，但是Collider却不会随之改变。如果每次调整Button大小后都需要随之再调整一次Collider大小，并使之精确匹配Button的大小，真的是一件挺蛋疼的事情。别着急，这个将会在下面解决。<br>
再来看第一个问题，其实原因也很简单，我们的ImageButton继承了UI2DSprite，而NGUI重写了它的Inspector窗口，导致我们添加的字段不会出现。那么解决方法也很简单，我们为我们的ImageButton也添加一个自定义的Inspector窗口即可。这涉及到Unity的编辑器扩展。

####2，ImageButton编辑器扩展：<br>
新建一个脚本，命名为ImageButtonEditor，并丢在某个Editor文件夹下（这样才能被Unity识别为编辑器扩展）。<br>
我们让它继承UIWidgetInspector（当然别忘记标记它为CustomEditor）。UIWidgetInspector是NGUI的一个编辑器扩展类，NGUI所有的Widget类型的编辑器扩展都继承自它。<br>
NGUI也封装了绘制属性到Inspector窗口的接口NGUIEditorTools.DrawProperty，我们打算调用这个接口，将4个Sprite的属性显示出来。那么在哪个方法中绘制呢？<br>
通过查看NGUI的代码发现，它在OnInspectorGUI方法中调用了ShouldDrawProperties、DrawCustomProperties和DrawFinalProperties 3个方法。根据ShouldDrawProperties的返回值为true/false，DrawCustomProperties中绘制的属性将是启用/禁用状态。DrawFinalProperties不受影响。我们将会在ShouldDrawProperties方法中进行绘制，并根据是否为NormalSprite属性赋值，决定是否禁用Widget相关属性。代码如下：

{% highlight csharp %}
protected override bool ShouldDrawProperties()
{
    SerializedProperty sp = NGUIEditorTools.DrawProperty("NormalSprite", serializedObject, "mSprite");
    NGUIEditorTools.DrawProperty("HoverSprite", serializedObject, "HoverSprite");
    NGUIEditorTools.DrawProperty("PressedSprite", serializedObject, "PressedSprite");
    NGUIEditorTools.DrawProperty("DisabledSprite", serializedObject, "DisabledSprite");
    return sp.objectReferenceValue != null;
}
{% endhighlight %}

此时切换到Unity，选中我们的ImageButton物体，会发现右边的窗口已经出现了4个属性，同时Widget是禁用的。拖动一个Sprite到NormalSprite属性上，并根据需要更改其他状态的Sprite。然后运行游戏，如果你已经添加了Collider，会发现按钮已经可以正常响应鼠标事件了。

接下来，我们再增加一行代码，使得NormalSprite属性更新后，Scene视图可以立即刷新显示。这很简单，直接在ShouldDrawProperties返回前加上：<br>
mButton.sprite2D = sp.objectReferenceValue as Sprite;

此时控件已经可以正常工作了，不过我们还想更进一步，让它更加完美。我们要增加代码，使得拖动改变Button大小时，它的Collider可以跟随一起改变大小。方法如下：

{% highlight csharp %}
public override void OnInspectorGUI()
{
    base.OnInspectorGUI();
    if (mButton.autoResizeBoxCollider && mButton.collider != null)
    {
        var col = mButton.collider as BoxCollider;
        if (col == null) return;
        col.size = new Vector3(mButton.width, mButton.height, 0);
    }
}
{% endhighlight %}

现在拖动Button大小试试，可以看到它的Collider也会随之改变（你需要将autoResizeBoxCollider勾上）。
最后，我们希望能增加一个菜单项，可以快速的添加一个具有Collider组件的ImageButton到项目中。这也很简单：

{% highlight csharp %}
[MenuItem("UI/", false, 2)]
static void Nothing() { }

[MenuItem("UI/Image Button", false, 1)]
static public void AddImageButton()
{
    GameObject go = NGUIEditorTools.SelectedRoot(true);
    if (go != null) Selection.activeGameObject = AddImageButton(go).gameObject;
    else Debug.Log("You must select a game object first.");
}

static public ImageButton AddImageButton(GameObject go)
{
    ImageButton w = NGUITools.AddWidget<ImageButton>(go);
    w.name = "Image Button";
    w.pivot = UIWidget.Pivot.Center;
    w.sprite2D = null;
    w.width = 100;
    w.height = 100;
    var col = w.gameObject.AddComponent<BoxCollider>();
    //col.center = sprite2D.bounds.center;
    col.size = new Vector3(w.width, w.height, 0);
    return w;
}
{% endhighlight %}

通过MenuItem标记增加菜单项，并设置它的触发方法。在方法中，添加一个GameObject，并给它增加ImageButton脚本和BoxCollider组件，最后将它添加到UI树中。

大功告成！现在切换到Unity，会发现顶部菜单项多了一个UI顶级菜单，选择UI-->Image Button，只需一步，即可添加一个ImageButton到UI树中。

###后记：

过程中曾经遇到个很纠结的问题：关于按钮的碰撞器，既然我们的按钮是Sprite2D，为什么不用BoxCollider2D组件呢？其实一开始我也是这么考虑，后来发现按钮一直无法触发点击事件，纠结了一个小时，各种方向怀疑，就是无法解决。后来突然想到UICamera，会不会是这里根本没监测到？于是一翻代码，坑爹呀，只见Raycast方法（检测碰撞的代码）中华丽丽的写着Physics.RaycastAll...它根本就不处理2D碰撞器！折腾这么久，竟然是这样一个小问题，反思了一下，一是对NGUI不熟悉，二是带着之前的想当然的经验去看待新东西，三是网上搜真不如看源码呀~<br>
通过此次入门学习，对NGUI的工作方式和框架有了大致的了解，对Unity的基础知识和编辑器插件开发也有了接触，再接再厉！

代码下载：
[ImageButton.cs](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/files/ImageButton.cs "ImageButton.cs")
[ImageButtonEditor.cs](https://raw.githubusercontent.com/rugbbyli/rugbbyli.github.io/master/files/ImageButtonEditor.cs "ImageButtonEditor.cs")