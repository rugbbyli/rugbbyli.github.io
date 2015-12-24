---
layout: post
title:  "Unity3D foreach 隐藏的深坑"
date:   2015-12-24
categories: Unity
tags: Unity bugs
---

###背景：
今天下午，就在我快（忧）乐（伤）地敲着代码时，突然发生了一件悲痛的事。<br>
当时情况是这样的。<br>
我有个十分简单的需求，读取某个模型身上的全部动画片段，每个动画对应生成一个按钮，点击这个按钮就播放对应的动画，没错，就是人物动画预览。于是信手拈来：<br>

```csharp
        List<string> actions = new List<string>();
		
		//read model actions...

        foreach(string act in actions)
        {
            var btn = Instantiate(btnPrefab) as Button;
            btn.GetComponentInChildren<Text>().text = act;
            btn.onClick.AddListener(() => PlayAnimClip(act));
            btn.transform.SetParent(transform);
        }
```

真的很简单对不对！一分钟就能搞定对不对！走你，点运行。<br>
屏幕上出现了一排按钮，每个都显示对应动画的名字，一切看起来都很顺利，直到我点下其中一个按钮……<br>
What？怎么人物还是一动不动的戳在屏幕中间？我点点点，几个按钮点过去，还是纹丝不动。<br>
嘿我这暴脾气，当时就怒了。切回代码反复检查了几遍，没发现什么问题啊？加上log继续运行，问题来了。<br>
虽然每个按钮显示的文字是正确的，但是点击回调时，传进的字符串都是同一个！WTF！这不科学啊！<br>


###分析：
根据代码推测，最大的嫌疑就只有一个了，就是foreach里面的变量act在整个foreach循环中共用了同一个变量。后果就是每个匿名方法捕获的外部变量实际上变成了同一个，而它最终的值，就是最后一次循环时的值了。运行结果也验证了这一点……<br>
可这明显跟我一直以来的认知不合。在我的印象中，foreach的临时变量应该是在每个循环体内声明，那么每个循环都有一个新的变量被构造才对。此刻我的内心是崩溃的，难道这样基础的地方我都理解错了吗？还是说，只是mono的实现和微软的有所不同？<br>
我决定做个实验验证一下，在Unity和VS Console环境下用同样的代码运行，看结果是否一致。为了排除其它干扰，我简化了测试代码，新的代码如下：<br>

```csharp
		List<Action> acts = new List<Action>();

        int[] nums = { 0, 1, 2, 3, 4 };

        foreach (int i in nums)
        {
            acts.Add(() => Debug.Log(i)); // replace "Debug.Log" with "Console.WriteLine" in VS Console
        }

        foreach (var act in acts)
        {
            act();
        }
```

然后在Unity运行，结果依然是『4,4,4,4,4』。然而当环境变为VS时，戏剧性的一幕出现了，控制台漂亮地打出了『0,1,2,3,4』。<br>
现在几乎可以确定是不同编译器的实现问题了。最后，将它们生成的dll分别反编译来看下，果然：<br>

```csharp

	//mono:
	List<RotateController.Action> list = new List<RotateController.Action>();
	int[] array = new int[]
	{
		0,
		1,
		2,
		3,
		4
	};
	int[] array2 = array;
	int i;
	for (int j = 0; j < array2.Length; j++)
	{
		i = array2[j];
		list.Add(delegate
		{
			Debug.Log(i);
		});
	}
	using (List<Action>.Enumerator enumerator = list.GetEnumerator())
	{
		while (enumerator.MoveNext())
		{
			Action current = enumerator.get_Current();
			current();
		}
	}
	
	
	//vs
	List<Action> acts = new List<Action>();
	int[] nums = new int[]
	{
		0,
		1,
		2,
		3,
		4
	};
	int[] array = nums;
	for (int j = 0; j < array.Length; j++)
	{
		int i = array[j];
		acts.Add(delegate
		{
			Console.WriteLine(i);
		});
	}
	foreach (Action act in acts)
	{
		act();
	}

```

可以看到，它们都将foreach实现为for循环（这里不同的集合可能会有不同的实现，比如List<T>会被实现为while循环），但非常重要的区别是，mono将循环变量声明在循环外部，而vs则声明在内部。这种差异从而引发了开头提到的问题。<br>


###结语
这只是一个简单的问题，且很容易查证。但不得不由此联想到的是，mono中还有多少地方是像这样跟标准实现不一致的呢？会不会有些习以为常的写法，却产生意想不到的作用呢？尤其是对于熟悉c#开发的同学来说，会不会导致无意中引入一些诡异的bug呢？<br>
请切记这点，尽量少一些经验主义。<br>

