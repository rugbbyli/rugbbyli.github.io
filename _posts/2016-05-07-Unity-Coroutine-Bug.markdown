---
layout: post
title:  "Unity3D Coroutine 的坑"
date:   2016-05-07
categories: Unity
tags: Unity Problem
---


一大早莫名其妙出了个问题，代码很简单，是一个协程函数内部加了异常处理的，原型如下：

```csharp
IEnumerator Test()
{
    yield return null;

    try
    {
        //do something
    }
    catch (System.Exception ex)
    {
        
    }
}
```

然后Unity就报了个编译错误，信息如下：

    Unhandled Exception: System.ArgumentException: Trying to emit a local from a different ILGenerator.

这是什么鬼？

###在协程函数内部使用try-catch的问题

于是试探了下，发现问题出在catch语句中。**当在catch中声明了捕获异常变量，然而并没有使用它时，就会导致这个编译错误。**去掉catch后面的Exception变量即可解决报错：

```csharp
try
{
    //do something
}
catch
{
    
}
```

如果声明了异常变量，则一定要在catch块中有引用过它至少一次，也能解决报错，比如：

```csharp
try
{
    //do something
}
catch (System.Exception ex)
{
    string msg = ex.Message;
}
```

那么如果不是协程函数呢？我又分别测试了普通的函数和泛型的枚举函数：

```csharp
IEnumerator<int> Test1()
{
    yield return 1;

    try
    {
        int i = 0;
    }
    catch (System.Exception ex)
    {
        
    }
}

void Test2()
{
    try
    {
    }
    catch (System.Exception ex)
    {
        
    }
}
```

发现只有普通函数不会报错。

那么是mono编译器的问题吗？于是我把同样的代码放在mono控制台程序中进行测试，可以正常通过编译，执行也没有异常。

这就比较有趣了。当然了，一般情况下不会出现这样的问题（因为谁会没事声明个变量又不用呢），但是我们关心的是为何会有这种奇怪的问题出现呢？

来看一下异常堆栈：

```
Unhandled Exception: System.ArgumentException: Trying to emit a local from a different ILGenerator.
  at System.Reflection.Emit.ILGenerator.Emit (OpCode opcode, System.Reflection.Emit.LocalBuilder local) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.LocalInfo.EmitAssign (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.VariableReference.EmitAssign (Mono.CSharp.EmitContext ec, Mono.CSharp.Expression source, Boolean leave_copy, Boolean prepare_for_load) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Catch.DoEmit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Statement.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.TryCatch.DoEmit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Statement.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Block.DoEmit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Block.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Iterator.EmitMoveNext (Mono.CSharp.EmitContext ec, Mono.CSharp.Block original_block) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.IteratorStatement.DoEmit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Statement.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Block.DoEmit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Block.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.ExplicitBlock.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.ToplevelBlock.Emit (Mono.CSharp.EmitContext ec) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.MethodData.Emit (Mono.CSharp.DeclSpace parent) [0x00000] in <filename unknown>:0 
  at Mono.CSharp.MethodOrOperator.Emit () [0x00000] in <filename unknown>:0 
  at Mono.CSharp.Method.Emit () [0x00000] in <filename unknown>:0 
```

根据堆栈分析，好像确实是mono编译代码时出的问题（编译catch块的参数赋值时引发异常，这也是为何去掉参数能编译通过，因为绕开了这个bug）。

随后google之，立刻就搜到了Unity论坛中的一个求助，跟我遇到的问题一模一样，链接如下：[Try / Catch in coroutines causes an internal compiler error][1]

里面有人推荐了一篇博客说可以解决此问题，链接：[Wrapping Unity C# Coroutines for Exception Handling, Value Retrieval, and Locking][2]

文章主要科普了协程的一些使用注意事项和心得（挺值得一看的），一开始就直接给出了观点：在协程级别是没有异常处理的，非要使用可能会造成潜在的问题。最后作者的建议是异常捕获可以放在协程函数的调用处进行。

然后继续往下翻，综合几个mono论坛的反馈帖子和官方的答复，基本能确定这属于mono早期版本的一个bug，最新版本已经修复了这个问题，所以在mono控制台程序中能正常编译通过，而由于unity使用了古老的mono库，所以……很悲催的，就出了这个结果。

所以最终，还是那个结论（貌似很多问题都可以归到这个结论上，(⊙﹏⊙)b）：

由于mono对c#的实现是仿照微软的黑盒实现，所以从代码编译到最终运行的许多细节可能都会有所不同，对于熟悉.Net的程序员，尤其要注意这一点，防止因此出现一些诡异的bug。


  [1]: http://forum.unity3d.com/threads/try-catch-in-coroutines-causes-an-internal-compiler-error.364866/
  [2]: http://www.zingweb.com/blog/2013/02/05/unity-coroutine-wrapper/
  