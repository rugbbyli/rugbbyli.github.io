---
layout: post
title:  "Unity3D插件教程：在项目中查找脚本的引用"
date:   2015-04-06
categories: Unity
tags: Unity Editor
---

###背景：

<!-- begin_summary -->

有时候我们需要找出项目中所有的引用到某个脚本的地方（比如Prefabs/Scene GameObjects等）。当项目比较大时，手工寻找还是非常费时费力的事情。本文尝试通过插件自动搜索。

<!-- end_summary -->

###分析：

基本的思路是：首先筛选出项目中全部Prefab，加载每个Prefab并判断是否有挂载目标脚本，然后载入每个场景，判断场景中每个物体是否有挂载目标脚本，最后列出结果。

###实现：

####1，在右键菜单项中添加菜单：<br>
新建一个类，命名为 FindScriptRef ，并继承自 EditorWindow 。添加如下方法：

{% highlight csharp %}
[MenuItem("Assets/Find All Reference")]
public static void ShowWindow()
{
    //Show existing window instance. If one doesn't exist, make one.
    EditorWindow.GetWindow(typeof(FindScriptRef));
}
{% endhighlight %}

这段代码会在菜单中添加一个名为"Find All Reference"的菜单项。选中菜单项会打开一个FindScriptRef窗口实例。当然此时窗口中没有任何内容。

####2，窗口基本显示逻辑：<br>

{% highlight csharp %}
void OnGUI()
{
    if (Selection.activeObject == null)
    {
        GUILayout.Label("select a script file from Project Window.");
        return;
    }

    //判断选中项是否为脚本
    var name = Selection.activeObject.name;
    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
    var dict = System.IO.Path.GetDirectoryName(assembly.Location);
    assembly = System.Reflection.Assembly.LoadFile(System.IO.Path.Combine(dict, "Assembly-CSharp.dll"));
    var selectType = assembly.GetType(name);
    if (string.IsNullOrEmpty(name) || selectType == null)
    {
        GUILayout.Label("select a script file from Project Window.");
        return;
    }
    
    GUILayout.BeginVertical();
    GUILayout.BeginHorizontal();

    //列出脚本名称和“Find”按钮
    GUILayout.Label(name);
    bool click = GUILayout.Button("Find");
    GUILayout.EndHorizontal();
    GUILayout.Space(10);

    //列出搜索结果
    if (findResult != null && findResult.Count > 0)
    {
        GUILayout.BeginScrollView(Vector2.zero, GUIStyle.none);
        foreach (string path in findResult)
        {
            GUILayout.Label(path);
        }
        GUILayout.EndScrollView();
    }

    if (click)
    {
        Find(selectType);
    }
    GUILayout.EndVertical();
}
{% endhighlight %}

然后，实现Find方法，搜索指定Type的全部引用：

{% highlight csharp %}
void Find(System.Type type){

    //step 1:find ref in assets

    //filter all GameObject from assets（so-called 'Prefab'）
    var guids = AssetDatabase.FindAssets("t:GameObject");

    findResult = new List<string>();

    var tp = typeof(GameObject);

    foreach (var guid in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);

        //load Prefab
        var obj = AssetDatabase.LoadAssetAtPath(path, tp) as GameObject;

        //check whether prefab contains script with type 'type'
        if (obj != null)
        {
            var cmp = obj.GetComponent(type);
            if (cmp == null)
            {
                cmp = obj.GetComponentInChildren(type);
            }
            if (cmp != null)
            {
                findResult.Add(path);
            }
        }
    }

    //step 2: find ref in scenes

    //save current scene
    string curScene = EditorApplication.currentScene;
    EditorApplication.SaveScene();

    //find all scenes from dataPath
    string[] scenes = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);

    //iterates all scenes 
    foreach (var scene in scenes)
    {
        EditorApplication.OpenScene(scene);

        //iterates all gameObjects
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            var cmp = obj.GetComponent(type);
            if (cmp == null)
            {
                cmp = obj.GetComponentInChildren(type);
            }
            if (cmp != null)
            {
                findResult.Add(scene.Substring(Application.dataPath.Length) + "Assets:" + obj.name);
            }
        }
    }

    //reopen current scene
    EditorApplication.OpenScene(curScene);
    Debug.Log ("finish");
}
{% endhighlight %}

此时切换到Unity，在Project窗口选中一个scrpit，右键选择『Find All Reference』，在打开的窗口选择『Find』按钮，即可看到下面列出了所有引用了这个脚本的位置（如果项目过于庞大，可能需要等待一会儿）。

代码下载：[FindScriptRef.cs](/files/FindScriptRef.cs "FindScriptRef.cs")
