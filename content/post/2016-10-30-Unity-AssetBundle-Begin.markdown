---
layout: post
title:  "Unity资源分离探索"
date:   2016-10-30
categories: Unity
tags: Unity AssetBundle
---


资源分离，即将一部分资源从初始安装包中分离出去，在需要用到时动态下载的功能。它最直观的好处就是减少了安装包大小，（可能）会对用户的安装欲望产生正面影响。

Unity提供有资源分离的官方解决方案：AssetBundle。这套系统支持将若干资源打包为AssetBundle，然后在运行时通过一系列API加载并使用其中的资源。

我们知道，Unity在打包安装包时，是根据场景确定资源引用的。比如在BuildSettings中添加并勾选了a场景，那么所有a场景中引用的资源都会被打包进安装包中，以确保运行正常。（此外，项目中任意Resources目录下的资源也会被打包进去，因为这些资源可以通过代码加载，无需提前引用。）其它资源则并不会被打包。

所以资源分离最简单的方式便是，将某些场景打包为AssetBundle，然后从BuildSettings中去除这些场景，这样生成的安装包不再包含这些资源，自然会减少一些尺寸。此外，如果能将Resources也打包为AssetBundle，会进一步减少包大小。极端情况下，我们的安装包可以只包含一个初始场景用于下载其它的AssetBundles并在下载完成后加载其中的场景，这样安装包几乎是空包的大小（比如安卓平台可能只有7-8MB大小）。

那么接下来就是打包和使用的具体过程。

一般来说，可以通过在编辑器中设置资源的属性，并调用

    UnityEditor.BuildPipeline.BuildAssetBundles
    
