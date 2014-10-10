---
layout: post
title:  "为Unity编辑器添加场景自动保存功能"
date:   2014-10-10
categories: Unity
tags: Unity Editor
---

###背景：

由于Unity的编辑器提供了强大的插件扩展功能，可能导致的一个后果就是编辑器会变得不稳定。某些插件的bug会导致编辑器直接挂掉。甚至Unity编辑器本身就有不少bug，一旦触发，就会直接挂掉。<br>
而且Unity本身没有场景自动保存功能。有时候辛辛苦苦摆了半天，因为忘记保存场景，如果突然触发一个bug，此时想死的心情都有了。不多说了，都是泪啊。。<br>
所以催生了自动保存场景的需求。在Unity官方未提供此功能之前，暂且自己实现一个吧。好在Unity的扩展能力实在是太强了，let's do it!

###分析：

在自己造轮子之前，先在网上搜了下，发现已经有提供这种解决方案的。他的思路是创建自定义编辑器窗口，通过窗口定期触发的方法（如OnGUI等）来保存场景。这种方式有两个不太理想的地方：<br>
一是必须要保持打开那个自定义的编辑器窗口，才能定期触发回调自动保存场景；<br>
二是他是通过时间间隔保存的，比如每隔1分钟保存一次；<br>
而通过本篇的方法，可以实现无UI后台定期保存，以及在Hierarchy窗口发生变化时主动保存。<br>

关键技巧如下：
1，借助InitializeOnLoad标记，在编辑器启动时实例化我们的对象，以便监听场景改变；<br>
2，借助EditorApplication.hierarchyWindowChanged回调，监听场景变化通知，以触发自动保存场景；<br>
3，调用EditorApplication.SaveScene方法保存场景。<br>

###实现：

新建一个类，命名为AutoSave：

{% highlight csharp %}
[InitializeOnLoad]
public class AutoSave
{
    static AutoSave()
    {
        
    }
}
{% endhighlight %}

这样，在Unity启动时，就会调用AutoSave类的静态构造函数。记得要把代码放在Editor文件夹下。<br>
然后实现AutoSave方法，如下：
{% highlight csharp %}
static AutoSave()
{
    EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
}

static void OnHierarchyWindowChanged()
{
    SaveScene();
}

static void SaveScene()
{
    if (!string.IsNullOrEmpty(EditorApplication.currentScene) && EditorApplication.SaveScene())
    {
        Debug.Log(string.Format("scene has auto saved (at {0}).", System.DateTime.Now.ToString("HH:mm:ss")));
    }
}

{% endhighlight %}

注意我在SaveScene里面首先判断了currentScene是否为空，如果为空，证明当前场景是新建场景，尚未保存为场景文件。此时如果调用SaveScene，会弹出一个保存文件的对话框。为了不干扰用户的使用，这种情况下将不会自动保存场景。你也可以改变这一逻辑。<br>
此时基本已经完成了功能，但是还有一个问题，那就是EditorApplication.hierarchyWindowChanged只会在Hierarchy视图的树结构发生改变时触发，比如添加、删除对象，改变对象的顺序和层级结构等。但是在Scene视图中拖动物体等情况是不会触发这个回调的（通过Inspector改数据没有这个问题）。暂时没找到能监听Scene视图改变的方法，如果大家有好的方法，欢迎讨论。<br>
那么作为补充，我们添加一个定期保存场景的功能，以处理上面的情况。<br>

声明两个变量，记录最近保存的时间，和保存间隔：<br>
static System.DateTime _lastSavedTime;<br>
static int saveDuration = 120;<br>
在AutoSave构造函数中添加编辑器Update事件的监听：<br>
{% highlight csharp %}
static AutoSave()
{
    EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
    EditorApplication.update += Update;
}
{% endhighlight %}
实现Update方法：<br>
{% highlight csharp %}
static void Update()
{
    if (System.DateTime.Now.Subtract(_lastSavedTime).TotalSeconds > saveDuration)
    {
        SaveScene();
    }
}
{% endhighlight %}
在SaveScene方法中更新_lastSavedTime字段：<br>
{% highlight csharp %}
static void SaveScene()
{
    _lastSavedTime = System.DateTime.Now;

    if (!string.IsNullOrEmpty(EditorApplication.currentScene) && EditorApplication.SaveScene())
    {
        Debug.Log(string.Format("scene has auto saved (at {0}).", System.DateTime.Now.ToString("HH:mm:ss")));
    }
}
{% endhighlight %}

至此，我们的自动保存场景插件就完工了。每隔120s或者当Hierarchy视图发生改变，都会触发场景的自动保存。从此以后妈妈再也不用担心我的场景忘记保存丢失啦~~<br>

代码下载：[AutoSave.cs](http://rugbbyli.github.io/master/files/AutoSave.cs "AutoSave.cs")