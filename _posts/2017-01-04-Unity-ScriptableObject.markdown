---
layout: post
title:  "Unity3D ScriptableObject 简要介绍"
date:   2017-01-04
categories: Unity
tags: Unity ScriptableObject
---


关于Unity3d的数据持久化，或者叫序列化，除了自己实现自定义格式的配置文件和存取逻辑外，更便捷的方式是使用Unity内置的序列化机制。大家都知道，我们在脚本（MonoBehaviour）中定义的公共变量或被标记为【SerializeField】的私有变量会被Unity持久化保存，且可以通过编辑器可视化修改，这便是利用序列化机制保存数据的一种典型使用场景。

由于MonoBehaviour是每个Unity程序员一开始就接触的东西，这种方式可谓人尽皆知，有相当多的人都在用它。然而这种方式也不是没有缺点，首先作为MonoBehaviour，需要挂在某个游戏物体身上才能被实例化，其次，当我们实例化多个脚本时，这些数据将会被创建多次。想象一种极端的例子，我们在某个Prefab的脚本中保存了一个百万级元素的int数组，那么这个Prefab将保留约4MB的内存空间用于存储它。每次实例化这个Prefab，都会复制一份新的数组。当实例化10个Prefab时，相关的内存空间占用将达到40MB。所以在某些场合，特别是类似全局配置项的地方，这并非一个好的选择。

而今天要介绍的主角是 [**ScriptableObject**](http://docs.unity3d.com/Manual/class-ScriptableObject.html) ，作为同样拥有序列化机制的内置类型，在保持了同样的便捷性的同时，又恰好解决了上面提到的问题。首先，它可以用（几乎）同样的方式可视化编辑和存取数据，其次，由于不是组件，它无需绑定在某个场景物体上即可使用。我们来看下它的用法（以及与MonoBehaviour的对比）。

首先看下它的定义，非常简单，我们只需要新建脚本（比如GameConfig），并继承ScriptableObject，然后在类中定义我们的数据项即可，比如：

```csharp
public class GameConfig: ScriptableObject
{
    public string GameName;
    public string Version;
    public string UpdateUrl;

    public bool MyConfig1;
    public int MyConfig2;
}
```

可以发现这一步它跟MonoBehaviour是很像的，除了继承自不同的类型外。

那么定义好结构后要怎么在编辑器中使用它呢？这就是接下来要介绍的，它的创建和管理流程。首先分析下MonoBehaviour的创建过程。当我们创建一个MonoBehaviour脚本并编译后，它只是一个“类”，而非“对象”；将它附加在物体上可以看做生成了一个实例，Unity才会将它序列化保存，并在游戏启动时反序列化为一个对象。

与之类似，虽然 ScriptableObject无法附加在物体上，Unity却提供了一种机制来生成“这个类型的实例化物体”。这个描述不是很严谨，不过应该不影响理解。我们只需要通过调用如下代码即可完成此过程（还是以上面的例子为准）：

```csharp
//生成一个GameConfig对象
var cfg = ScriptableObject.CreateInstance<GameConfig>();
//以它为基础创建一个Asset
AssetDatabase.CreateAsset(cfg, "path to save asset");
//保存Asset
AssetDatabase.SaveAssets();
```

这样即可在项目目录下面看到一个资源，选中它，即可在Inspector窗口看到与MonoBehaviour类似的数据配置界面。

![image](/imgs/unity_scriptable_object_1.png)

其实还有更简单的方法，直接在定义类型前面加上

```csharp
[CreateAssetMenu(fileName = "GameConfig.asset")]
```

这样即可在Project窗口右键Create菜单直接创建一个资源出来，效果跟上面是一样的。

接下来还有最后一步，就是在游戏运行过程中使用这些数据，这个直接通过

```csharp
Resources.Load<GameConfig>("asset path");
```

来访问即可（MonoBehaviour加载后还需要实例化）。当然最好还是包装个单例出来：

```csharp
private static GameConfig s_instance;
public static GameConfig getInstance()
{
    if (s_instance == null)
        s_instance = Resources.Load<GameConfig>(PrefabPath);
    return s_instance;
}
```

至此整个环节就全部通了。可以发现，用作配置文件的话，ScriptableObject比MonoBehaviour还是方便和专业了很多的。

ok，这次的内容就介绍到这里。

附：完整代码（Unity5.3测试通过）：

```csharp
using System;
using UnityEngine;

namespace Demo
{
    [CreateAssetMenu(fileName = "GameConfig.asset")]
    public class GameConfig: ScriptableObject
    {
        public string GameName;
        public string Version;
        public string UpdateUrl;

        public bool MyConfig1;
        public int MyConfig2;

        private static GameConfig s_instance;
        public static GameConfig getInstance()
        {
            if (s_instance == null)
                s_instance = Resources.Load<GameConfig>(PrefabPath);
            return s_instance;
        }
        private const string PrefabPath = "GameConfig";
    }
}
```
