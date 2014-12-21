---
layout: post
title:  "Unity学习笔记：“粉碎糖果”（2）"
date:   2014-12-19
categories: Unity
tags: Unity Demo
---

###游戏的核心数据结构和算法：

首先，我们新建一个脚本，命名为Candy，用来存储每个Candy的行列值等信息，然后将它挂载到上一节创建的Prefab上。内容如下：

{% highlight csharp %}
public class Candy : MonoBehaviour,IPointerClickHandler  {

    Image image;
    public byte Row { get; private set; }
    public byte Column { get; private set; }

	void Awake () {
        image = GetComponent<Image>();
	}

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetPosition(byte row, byte column)
    {
        Row = row;
        Column = column;
    }

    public void SetSelect(bool select)
    {
        image.color = select ? new Color(.5f,.5f,.5f) : Color.white;
    }

	public bool ContentEquals(Candy other)
	{
		return image.sprite == other.image.sprite;
	}

    public string SpriteName
    {
        get
        {
            return image.sprite.name;
        }
    }

    public event System.EventHandler Click;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Click != null)
        {
            Click(this, System.EventArgs.Empty);
        }
    }
}
{% endhighlight %}

通过实现IPointerClickHandler接口，可以在我们点击Candy时触发回调事件。可以发现uGUI的事件系统用起来还是挺方便的。对于Candy的点击，我们自身无法处理，所以把事件包装分发出去。<br>


然后新建一个脚本，命名为Candys，用来储存Candy对象。我们使用二维数组进行存储。

{% highlight csharp %}
public class Candys : MonoBehaviour
{
    public Sprite[] CandyImgs;
    public GameObject CandyPrefab;

    public Candy[,] Data { get; private set; }
    int Rows;
    int Columns;

    /// <summary>
    /// 交换两个candy的位置
    /// </summary>
    public void Swap(Candy c1, Candy c2)
    {
        var r = c1.Row;
        var c = c1.Column;

        var tmp = Data[r, c];
        Data[r, c] = Data[c2.Row, c2.Column];
        Data[c2.Row, c2.Column] = tmp;

        c1.SetPosition(c2.Row, c2.Column);
        c2.SetPosition(r, c);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public Candy[,] Init(byte rows, byte columns)
    {
        Rows = rows;
        Columns = columns;
        Data = new Candy[rows, columns];
        for (byte r = 0; r < rows; r++)
        {
            for (byte c = 0; c < columns; c++)
            {
                var candy = NewCandy();
                Data[r, c] = candy;
            }
        }
        return Data;
    }

    Candy NewCandy()
    {
        obj = Instantiate(CandyPrefab) as GameObject;
        var candy = obj.GetComponent<Candy>();
        candy.SetImage(CandyImgs[Random.Range(0, CandyImgs.Length)]);
        return candy;
    }
}
{% endhighlight %}

然后对CandyManager进行修改，移除上一节的测试代码，添加InitCandys方法如下：

{% highlight csharp %}
    IEnumerator InitCandys(Candy[,] Data)
    {
        rows = Data.GetLength(0);
        columns = Data.GetLength(1);
        gridWidth = transform2D.rect.width / columns;
        gridHeight = transform2D.rect.height / rows;

        for (int r = rows - 1; r > -1; r--)
        {
            for (int c = columns - 1; c > -1; c--)
            {
                var candy = Data[r, c];
                candy.SetPosition((byte)r, (byte)c);
                candy.transform.SetParent(transform);
                candy.Click += Candy_Click;

                var rtf = candy.transform as RectTransform;
                rtf.anchoredPosition3D = Vector3.zero;
                rtf.anchoredPosition = new Vector2(c * gridWidth + candyMargin, (rows - r) * gridHeight - candyMargin);
                rtf.localScale = Vector3.one;
                rtf.sizeDelta = new Vector2(gridWidth - 2 * candyMargin, gridHeight - 2 * candyMargin);
                TweenPosition.Begin(candy.gameObject, 0.5f, new Vector2(c * gridWidth + candyMargin, -r * gridHeight - candyMargin)).method = UITweener.Method.EaseIn;
                yield return null;
            }
        }
    }
{% endhighlight %}

这与上一节的测试代码类似，不同的是Candy不是由我们生成，而是通过Candys返回。同时，我们也添加了candy的点击监听事件。<br>
接下来，在Start方法中调用InitCandys。现在它看起来像这样：

{% highlight csharp %}
	void Start () {
        transform2D = transform as RectTransform;

        Candys = GetComponent<Candys>();
        var candys = Candys.Init(6, 8);
        
        StartCoroutine(InitCandys(candys));
	}
{% endhighlight %}

然后我们实现相邻两个candy的交换效果。在CandyManager中增加如下方法：
{% highlight csharp %}
    private Candy _select;
    private void Candy_Click(object sender, System.EventArgs e)
    {
        var candy = sender as Candy;
        if (_select == null)
        {
            _select = candy;
            _select.SetSelect(true);
        }
        else if (_select == candy)
        {
            _select.SetSelect(false);
            _select = null;
        }
        else
        {
            var rdiff = Mathf.Abs(candy.Row - _select.Row);
            var cdiff = Mathf.Abs(candy.Column - _select.Column);
            //点击的不是相邻的两个点
			if (rdiff + cdiff != 1)
            {
                _select.SetSelect(false);
                _select = candy;
                _select.SetSelect(true);
            }
            else
            {
                StartCoroutine(CheckCandy(_select, candy));
                _select.SetSelect(false);
                _select = null;
            }
        }
    }
	
