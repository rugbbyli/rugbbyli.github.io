---
layout: post
title:  "Unity3D学习笔记：5，动画"
date:   2015-03-25
categories: Unity
tags: Unity StudyLine
---

###动画
从4.0开始，Unity推出了新的动画系统（称作MecAnim），功能更加强大，用起来也更加简单和便利。由于新版动画系统兼容旧版，在它的基础上进行了扩充，因此本文首先介绍新版动画系统，最后指出旧版的工作流程。<br>
首先介绍几个概念：<br>
AnimationClip：包含了具体的动画信息，即物体随时间的属性（位置/旋转等）变化数据记录。这个可以是Unity生成的，也可以是外部动画工具创建并导入的。<br>
AnimatorController：以“状态机”的形式组织和管理AnimationClips，控制当前播放的AnimationClip，以及如何在不同的Clip间转换。<br>
Avatar：Unity提供的一项功能，可以将一个模型的动画重定向到另一个模型，只要它们有类似的外观（类人）。这样当项目中有许多动作一样但外形不同的类人模型时，可以节省大量的动画制作成本，且管理起来更加方便。<br>
Animator：新版动画组件。上面提到的东东都通过Animator统一附加到物体上。具体来讲，Animator包含了AnimatorController的引用，以及（如果播放的是类人动画）模型要使用的Avatar。而AnimatorController包含了若干AnimationClips的引用。<br>
Unity内置了一个动画编辑器，可以方便的进行动画制作。动画本质上就是物体某个属性随时间的变化，因此理论上来讲，使用这个内置的编辑器可以制作任何动画。不过从效率方面考虑，过于复杂的3d模型动画最好还是由动画设计师通过第三方工具生成，然后导入使用。。<br>
选中场景中某个物体后，通过『Window』->『Animation』或按下Ctrl+6可以打开Animation窗口。它会列出物体当前附加的全部AnimationClip，并可以进行编辑（不是外部导入的）、预览等操作。下图是一个外部导入的模型的行走动画的数据，可以看到整个动画是由身体不同部位的上百个旋转/缩放/平移动作等组合而成的。<br>
{% include img.html param="unity_skill_animation_wnd.png" %}
选择【create new clip】可以生成新的AnimationClip并添加到AnimatorController里面，AnimationClip会保存为.anim格式的文件。然后点击【Add Property】，选择要进行动画的属性，比如【Transform.Scale】，添加进来。通过时间轴调节不同时期属性的值，就可以实现物体的动画。比如下图，添加了一个缩放动画，Scale值先从1.0到1.2平滑过渡，再原样缩小：<br>
{% include img.html param="unity_skill_animation_wnd_2.png" %}
从外部导入的动画一般是3d模型格式的。虽然不能编辑外部导入的动画，但可以进行一些调整。从Project窗口选中一个模型，然后在Inspector窗口切换到Animations选项卡，如果模型有附带动画，会在这里列出来。如下图，展示了一个模型附带了7个动画片段。在这里可以进行分割动画、调整动画开始和结束帧、预览动画、设置关键帧回调、屏蔽某些部位动画等操作。
{% include img.html param="unity_skill_animation_import_1.png" %}
接下来展示一下通过Avatar如何快捷地重定向模型动画到别的模型。首先我准备了3个人物模型（都是在AssetStore下载的免费资源），一个美少女自带了行走的动画，一个Robot无动画，一个抠脚大汉无动画。当然啦，我要让他们俩照着美少女的动作动起来~<br>
1：在Project窗口中找到这3个模型文件，选中后在Inspector窗口选择Rig选项卡，将AnimationType改为Humanoid，点击Apply，Unity会自动帮我们生成角色的Avatar；<br>
{% include img.html param="unity_skill_animation_avatar_1.png" %}
2：创建一个AnimatorController（『Project』->『Create』->『Animator Controller』），命名为animdemo，打开Animator窗口，将美少女的行走动画拖进窗口（模型动画一般在模型的子级）；<br>
3：将3个模型都拖放到场景中排列开，面对摄像机露出微笑；<br>
{% include img.html param="unity_skill_animation_avatar_2.png" %}
4：分别给它们添加Animator组件，Controller指定为animdemo，Avatar指定为自身的Avatar；<br>
现在运行游戏，3个人快乐地走起猫步：<br>
{% include img.html param="unity_skill_animation_avatar_3.gif" %}
这里面有什么猫腻呢？个人感觉跟Avatar有很大关系。Avatar抽取了人形生物共有的身体部位（头/肩/腿/脚等），然后将动画映射到Avatar的这些关键部位上（比如头如何转/腿如何摆动等），这样，就实现了动画跟模型分离（动画控制Avatar，Avatar控制模型）。当两个模型的Avatar一样时，动画自然就可以共用了。这也是为何官方强调Avatar系统的使用对象一定要是人生生物。可以猜测，即使不是人形生物，只要我们配置好Avatar，一样可以共用别的人物模型的动画。我拿一只白猫做了测试：<br>
{% include img.html param="unity_skill_animation_avatar_4.gif" %}
虽然看起来有些诡异，但确实验证了我的推测。<br>
上面介绍的都是单个动画的情景。当角色需要多个动画时，AnimatorController的优势就体现出来了。AnimatorController引入了状态机的概念来管理动画的切换。每个动画都被视作一个状态，当某个状态被激活时，对应的动画就开始播放。同一时刻只会有一个激活的状态。通过AnimatorController，可以设置各个状态之间切换的条件和对象。来看一个具体的例子。<br>
还使用上面的白猫模型。它自带有几个动画，如下图所示：<br>
{% include img.html param="unity_skill_animation_controller_1.png" %}
1：创建一个AnimatorController，选中它，然后打开Animator窗口。把所有的动画片段拖拽到Animator窗口中，会自动生成分别以它们命名的各个状态。在『Idle』状态上右键，选择『Set as default state』将它设置为默认状态。<br>
2：现在各个状态间是孤立的。在想要互相切换的状态上右键选择『Make Transition』，会出现一个箭头线，将它设置在想要切换的状态上。由A状态连接到B状态的箭头代表可以由A状态切换到B状态，没有连接的状态一般是不能互相跳转的，除了『AnyState』。『AnyState』是一个内置的特殊的状态，它代表“任何状态”。如果从『AnyState』连接箭头到状态A，表示可以从任何状态跳转到状态A。我们规定一下猫咪的行为：『Idle』可以转到『IdleSit』、『Walk』和『Itching』，『Walk』和『Jump』可以互转，除了『Jump』别的状态都可以转到『Idle』，任何状态都可以转到『Meow』。于是最终设置的结果如下：
{% include img.html param="unity_skill_animation_controller_2.png" %}
3：添加了箭头只是表示可以切换，下面添加切换条件（如果没有添加条件，默认是播放完动画直接切换）。状态切换是通过对参数的判断进行的，判断成立就会切换。可以添加4种类型的参数，分别是Float/Int/Bool和Trigger。不同类型的参数的判断有所不同，Float型可以判断『大于/小于』两种条件，Int型可以判断『大于/小于/等于/不等于』四种条件，Bool型的条件有『为真/为假』，Trigger比较特殊，无条件判断，是一次性的触发。分析可以发现，IdleSit和Walk都是持续性的行为，而Itch/Jump和Mew则是一次性的。所以我们添加5个参数：bool isSet，bool isWalk，trigger itch，trigger jump，trigger mew。点击Idle到IdleSit的箭头，在Inspector窗口的Conditions下添加一个条件。选择参数『isSit』，条件选择“true”。这样，当isSit变为true时，如果当前状态是Idle，就会切换为IdleSit。按同样的方式设置每个箭头的条件（有些切换是无条件的，比如当参数『jump』被触发时，如果当前状态是Walk，应当切换到Jump，但是Jump切换到Walk则是无条件的，当Jump动画播放完应当自动切换过去，这时候就无需设置Conditions）。<br>
4：设置完成了，看下如何通过代码控制状态切换。新建一个测试场景，将白猫模型拖到场景中，为它添加Animator组件，Animator的Controller设置为第一步创建的。新建一个脚本，命名为catAnimDemo，并挂载到白猫身上。在catAnimDemo中添加如下代码：<br>

