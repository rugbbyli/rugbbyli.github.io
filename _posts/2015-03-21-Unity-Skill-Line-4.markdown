---
layout: post
title:  "Unity3D学习笔记：4，声音"
date:   2015-03-21
categories: Unity
tags: Unity StudyLine
---

###声音
Unity的声音系统模拟了现实中的情形，分为两个部分：AudioSource和AudioListener。很直观的字面理解，AudioSource发出声音，AudioListener接收前者的声音并播放出来。
![image](/imgs/unity_skill_line_audio_1.png)
一个场景中要有一个AudioListener，才能听到AudioSource发出的声音。Unity会根据两者的距离、声音传播速度等条件计算声音的衰减（可以自定义衰减曲线）和最终输出。<br>
AudioSource可以播放的对象称作AudioClip，它包含了音频文件数据，可以通过标准的资源管理方式导入音频文件（支持 .aif, .wav, .mp3, .ogg格式）生成。可以在Inspector窗口添加AudioSource默认播放的AudioClip，还可以通过脚本更改AudioSource播放的AudioClip。比如一个怪兽，可能有“攻击/受伤/死亡”等多种声音，只需要添加一个AudioSource，然后不同时期通过脚本指定播放具体的AudioClip即可。<br>
为了确保声音系统正常工作，一个场景中最好只存在一个AudioListener（虽然Unity不会强制禁止添加多个）。AudioSource和AudioListener的摆放位置很重要，它们决定了场景音效的逼真程度。一般情况下，AudioSource附加在发声的物体上，比如场景中的音响/怪物等，而AudioListener附加在主角身上或者主摄像机上。当然，根据实际情况，可能需要多次调节才能确定最佳摆放位置。<br>
此外还有一些实用的辅助组件，比如：<br>
AudioReverbFilter：处理声音，附加各种混响特效，模拟不同的空间感（大多数音乐播放器都会有的功能）；<br>
AudioDistortionFilter：使得声音产生畸变，比如用来模拟低质量的无线电广播等；<br>
AudioChorusFilter：模拟合唱的声音效果；<br>
AudioEchoFilter：模拟回声；<br>
AudioReverbZone：类似AudioReverbFilter，作用于某个空间范围；<br>
AudioHighPassFilter：过滤掉低音，通过高音；<br>
AudioLowPassFilter：过滤掉高音，只通过低音；<br>
通过这些组件的配合使用和参数的精心调节，可以打造出令人赞叹的游戏音乐体验。<br>
<br>
Microphone类可以调用运行设备上的音频输入设备采集音频数据。它不是以组件形式提供，要使用需要通过脚本调用。下面的代码示例了采集语音数据然后直接播放出来：<br>

{% highlight csharp %}
AudioSource audioSource = GetComponent<AudioSource>();
audioSource.clip = Microphone.Start(null, true, 10, 44100);
audioSource.Play();
{% endhighlight %}