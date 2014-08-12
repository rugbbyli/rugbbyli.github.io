---
layout: post
title:  "使用Unity导出Texture中的Sprite"
date:   2014-08-12
categories: Unity
tags: Unity Editor
---

###背景：
	Unity4.3新增了2D框架，我们可以导入一张拼图，并通过内置的切割工具切割为若干Sprite。
	但是由于Sprite只记录了原始图片中的某块区域信息，而并非保存为物理文件，造成一些第三方插件对Sprite的支持还不是很好，比如NGUI等。
    因此本文通过创建一个插件，将Sprite导出为文件，实现了某些特殊需求。

###分析：
	我们基本的思路是：通过Unity插件系统生成一个编辑器窗口，在窗口中监控Project等目录下选中的Sprite并列出在窗口中。点击"Export"即可导出到磁盘上。
    本文的知识点涉及插件系统、GUI绘图、Texture2D、文件操作等。

###实现：
1，在右键菜单项中添加菜单：
    新建一个类，命名为SpriteExporter，并继承自EditorWindow。添加如下方法：

{% highlight csharp %}
[MenuItem("Assets/Export Sprite", false, 0)]
static public void OpenSpriteExporter()
{
    EditorWindow.GetWindow<SpriteExporter>(false, "Export Sprite", true);
}
{% endhighlight %}

    这段代码会在菜单中添加一个名为"Export Sprite"的菜单项。选中菜单项会打开一个SpriteExporter窗口实例。当然此时窗口中没有任何内容。

2，将选中的Sprite显示在窗口中：
    首先添加一个方法，获取当前选中的全部Sprite。原理很简单，通过对Selection.objects的枚举，依次判断是否为Sprite并添加到数组中：

{% highlight csharp %}
[MenuItem("Assets/Export Sprite", false, 0)]
List<Sprite> GetSelectedSprites()
{
    List<Sprite> textures = new List<Sprite>();

    if (Selection.objects != null && Selection.objects.Length > 0)
    {
        Object[] objects = EditorUtility.CollectDependencies(Selection.objects);

        textures.AddRange(from obj in objects where obj is Sprite select obj as Sprite);
    }
    return textures;
}
{% endhighlight %}

    然后，实现OnGUI方法，画出这些Sprite：

{% highlight csharp %}
[MenuItem("Assets/Export Sprite", false, 0)]
void OnGUI()
{
    var list = GetSelectedSprites();

    if(GUILayout.Button("OK, Export thess Sprites as image file."))
    {
        
    }

    var rect = GUILayoutUtility.GetLastRect();

    int i = 0, j = 0, w = 50, h = 50, margin = 5;
    foreach(var sp in list)
    {
        int x = (i++) * (w + margin) + margin;
        int y = j * (h + margin) + margin + (int)rect.height;
        if (x + w*2 >= rect.width)
        {
            j++;
            i = 0;
        }
        GUI.DrawTextureWithTexCoords(new Rect(x,y,w,h), sp.texture, uvRect(sp), true);
    }
}

public Rect uvRect(Sprite sp)
{
    Texture tex = sp.texture;

    if (tex != null)
    {
        Rect rect = sp.textureRect;

        rect.xMin /= tex.width;
        rect.xMax /= tex.width;
        rect.yMin /= tex.height;
        rect.yMax /= tex.height;

        return rect;
    }
    return new Rect(0f, 0f, 1f, 1f);
}
{% endhighlight %}

我们将每个Sprite显示为50*50大小的预览图，并通过计算窗口的宽度执行换行操作，以免太多的图片显示不全。这里有一个细节，由于暂时没有找到获取窗口宽度的方法，我是通过GUILayoutUtility.GetLastRect方法，来得到窗口宽度的。因为在这个调用之前有GUILayout.Button方法，而这个生成的Button会自动铺满窗口宽度，因此才可以通过这种方法取得窗口宽度。
此时切换到Unity，选中一些Sprite，右键打开Export Sprite窗口，会发现它们已经列在了窗口中。左右拖动改变窗口大小，会发现图片会自动换行，酷毙了`(*∩_∩*)′
但是此时如果改变选择，窗口并不会实时更新，我们还需要添加OnSelectionChange方法，通知窗口刷新：

{% highlight csharp %}
void OnSelectionChange()
{
    Repaint();
}
{% endhighlight %}

接下来，我们需要实现最关键的一步，保存Sprite到文件。大致的步骤如下：
通过Sprite.textureRect和Sprite.texture属性获取Sprite的源和位置信息；
通过Texture.getPixels方法取得对应区域的颜色数据；
新建一个Texture2D，大小指定为Sprite大小，颜色设置为上面取得的数据；
调用Texture.EncodeToPNG编码为png格式数据，保存到文件中。

下面是具体的实现方法：


{% highlight csharp %}
void ExportSprites(IEnumerable<Sprite> sprites)
{
    foreach (var sp in sprites)
    {
        int x = (int)sp.textureRect.x;
        int y = (int)sp.textureRect.y;
        int width = (int)sp.textureRect.width;
        int height = (int)sp.textureRect.height;
        var tex = new Texture2D(width, height, sp.texture.format, false);

        Color[] colors = sp.texture.GetPixels(x, y, width, height);

        tex.SetPixels(colors);
        tex.Apply();

        var data = tex.EncodeToPNG();

        var pathbase = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        var path = string.Format("{0}/ExportedSprites/{1}", pathbase, sp.texture.name);
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        var file = string.Format("{0}/{1}.png", path, sp.name);
        System.IO.File.WriteAllBytes(file, data);
    }
}
{% endhighlight %}

然后将ExportSprite方法添加到OnGUI的Button判断条件下面，这样当Button被点击时，将会执行我们的方法。
注意这里我将导出路径设置为[Desktop]/[TextureName]/[SpriteName.png]。

看起来一切都没有问题。我们切换到Unity，选中几个Sprite并打开我们的窗口，点击按钮，很不幸，你很可能会遇到下面的错误提示：
    Unity Exception：Texture 'XXX' is not readable,the texture memory can not be accessed from scripts....
看起来似乎是说Texture不可读？
这还真是个麻烦事~。最后通过读NGUI的代码，找到了一种解决方案，将Texture设置为可读，代码如下：

{% highlight csharp %}
bool MakeTextureReadable(Texture tex)
{
    string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
    if (string.IsNullOrEmpty(path)) return false;
    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
    if (ti == null) return false;

    TextureImporterSettings settings = new TextureImporterSettings();
    ti.ReadTextureSettings(settings);

    if (!settings.readable)
    {
        settings.readable = true;

        ti.SetTextureSettings(settings);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }
    return true;
}
{% endhighlight %}
它的基本原理是改变TextureImporter的设置中readable属性为true，并重新导入资源~。~
接下来，我们修改一下ExportSprites方法，在调用GetPixels之前，先执行一下MakeTextureReadable方法，并根据返回值决定是否继续进行下去。

这次再试验一次，发现可以正常导出了。over。

###改进：

{% highlight csharp %}

{% endhighlight %}




