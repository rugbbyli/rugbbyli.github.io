---
layout: post
title:  "Unity3D学习笔记：6，脚本"
date:   2015-04-05
categories: Unity
tags: Unity StudyLine
---

###脚本
即使Unity已经提供了许多组件简化游戏开发中，脚本仍然是不可或缺的一部分。脚本是游戏的灵魂，它控制其它组件的良好运行，控制游戏流程的发展，对用户输入做出反馈，体现游戏的整体逻辑等。<br><br>
Unity使用CSharp（符合标准csharp规范）和UnityScript（以javascript为原形，进行了一些调整以更适合unity使用）作为脚本语言，以『.Net Framework』作为运行环境。由于微软标准的.Net框架只适配windows平台（目前官方.Net也已开源，且未来将会适配别的操作系统平台），因此Unity使用『Mono（第三方的开源跨平台.Net实现）』来实现跨平台的需求。得益于.Net的CLR设计，所有.Net语言最终都编译成同样的IL语言，因此不同语言提供完全一样的功能，可以根据喜好随意选择一种或混合使用。此外，由于运行于.net平台之上，因此所有能够编译成IL的语言都可以为Unity所用（比如通过visual studio和托管c++编写的类库）。<br>
Unity内部提供了一套完整的api供脚本调用。前面有提到过，脚本要想生效，需要作为组件挂载到某个GameObject上面。而要想作为组件挂载到GameObject，脚本必须满足如下几个条件：<br>
1，脚本中必须有一个类继承自UnityEngine.MonoBehaviour，后者是Unity api中提供的类型；<br>
2，脚本的名称（排除后缀名）必须与上面定义的那个类同名；<br>
注意，UnityScript的语法跟CSharp不同。UnityScript无需在脚本中定义类型，直接实现方法即可，Unity会在编译时自动生成以文件名命名的且继承自MonoBehaviour的类型。本系列博客中除非特别指出，都使用CSharp作为脚本语言，不再累述。<br><br>
当脚本被挂载到GameObject上时，会自动创建一个脚本中定义类型的实例，然后被序列化以存储。序列化是Unity广泛使用的机制，也是非常核心的机制。Unity的许多特性都构建于序列化之上，包括但不限于：<br>
Object：所有继承自UnityEngine.Object的类型都可以被序列化；<br>
Prefab：当我们将一个GameObject保存为Prefab时，实际上是将这个物体（及其附加的组件）序列化并保存在文件中。<br>
Scene：所有的场景都是通过序列化机制，将场景中全部的GameObject保存在文件中。<br>
Instantiation：我们可以通过调用Instantiate方法在运行时实例化新的Prefab或GameObject等，它背后的工作过程是：先序列化要被实例化的源对象，然后创建新的目标对象，然后将之前序列化的数据反序列化到目标对象上。<br>
InspectorWindow：当我们将脚本附加在物体上后，在InspectorWindow可以查看到脚本的一些公共变量等。实际上此处显示的内容正是脚本被序列化后的数据。这也正是“要想将私有变量也显示出来，需要将它标记为[SerializeField]”的原因。同样的，当我们在InspectorWindow修改脚本的参数值，同样会序列化保存到文件中。<br>
Unity的序列化机制为了保证良好的性能，技术上并没有采用CSharp的序列化方案，因此在行为上与CSharp的序列化有所不同，有些需要注意的细节。比如，哪些类型可以被序列化呢？<br>
1，Unity内置类型都可以被序列化；<br>
2，CSharp简单数据类型可以被序列化（int/float/bool/string等）；<br>
3，继承自UnityEngine.GameObject的自定义类型；<br>
4，有[Serializable]标记的且不为abstract的自定义类型；<br>
5，储存元素是可序列化对象的集合（包括Array和List<T>）；<br>
另外，对象中的成员变量要想被Unity自动序列化，需要满足几个条件：<br>
1，不能是static/const/readonly的；<br>
2，需要是public的，或者有[SerializeField]标记的；<br>
3，需要是可序列化的类型；<br>
此外，还有诸如不能处理多态、不能处理自定义类型的空引用、多个字段引用同一个对象，反序列化后会成为多个对象等问题。如果实际需求中遇到这些，需要注意避开这些问题。比如可以考虑自己实现序列化和反序列化对象的逻辑，Unity提供了这样的接口，具体可以参考『ISerializationCallbackReceiver』接口。<br><br>
接下来我们看一下脚本的体系结构。前面介绍过，脚本是需要继承MonoBehaviour类型的，而实际上它也是继承自别的类型，整个继承体系如下：<br>
{% include img.html param="unity_skill_script_monobehaviour.png" %}
因此，我们在脚本中就可以自由调用上面那些基类提供的实用方法，来完成自己的工作。等等，那么脚本究竟要如何运行起来呢？我们在脚本中定义的方法，如何执行呢？<br>
我们知道Unity采用单线程来运行游戏逻辑。在线程内部维护了一个循环，以间歇触发每个脚本的执行。Unity预定义了游戏过程中的一系列事件（如初始化/物理更新/输入等），并允许脚本针对每个事件设置回调方法，这样当对应的事件发生时，Unity会调用对应脚本设置的回调方法。设置回调方法很简单，只需要在脚本中声明Unity规定名称的方法即可。Unity会寻找脚本中是否存在特定名称的方法，并自动调用它。举例来说，如果在脚本中声明Update方法，则Unity会在每帧调用这个方法。关于详细的Unity会进行回调的方法列表，请参考[这里](http://docs.unity3d.com/ScriptReference/MonoBehaviour.html "这里")。<br>
除了系统事件外，脚本也可以声明自己的事件，别的物体可以设置监听对应的事件，以在某个时机得到通知。CSharp的开发者应该对此比较熟悉，可以通过CSharp内置的事件机制来实现。此外，Unity内置了一个封装好的事件系统，位于UnityEngine.Events命名空间下。使用这一系统的好处在于，可以通过编辑器，在Inspector窗口方便地设置事件的监听者，而无需改动代码。<br>
下图展示了Unity整个游戏流程以及各个系统事件在流程中所处位置及回调方法的回调时机：<br>
![image](http://docs.unity3d.com/uploads/Main/monobehaviour_flowchart.svg)
可以看到，整个流程主要分成3大块，分别是初始化、更新和销毁。初始化和销毁都是一次性事件，更新是一直循环进行的，每执行一次更新，主要完成了包括物理引擎更新、输入、游戏逻辑更新、屏幕渲染更新等事件。每完成一次更新称作一帧，每秒钟能够完成的更新次数称作帧率。由于每帧会刷新屏幕显示，因此相应地，帧率越高，游戏画面显示看起来就越平滑。对于游戏来讲，最好能至少保持帧率在30以上，才能让玩家体验良好。<br><br>
前面讲过Unity是单线程执行，因此提高帧率的关键就在于不要在每次更新中执行太多代码。Unity提供了协程模型来帮助将一个任务分散到多帧中执行，通过使用协程，可以简化游戏逻辑和代码可读性。协程利用了CSharp语法中的迭代器技术，以保证方法中的代码可以“暂时返回，等下次调用时继续从暂停处执行”。通过MonoBehaviour类提供的StartCoroutine方法，可以启动一个协程，下面是一个例子，它会在一定时间内让物体渐变为透明：<br>

```csharp
IEnumerator Fade() {
    for (float f = 1f; f >= 0; f -= 0.1f) {
        Color c = renderer.material.color;
        c.a = f;
        renderer.material.color = c;
        yield return null;
    }
}

void Update() {
    if (Input.GetKeyDown("f")) {
        StartCoroutine("Fade");
    }
}
```
可以想象，如果不使用协程，我们必须在每次Update中执行一次for循环中的代码，如果类似的功能过多，会造成代码逻辑混乱，可读性差等问题。<br><br>
需要注意，由于各种因素，帧率并不是一个稳定的数值，也就是说，每秒钟可以执行的更新次数是会变化的。因此，代码控制的游戏物体更新（比如位移的移动）不能基于帧进行，而要基于时间。举例来说，如果使用下面的代码控制物体移动：<br>

```csharp
void Update() {
    transform.Translate(0, 0, distancePerFrame);
}
```
那么如果帧率波动过大，会发现物体移动明显会忽快忽慢。解决方案就是改为基于时间的速度控制。Unity提供了Time类来获取与游戏时间相关的一些东西，比如Time.deltaTime表示上一帧执行所用的时间。利用这个参数改进代码：<br>

```csharp
void Update() {
    transform.Translate(0, 0, distancePerSecond * Time.deltaTime);
}
```
这样就能保证即使帧率波动，每秒物体移动的距离也是固定的。<br>
此外，与逻辑更新不同的是，物理引擎的更新则是固定频率进行的，这是为了保证各种物理模拟和计算的准确。也因此，物理更新和逻辑更新并不是同步的。在编辑器中选择『Edit』->『Project Settings』->『Time』在Inspector中可以设置『Fixed Timestep』的值，即是物理引擎的更新频率（可以在代码中通过Time.fixedDeltaTime取得它的值）。<br>
Time类还有个很有用的属性叫做timeScale，它可以设置“时间流逝比例”。比如说，设置Time.timeScale = 2，可以让游戏时间以2倍速流逝。甚至可以设置Time.timeScale = 0，达到时间停滞的效果。请注意仅仅是“时间”的速度变慢了，我们游戏实际上执行速度还是不变的（比如帧率）。比如前面的物体移动，如果我们是使用distancePerSecond * Time.deltaTime来计算每帧移动距离，那么当时间“变化”，我们的计算结果也会相应变化，反映到游戏中，就是游戏整体效果的变化了。这也是涉及到值改变都使用时间系数进行计算的原因：可以很方便地实现类似“暂停”或“慢速”等效果。<br><br>
由于Unity是跨平台的，很多时候我们会需要针对不同的目标平台编写不同的代码。除了使用条件分支在代码中判断外，更加高效的方法是使用条件编译。使用预编译符号即可达到这一目的，比如以下代码会在编译时判断预编译符号是否定义，如果定义了才把代码编译：<br>

```
function Awake() {
  #if UNITY_EDITOR
    Debug.Log("Unity Editor");
  #endif
    
  #if UNITY_IOS
    Debug.Log("Iphone");
  #endif

  #if UNITY_STANDALONE_OSX
    Debug.Log("Stand Alone OSX");
  #endif

  #if UNITY_STANDALONE_WIN
    Debug.Log("Stand Alone Windows");
  #endif    
}
```

Unity提供了一些预编译符号，以帮助开发者针对不同平台编译不同的代码。涉及到目标平台相关的预编译符号如下：<br>

    UNITY_EDITOR            Define for calling Unity Editor scripts from your game code.
    UNITY_EDITOR_WIN        Platform define for editor code on Windows.
    UNITY_EDITOR_OSX        Platform define for editor code on Mac OSX.
    UNITY_STANDALONE_OSX    Platform define for compiling/executing code specifically for Mac OS (This includes Universal, PPC and Intel architectures).  
    UNITY_STANDALONE_WIN    Use this when you want to compile/execute code for Windows stand alone applications.
    UNITY_STANDALONE_LINUX  Use this when you want to compile/execute code for Linux stand alone applications.
    UNITY_STANDALONE        Use this to compile/execute code for any standalone platform (Mac, Windows or Linux).
    UNITY_WEBPLAYER         Platform define for web player content (this includes Windows and Mac Web player executables).
    UNITY_WII               Platform define for compiling/executing code for the Wii console.
    UNITY_IOS               Platform define for compiling/executing code for the iOS platform.
    UNITY_ANDROID           Platform define for the Android platform.
    UNITY_PS3               Platform define for running PlayStation 3 code.
    UNITY_PS4               Platform define for running PlayStation 4 code.
    UNITY_XBOX360           Platform define for executing Xbox 360 code.
    UNITY_XBOXONE           Platform define for executing Xbox One code.
    UNITY_BLACKBERRY        Platform define for a Blackberry10 device.
    UNITY_WP8               Platform define for Windows Phone 8.
    UNITY_METRO             Platform define for Windows Store Apps (additionally NETFX_CORE is defined when compiling C# files against .NET Core).
    UNITY_WINRT             Equivalent to UNITY_WP8 |UNITY_METRO
    UNITY_WEBGL             Platform define for WebGL.

此外还可以针对不同的Unity版本条件编译不同的代码。Unity每个版本都会新增当前版本的预编译符号，比如当前版本是5.0，则符号是UNITY_5_0，以此类推。版本判断经常用在编辑器插件扩展里面。<br>
当然了，你还可以定义自己的预编译符号。定义当前文件的预编译符号可以直接在文件最上方通过『#define xxxx』来定义，定义针对某平台的全局符号，可以在编辑器中选择『Edit』->『Project Settings』->『Player』，在Inspector窗口中展开『Other Settings』选项，在『Scripting Define Symbols』下方输入要定义的符号即可。<br>