	private IEnumerator CheckCandy(Candy candy1, Candy candy2)
    {
        var animtime = 0.2f;
        TweenPosition.Begin(candy1.gameObject, animtime, (candy2.transform as RectTransform).anchoredPosition).method = UITweener.Method.Linear;
        TweenPosition.Begin(candy2.gameObject, animtime, (candy1.transform as RectTransform).anchoredPosition).method = UITweener.Method.Linear;
        Candys.Swap(candy1, candy2);
        
        yield return new WaitForSeconds(animtime);

		TweenPosition.Begin(candy1.gameObject, animtime, (candy2.transform as RectTransform).anchoredPosition).method = UITweener.Method.Linear;
		TweenPosition.Begin(candy2.gameObject, animtime, (candy1.transform as RectTransform).anchoredPosition).method = UITweener.Method.Linear;
		Candys.Swap(candy1, candy2);        
    }
{% endhighlight %}
现阶段我们并没有做消除判断，当点击相邻的两个点，我们使用平移动画交换他们的位置；0.2s后，再交换回来。<br>

###消除判断
接下来是这个游戏的核心算法之一：消除的判断。简单来讲就是，如何确定某个糖果和周围同样的糖果是否构成某个特殊的“形状”。在这个游戏中，我们只考虑“条状”，即相邻行或列的糖果达到3个以上。<br>
我们来分析下消除的过程。首先，玩家交换两个糖果的位置，然后，我们分别对每个糖果检查（你当然也可以遍历界面上的每个糖果进行检查，这是另外一种算法，本文不采用这种方法），看它是否与周围的糖果可以消除。如果可以，我们就消除它们，然后对上面的糖果下移，并在顶部补满糖果。迭代这个过程，直到全部位置有变化的糖果都被检查过。<br>
注意到，在两次消除之间，还有新增和移动糖果的操作。因此，我这里使用一种取巧的写法，将这个接口实现为迭代类型，对于使用者来说，他只需调用一次接口，然后对结果进行迭代，即可得到整个变动过程中消除的点的集合。我们在Candys中声明这个接口：

{% highlight csharp %}
    public IEnumerable<IEnumerable<Candy>> CheckDispel(Candy c1, Candy c2)
    {
        foreach (var ret in CheckDispel(c1.Row, c1.Column))
            yield return ret;
        foreach (var ret in CheckDispel(c2.Row, c2.Column))
            yield return ret;
    }
	