```csharp
public class catAnimDemo : MonoBehaviour {
    Animator animator;

	void Start () {
        animator = GetComponent<Animator>();
	}

    void OnGUI() {
        var rect = new Rect(20, 20, 100, 30);
        if (GUI.Button(rect, "空闲"))
        {
            animator.SetBool("isSit", false);
            animator.SetBool("isWalk", false);
        }
        rect.Set(20, 60, 100, 30);
        if (GUI.Button(rect, "坐下"))
        {
            animator.SetBool("isSit", true);
        }
        rect.Set(20, 100, 100, 30);
        if (GUI.Button(rect, "挠痒"))
        {
            animator.SetTrigger("itch");
        }
        rect.Set(20, 140, 100, 30);
        if (GUI.Button(rect, "走几步"))
        {
            animator.SetBool("isWalk", true);
        }
        rect.Set(20, 180, 100, 30);
        if (GUI.Button(rect, "跳"))
        {
            animator.SetTrigger("jump");
        }
        rect.Set(20, 220, 100, 30);
        if (GUI.Button(rect, "叫"))
        {
            animator.SetTrigger("mew");
        }
    }
}
```

借助Animator类的『SetFloat/SetInt/SetBool/SetTrigger』即可设置动画状态机的参数值，然后状态机会自动根据值的改变判断条件，并切换状态。运行游戏，点击每个按钮，看动画切换是否正常。也可以试一下异常情况，比如在IdleSit状态点击『行走』按钮，可以发现参数isWalk被设置为true了，但是状态没有切换到Walk，就是因为IdleSit状态和Walk状态之间没有设置转换箭头的原因。<br>
除了管理动画状态间的切换，AnimatorController还可以管理多个Layers（层）。Layer主要用于物体不同部位不同动画的分离（比如人物上半身和下半身动画的分离）。不同Layer之间可以是完全隔离或者叠加，每个Layer可以设置自己的AvatarMask以屏蔽某些部位的动画。<br>
从5.0版本起，可以在动画状态上添加『StateMachineBehaviour』脚本，通过重写特定的方法，在State触发特定状态（进入/退出/更新等）时得到回调，从而更加便捷地管理动画。<br>
此外，Animator还有别的一些管理动画的方法，如获取当前正在播放的动画，获取每个参数的值，强制切换到某个状态等，就不再一一介绍了。<br>
最后介绍下旧版的动画系统。新版和旧版最大的区别就是动画的管理，旧版由于没有AnimatorController，因此所有的动画切换逻辑都需要自己去管理。通过在物体上挂载Animation组件，然后将物体的AnimationClips都拖放到Animations集合中，然后在脚本中通过Animation.Play/Animation.CrossFade等方法播放和切换动画。