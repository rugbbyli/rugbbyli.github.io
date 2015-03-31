---
layout: post
title:  "WP8应用调用Facebook客户端登陆"
date:   2014-10-15
categories: WindowsPhone
tags: WindowsPhone AppCommunication
---

###背景：

项目中用到了Facebook登录，之前一直是用的Facebook Graph API的形式，通过http请求拿token。今天想试一下调用客户端的形式。本来这应该是个挺简单的东西，App发起请求，Facebook鉴定权限返回token，App拿到token后处理。但是真正做的时候发现里面还有有几个坑，记录一下备注。<br>
<!-- more -->
###分析：

Facebook的客户端使用Uri协议启动的方式与我们的App通信，它绑定了fbconnect的协议头，我们只需要调用类似 fbconnect://authorize?client_id={0}&scope={1}&redirect_uri=msft-{2}://authorize 的Uri启动默认程序就可以了。参数第一个是在Facebook申请的appid，第二个是要请求的功能，这些在http形式中都有出现，不再详述。需要注意的是第三个参数，它是我们项目在应用商店的GUID，在WMAppMainfest文件里面可以看到。当然，这个东西还需要在Facebook的管理后台绑定一下才能使用。<br>
如果调用成功，Facebook会回调redirect_uri参数指定的uri，将结果返回我们的App。回调的字符串格式为：msft-{0}://authorize/?access_token={1}&expires_in={2}，或者msft-{0}://authorize/?error={1}&error_code={2}&error_description={3}&error_reason={4}。<br>
那么我们的App要先处理一下WMAppMainfest文件，增加对msft-{0}的uri启动协议的处理，才能接收到Facebook的回调结果。这里第一个坑出现了。fb的开发文档只是模糊的说参数是ProductID，而我们App的ProductID的格式类似xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx这样，如果你直接填写这个，在fb的后台绑定时是没问题的，但是你在WMAppMainfest文件操作时，则会提示错误，大意是名字过长，超过（貌似）39个字符限制。后来试了下才发现，原来这里要去掉GUID里面的-连字符，这样加上"msft-"，总长度就小于限制了。。。当然fb后台也要去掉。。<br>
其次是申请功能时填的参数值，fb的开发文档里面有个示例，如下：
fbconnect://authorize?<br>
  client_id=675854675761202&<br>
  scope=basic_info,user_photos&<br>
  redirect_uri=msft-4ce6ab44b7f9442482de17535b713cde%3a%2f%2fauthorize<br>
  但是你用这个例子里面的scope调用，fb客户端会提示错误，basic_info值无效，需要用public_profile替换。。这个明显是客户端改版了，文档却没有更新，果然是程序员都不喜欢写文档啊。。<br>
这些都搞定后，最后终于可以调用成功，并接收回调结果了。关于如何监听回调uri调用，请参考MSDN文档。这里只提一点，关于UriMapper的MapUri方法。开始我理解的是，这个方法将传入的Uri处理，并返回新的Uri，然后页面将会导航到新的Uri去。所以我计划在这里处理msft回调，解析参数并加在要导航到的页面的Uri后面，作为Query参数传给目标页面。后来试了下，发现这里也有个坑。。<br>
原来，这个方法返回的Uri仅仅是用于确定要导航的目标页面，不管你在后面添加任何参数，都不会传递给页面的，你在页面的OnNavigationTo方法里面拿到的Uri，也不是你在MapUri方法中返回的Uri，而是原始的Uri。。<br>
所以，在MapUri方法中只需要判断要处理回调的页面，然后返回页面对应的Uri即可。在目标页面再解析fb的回调uri，拿到token等内容。<br>
最后，还发现每次导航，MapUri方法都会执行两次。这个暂时没弄明白有什么用意。。。

###实现：



代码下载：