---
layout: post
title:  "windows下搭建Jekyll环境"
date:   2014-07-15
---

1，在http://rubyinstaller.org/下载安装ruby环境;<br>
2，安装完成后打开cmd，切换到ruby安装文件夹;<br>
3，输入指令gem install jekyll，回车;<br>
4，等几分钟后，弹出错误提示：<br>
	ERROR:  Could not find a valid gem 'jekyll' (>= 0), here is why:<br>
	Unable to download data from https://rubygems.org/ - Errno::ETIMEDOUT<br>
5，切换ruby gem库到国内平台https://ruby.taobao.org/：<br>
	$ gem sources --remove https://rubygems.org/<br>
	$ gem sources -a https://ruby.taobao.org/<br>
	$ gem sources -l<br>
	\*\*\* CURRENT SOURCES \*\*\*<br>
	<br>
	https://ruby.taobao.org<br>
6，继续输入gem install jekyll，回车；<br>
7，等几分钟后，弹出错误提示：Please update your PATH to include build tools or download the DevKit;<br>
8，到http://rubyinstaller.org/下载安装devkit；<br>
9，运行devkit安装目录下的msys.bat；<br>
10,切换gem库，参考5；<br>
11，继续输入gem install jekyll，回车；<br>
12，等几分钟下载完成，弹出提示Building native extensions.  This could take a while...；<br>
13，耐心等待一大堆组件解压安装过程；<br>
14，输入jekyll，回车，弹出提示：jekyll 2.1.1 -- Jekyll is a blog-aware, static site generator in Ruby，大功告成！<br>