来生成AssetBundle的；但是这种方式比较不直观，此处推荐一个可视化打包工具 [AssetBundleGraph](https://github.com/unity3d-jp/AssetGraph)，这是Unity日本团队开发的辅助插件，可以提高打包的效率，优化流程。

关于此工具的使用这里就不再详述。最终我们将所有Resources资源打为一个AssetBundle，将所有场景按需要打为一个或多个AssetBundle。然后将它们从安装包中去除，并在运行时下载这些Bundle。

下载完成的Bundle可以通过WWW加载，或者通过

    AssetBundle.LoadFromFile
    
加载。对于包含场景的AssetBundle，只要加载了后续就可以直接通过

    SceneManager.LoadScene
    
传入场景名称来载入场景了，无需额外工作；而对于Resources，由于之前是通过

    Resources.Load
    
接口加载的，需要改为调用AssetBundle的实例方法

    AssetBundle.LoadAsset
    
来加载。

此处需要注意的是，两种方式传入的参数不太一致，Resources需要通过资源与Resources目录的相对路径去除扩展名来加载，而AssetBundle可以通过资源名称或者相对项目根目录的路径信息加载（如Assets/xxx/xxx.prefab等）。另外一个比较麻烦的问题是关于资源命名的问题，Resources是可以存在多个不同目录下相同的资源名的，比如：
    
    Assets/Resources/XXX/a.prefab;
    Assets/Resources/YYY/a.prefab;
    
而这种情况下，当把这两个Prefab打包进同一个AssetBundle时，就不能通过资源名称来加载了，只能通过路径信息。此外，Resources某种情况下也存在此类问题，比如两个资源的目录结构如下时：

    Assets/XXX/Resources/a.prefab;
    Assets/YYY/Resources/a.prefab;

前面说过，由于Resources加载资源是使用相对Resources目录的路径信息，那么此时如果用

    Resources.Load(a);
    
来加载，就会存在歧义。所以不管是用Resources还是AssetBundle，最好在创建资源时就引入一套好的命名机制，以规避资源同名这种情况的发生。

OK，回到主题，我们可以发现，场景资源（即.unity文件）相对其它类型资源是比较特殊的。比如AssetBundle提供了GetAllScenePaths等专为场景资源服务的接口。事实上，针对场景和其它资源的AssetBundle确实是不同处理的，这表现在：

     1. 场景文件和其它类型资源不能打包进同一个AssetBundle；
     2. 含有场景的AssetBundle不能通过LoadAsset等接口加载资源；

所以你需要通过某种机制区分这两种不同的Bundle，以避免使用时出现错误。

如果正确处理了上面的问题，你会发现你已经得到了一个初步实现的资源分离版本。在多数情况下它已经可以实现跟完整版本相同的效果了。截止目前为止，整个系统看起来还算简单，并没有涉及到一些类似资源依赖等烦人的问题。我提到了“多数情况”，也就意味着，是的，在某些情况下会出问题。典型的例子是，两个资源包用到了同一个资源。说到这里，还是要提一下AssetBundle打包的一些细节问题。

规则1. 指定资源被打包进某个资源包，这种资源打包方式可称作**显式打包**。其它依赖它的资源包不会再次将它打包进来，而是转而依赖含有它的资源包。使用时，只要先行加载了包含依赖资源的包，就能正常使用了。

规则2. **如果依赖资源没有被显式打包，AssetBundle会自动将依赖资源打包进资源包**。这种依赖资源的自动打包可称作**隐式打包**。比如我们打包了一个场景，场景中引用了一张贴图，而我们没有将这张贴图打包进任意AssetBundle，那么这种情况下这个场景的AssetBundle将会包含这张贴图。隐式打包的资源**无法通过LoadAsset加载使用**。

规则3. **一个资源只能被显式打包进一个AssetBundle中**，（但可能被隐式打包进多个AssetBundle中）。

规则4. 根据规则1和2可以推出，某个资源要么被隐式打包，要么被显式打包，不可能两者皆有。

举个例子来说明这几个规则：

现有贴图 t，Prefab p和场景 s， p 和 s 都引用了 t ，我们将 s 和 p 分别打包为 as 和 ap ，然后分析包内容：

    as: 包含 s 和 t ; 
    ap: 包含 p 和 t ;

（但是两个包都不能通过LoadAsset的方式加载 t ）

现在我们改变打包规则，将 p 和 t 都打包进 ap 中，再次分析包内容：

    as: 包含 s ;
    ap: 包含 p 和 t ;

可以发现，Unity正确执行了判断，发现了 t 已经被打包，则不再将它打包进 as ，而是通过 ap 来引用它。此时在加载场景 s 前，不但要加载 as ，还需要加载 ap 。

接下来，我们在上面的基础上再新增一个包 at ，将贴图 t 打包进去（此时 t 同时被 ap 和 at 打包）。分析包内容：

    as: 包含 s ;
    ap: 包含 p ;
    at: 包含 t ;

可以发现一个资源只能存在于一个资源包中，此时依赖关系也变成了 as 和 ap 都依赖 at 。

通过上面的实验可以发现，隐式打包的资源是可以同时存在于多个资源包中的，这就意味着，最终的资源包总大小超过了资源实际大小；反过来，显式打包的资源越多，则总体资源包的大小就越接近原始大小，但包的依赖关系也会越复杂。所以此处也需要一个折衷，在易用性和大小中找到一个平衡点。

背景介绍完毕。上面说过当多个 AssetBundle 包含同一个资源时，可能会出现问题，而根据一般认知，即使有同一资源的多个拷贝，也不应该出问题，除非出现一种比较特殊的情况，举例来说明：

假如有一个相机Camera，我们设置它渲染到一个RenderTexture中，然后有一个Material引用RenderTexture，并用于场景中一个Quad的渲染，此时Quad显示的内容为Camera的渲染画面。我们将Camera保存为Prefab，在运行时动态加载。此时如果我们按照最简单的打包方式，将Scene和Prefab分别打包，则RenderTexture会被分别包含进两个AssetBundle中，运行时，场景和Prefab会分别从各自的包实例化一个RenderTexture，两者引用的不再是同一个RenderTexture，则Camera的渲染内容无法在Quad中显示了。

要解决这个问题，则按照前面介绍的规则，我们只需要在打包Prefab时，手动将RenderTexture也打包进来即可，这样Scene会自动改为从Prefab所在的包引用，从而恢复了两者的联系。

所以归纳下就是，**当资源是运行时动态更新内容时，就一定要显式打包进某个AssetBundle，以强制所有的引用都使用同一份实例**。

关于资源分离的内容就先介绍到这里。



PS：从Unity5开始，不能再通过类似AssetBundle.LoadAsset<YourClass>(name)的方式从AssetBundle来加载物体并自动获取脚本了（Resources还是可以这样操作）。

        Prior to version 5.0, users could fetch individual components directly using Load. This is not supported anymore. Instead, please use LoadAsset to load the game object first and then look up the component on the object.
    
PS2：Unity在打包时可以将没有用到的代码从运行库剥离出去，从而减少包大小，这个选项叫做“StrippingScripts”。由于此技术只会判断最终安装包引用的代码，使用资源分离时，切记要关闭此选项，否则一些Assetbundle引用类库会被剥离出最终的代码库，导致加载AssetBundle时报错。