---
layout: post
title:  "Unity学习笔记：“粉碎糖果”（3）"
date:   2014-12-22
categories: Unity
tags: Unity Demo
---

###使用”对象池“优化内存波动：

对象池听上去似乎是个高大上的东东，其实这玩意也没什么复杂的。本节我们就来简单了解下它的原理和实现过程，并将它用到我们刚刚完成的游戏上。<br>
<!-- more -->
对象池，顾名思义就是对象的池子。类似的概念有线程池等。它们的原理类似，就是接管目标的创建和回收过程。具体到对象池就是，当我们需要创建对象时，直接从池子中拿现成的就ok；用完需要释放时，直接丢回池子就完事。<br>
那么，为何要这么麻烦用它呢？它有什么好处？说到这里，就要扯一下.net的垃圾回收了。我们知道，c#自带垃圾回收，当我们创建对象时，系统分配一定的内存空间以容纳对象的数据；释放对象后，系统在某个时机对这个对象的内存进行回收以便复用。由于回收的时机是不确定的，如果我们释放大量的对象，然后立刻创建新的对象，虽然对象的总数基本不变，但可能存在老的对象空间还没回收，又分配了新的空间这样的情况，这样就会造成程序使用内存量的波动。在移动平台上，如果内存波动过大，就有可能造成app的crash。同时，内存空间的分配/回收和整理，都会消耗系统资源，尤其是频繁的分配和回收，势必会影响到性能。这时候，对象池就派上用场了。因为对象池只是缓存对象，避免了对象的频繁创建和释放，所以可以减轻系统压力，减少内存波动。<br>
当然了，对象池并非有利无弊，整体来看，它肯定导致内存占用高出应有数值。所以使用场合要注意。一般情况下，对于游戏中可能频繁创建销毁的/比较小的对象，使用对象池是比较合理的选择。<br>
下面我们来看一下，如何实现一个简单的对象池，并对我们的Candy使用它。<br>

###简单的对象池实现

我打算使用一个Dictionary结构来保存池中的对象，用string类型的key来区分不同类别的对象，字典中的每个键对应的值都是一个队列结构。当调用者申请新对象时，我们先在池中寻找同类对象，存在则移出队列并返回，否则根据需要创建新的对象或者返回null。当调用者申请释放对象时，我们直接将对象插入对应的队列中。代码如下：<br>

{% highlight csharp %}
public class ObjectPool : MonoBehaviour
{
    public GameObject[] _prefabs;

    Dictionary<string, GameObject> _prefabsPool;
    Dictionary<string, Queue<Object>> _objectPool;

    static ObjectPool _instance;
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("ObjectPool");
                _instance = obj.AddComponent<ObjectPool>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        _objectPool = new Dictionary<string, Queue<Object>>();

        _prefabsPool = new Dictionary<string, GameObject>();

        if (_prefabs != null && _prefabs.Length > 0)
        {
            foreach (var perfab in _prefabs)
            {
                _prefabsPool.Add(perfab.name, perfab);
            }
        }
    }

    Queue<Object> GetPool(string ObjectID)
    {
        if (!Instance._objectPool.ContainsKey(ObjectID))
        {
            var queue = new Queue<Object>();
            Instance._objectPool.Add(ObjectID, queue);
            return queue;
        }
        else
        {
            return Instance._objectPool[ObjectID];
        }
    }

    public static Object Instantiate(string ObjectID)
    {
        Queue<Object> queue = Instance.GetPool(ObjectID);
        
        Object obj = null;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else if(Instance._prefabsPool.ContainsKey(ObjectID))
        {
            obj = Instantiate(Instance._prefabsPool[ObjectID]);
            queue.Enqueue(obj);
        }

        if (obj && obj is GameObject)
        {
            (obj as GameObject).SetActive(true);
        }

        return obj;
    }

    public static void Destroy(Object Obj, string ObjectID = null)
    {
        if (Obj is GameObject)
        {
            (Obj as GameObject).SetActive(false);
        }

        if (string.IsNullOrEmpty(ObjectID))
        {
            ObjectID = Obj.name;
        }

        Instance.GetPool(ObjectID).Enqueue(Obj);
    }

    public static void Destroy(MonoBehaviour behaviour, string ObjectID = null)
    {
        Destroy(behaviour.gameObject, ObjectID);
    }
}
{% endhighlight %}

这是一个很初级的对象池实现，虽然很简单，不过已经足以体现对象池的核心思想。在这里我让它继承了MonoBehaviour，是为了能够将它挂载到场景的物体上，从而可以拖动添加若干Prefab作为对象的拷贝源。这不是必须的，但某些时候会很有用。另外，虽然我们使用了类似单例模式的写法，事实上，我们无法阻止创建多个ObjectPool实例。如果你想实现这一点，可能需要考虑别的方案，比如不从MonoBehaviour继承，然后对它实现单例模式。或者，在你的项目规范文档中明确指出不允许创建多个实例等。<br>

###改造Candy的创建和回收

接下来，我们修改一些代码，以利用到刚刚创建出来的新东西。首先是Candys里面的NewCandy方法，改成这样：

{% highlight csharp %}
    var obj = ObjectPool.Instantiate("Candy") as GameObject;
    if (obj == null) obj = Instantiate(CandyPrefab) as GameObject;
    var candy = obj.GetComponent<Candy>();
    candy.SetImage(CandyImgs[Random.Range(0, CandyImgs.Length)]);
    return candy;
{% endhighlight %}

对NewCandyExclude也使用同样的方法修改。<br>
然后是销毁Candy的地方，它位于CandyManager类的CheckCandy方法中。找到<br>

    Destroy(p);

改为

    (p.transform as RectTransform).anchoredPosition = Vector2.up * transform2D.rect.height;
    ObjectPool.Destroy(p, "Candy");

即可。在回收前，我们改变了candy的默认位置。<br>
怎么样，挺简单的吧？

###小结
本节讨论了内存优化的一种方法：对象池。完善的对象池实现应该考虑更多的一些东西，比如池子的容量等。<br>

至此，这个主题已经全部完成了。但是，对于一个完整的游戏，这才仅仅是开始。还有许多细节部分需要去完善，消除时的粒子效果/音效/分数系统等，对于一款好玩的游戏都是不可或缺的，请记住，细节决定成败。<br>