	    /// <summary>
    /// 检查某个点是否可以消除，如果可以，返回消除的点的集合
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    IEnumerable<List<Candy>> CheckDispel(byte row, byte column)
    {
        yield break;
    }
{% endhighlight %}

那么该如何进行检查呢？我打算采用这样一种思路：首先，我们统计出待检查点上下左右4个方向上与它一样（指糖果类型一样）且连续的点的个数（不包括它自身）。这步完成后，我们其实已经得到了这个点连通的同类点的整体形状的数字模型。接下来，针对我们的需求（条状形状），只需要判断如果左右两个数字或者上下两个数字相加大于1，就一定满足了消除的条件。否则就不满足条件。请花1分钟想一下是不是这样。<br>
如果能够消除，接下来我们记录下能消除的点的集合，然后返回给调用者。于此同时，我们记录下待检查点的集合，然后遍历这些点，对每个点递归调用CheckDispel，并将结果返回给调用者。至于哪些点是待检查的呢？我们消除的“形状”的“底边”以上的全部点。因为它们的位置都会发生改变。CheckDispel的实现如下：

{% highlight csharp %}
    IEnumerable<List<Candy>> CheckDispel(byte row, byte column)
    {
        if (row < 0 || column < 0 || row >= Rows || column >= Columns)
        {
            Debug.Log("args error!" + row + ',' + column);
            yield break;
        }

        #region find same candys count

        byte left = 0, right = 0, up = 0, down = 0;
        int index = row;
        while (--index >= 0 && Data[index, column].ContentEquals(Data[row, column])) up++;
        index = row;
        while (++index < Rows && Data[index, column].ContentEquals(Data[row, column])) down++;
        index = column;
        while (--index >= 0 && Data[row, index].ContentEquals(Data[row, column])) left++;
        index = column;
        while (++index < Columns && Data[row, index].ContentEquals(Data[row, column])) right++;

        #endregion

        //more than two, can dispel
        if (left + right > 1 || up + down > 1)
        {
            //will dispels
            List<Candy> dispels = new List<Candy>();
            //will check next. all candys that moved should be checked.
            List<Candy> willChecks = new List<Candy>();

            if (left + right > 1)
            {
                while (left > 0)
                {
                    dispels.Add(Data[row, column - left]);
                    willChecks.Add(Data[row, column - left]);
                    left--;
                }
                while (right > 0)
                {
                    dispels.Add(Data[row, column + right]);
                    willChecks.Add(Data[row, column + right]);
                    right--;
                }
            }
            willChecks.Add(Data[row + down, column]);
            if (up + down > 1)
            {
                while (down > 0)
                {
                    dispels.Add(Data[row + down, column]);
                    down--;
                }
                while (up > 0)
                {
                    dispels.Add(Data[row - up, column]);
                    up--;
                }
            }
            dispels.Add(Data[row, column]);

            yield return dispels;

            for (int i = 0; i < willChecks.Count; i++)
            {
                for (int r = willChecks[i].Row; r >= 0; r--)
                {
                    foreach (var ps in CheckDispel((byte)r, willChecks[i].Column))
                    {
                        yield return ps;
                    }
                }
            }
        }
    }
{% endhighlight %}

注意，这种写法有个前提，那就是假设调用方在每次返回可消除的点后，都会对这些点进行消除，并更新数据，然后才进行下一次迭代。否则，最后的检查代码将会无限迭代。在正式的项目开发中不推荐，严格来讲是应该禁止这种对接口调用方的用法进行假设的编程习惯。<br>
所以现在，我们还无法测试上面的代码。我们必须一鼓作气，实现消除和更新数据的接口，然后统一来测试。<br>

{% highlight csharp %}
    /// <summary>
    /// 移除一些点，并将它们上面的点“落下”在它们的位置，最上面补上新的点。返回所有位置改变的点的集合。
    /// </summary>
    /// <param name="candys">待移除的点</param>
    /// <returns>位置改变的点</returns>
    public IEnumerable<Candy> RemoveCandys(IEnumerable<Candy> candys)
    {
        List<Candy> changes = new List<Candy>();

        Candy last = null;
        int same = 0;


        func movedown = () =>
        {
            for (byte i = last.Row; i > same; i--)
            {
                Data[i, last.Column] = Data[i - 1 - same, last.Column];
                Data[i, last.Column].SetPosition(i, last.Column);
                changes.Add(Data[i, last.Column]);
            }
            do
            {
                var chg = NewCandy();
                chg.SetPosition((byte)same, last.Column);
                changes.Add(chg);
                Data[same, last.Column] = chg;

            } while (same-- > 0);
            same = 0;
        };

        foreach (var candy in candys)
        {
            if (last == null)
            {
                last = candy;
            }
            else if (candy.Column != last.Column)
            {
                movedown();
                last = candy;
            }
            else
            {
                same++;
                if (candy.Row > last.Row)
                {
                    last = candy;
                }
            }
        }
        movedown();

        return changes;
    }
    delegate void func();
{% endhighlight %}







{% highlight csharp %}

{% endhighlight %}














