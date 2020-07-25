# 1. 简介
这是Unity Android APP il2cpp热更完美解决方案的Demo（[Git地址](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo/)）的说明。
    
和现有的热更解决方案不同的是，他不会引入多余的语言（只是UnityScript，c#...），对Unity程序设计和编码没有任何限制。你可以在预置和场景里的GameObject上添加任何的Compnents组件，需要序列化的和不需要序列化的，他们都是可以热更的，也不需要做额外的标记处理。简而言之，在此方案下，Unity的所有资源和脚本，都是可以热更的。

本文接下来将介绍如何去制作热更文件和如何应用这些热更文件。为了简化Demo的设计，Demo包含的热更文件会事先以全量更新的方式制作好，一起打到了Apk里面。具体到项目中热更文件得放服务器，正式上线得放CDN，以增量更新的方式捣鼓出和文中一样的目录结构就OK了。

Demo代码适用Unity2018-？，2017需要自己看版本历史回退Zip库。

# 2. 方案总览

Unity在以il2cpp方式导出Android工程（或者Apk文件）的时候，代码会被编译成libil2cpp，而相关的资源、配置和序列化数据会以他们各自的格式导出到android的assets目录（assets/bin/Data）。这两部分，libil2cpp和assets目录，必须匹配（即需要在同一次打包中提取，可能有的变了，有的没变，增量方式只提取变化的部分）才能正常工作，不然Unity会在启动时崩溃。本方案就是热更这两部分。

热更的正式流程如下图.

![运行时流程图](https://github.com/noodle1983/private/raw/master/UnityAndroidIl2cppPatch/patch_workflow.png)

流程说明：
* 步骤1，在Unity的逻辑之前，libbootstrap会检查本地是否有Patch. Apk安装后，没应用过任何热更，本地是不会有Patch文件的，走no流程。如果热更过，则会有Patch目录，走yes流程。如果是覆盖安装，会检查apk和Patch文件的修改时间，走no流程。Patch目录如何准备，后面会将到。
* 步骤2，加载Patch目录对应架构（arm/x86）的libil2cpp库，并应用assets目录的更新文件。
* 步骤3，开始Unity的流程，进入Unity第一个场景，并执行相关的Unity Script，一般是C#，我们都以C#举例。
* 步骤4，检查服务端是否有新的patch，这步demo没有演示，需要自己实现。
* 步骤5，下载新的patch，这步demo也没有演示，需要自己实现
* 步骤6，根据规则准备patch目录，详细规则会在后面描述。在Demo中只是将全量更新包解压，全量更新包打包的时候目录结构就是对的，所以不需要做其他的处理。
* 步骤7，调用libbootstrap的接口设置patch目录，因为libil2cpp已经加载进进程，所以需要重启APP，从新的patch目录加载patch。这步Demo中有设置patch目录的例子。

* 步骤8，重启APP，Demo提供了纯C#代码。
* 步骤9，没有新的Patch，就正常进入游戏了。

流程里更详细的描述和如何生成Patch文件，见第三章。

# 3. Demo详述

## 3.1.	Demo目录结构

工程所有文件均置于AndroidIl2cppPatchDemo目录下。各文件目录说明如下表。

| 名字        | 说明   |
| -------- | ----- |
| Editor/AndroidBuilder.cs| 这个文件包含所有从导出Android工程，到输出Patch和生成Apk安装文件的代码。|
| Editor/Exe/zip.exe| zip压缩工具，用来将asset/bin/Data下的文件压缩成标准zip格式。 |
| Plugin/| 包含libbootstrap库. |
| PrebuiltPatches/| 包含预先生成的两个全量热更新版本。|
| Scene/*.unity| 演示场景，母包和版本1仅有0.unity，版本2增加了1.unity，测试新增场景和脚本的patch |
| Script/Bootstrap.cs| 这个文件定义了libbootstrap的c#接口和重启APP的纯c#实现|
| Script/VersionSettor.cs| 这个脚本用于运行时准备相应的热更版本目录。|
| Script/UI/MessageBoxUI.cs| 这是一个简单的运行时MessageBox控制器。|
| ZipLibrary/| c#版的压缩解压工具，输出的zip文件为非标准文件，Patch制作中不能用于asset/bin/Data文件的压缩，仅用于libil2cpp库的压缩，运行时用于全量热更包的解压. |

所有文件就这么多，项目用git管理，master分支为母包分支，version1和version2分支为热更1和热更2分支，分支间会有些细微的差别，version1主要测试序列化数据，version2添加了新场景和新脚本，具体可以diff查看。下面会详细描述打包过程和如何应用热更文件。

## 3.2.	打包过程

所有的打包逻辑在文件Editor\AndroidBuilder.cs里。展开主菜单**AndroidBuilder**, 可以看到有5步，为了和热更启动流程区分，我们就叫他过程。

- 过程1：以il2cpp的方式，导出Gradle Android工程。选择Gradle Android工程，而不是ADT Android工程，只是因为Unity2018不再支持ADT方式。Demo并不依赖AndroidStudio，只是导出的Android工程目录结构是以Gradle的方式注释，之后的构建步骤都是调用原始JDK/SDK的方式。Demo这部分的代码可以复用，但需要根据项目需求做一些修改。

* 过程2：需要修改一下Android工程，因为libbootstrap需要在进入Unity的帧循环前，检查加载本地准备好的patch。大多数情况，你可以复用这个步骤的代码。但是如果你的项目修改了Unity Java的继承体系，你需要检查一下这块代码是否有调用到。如果没有调用到，后面Unity帧循环中的逻辑和资源，用的都是Apk内的相应文件。

* 过程3：生成热更文件。如在第二章所述，patch分为两部分，il2cpp库和assets/bin/Data目录。具体做法代码均有提供，需要注意的是必须遵守各个文件的命名方式和相对路径。各个文件均有压缩，对于增量包，如果压缩前的文件和之前相比没有变化，则不需要制作对应的压缩文件。这部分制作压缩部分的代码可复用，增量部分需要自己实现，热更文件最好也加进版本管理（svn/git/...）中。

* 过程4: 生成打包的windows脚本。脚本仅依赖JDK/SDK命令，可复用。生成脚本后，Android工程就不依赖Unity了，可以随意替换文件，再次调用脚本生成新的Apk。需要注意的是，打包用的so动态库，是pkg_raw目录下的so文件，替换时请注意。首次会在Unity目录下生成keystore目录和相应的签名文件，可以将此签名替换，并修改导出脚本中的签名密码。

* 过程5: 执行过程4中的脚本，生成Apk安装文件，可复用。

主菜单AndroidBuilder下还提供了菜单“Running Step 1, 2, 4, 5 for the base version”，这是一键构建母包版本用的，母包不需要制作patch文件，所以少了过程3；和菜单“Runnnig Step 1-4 for patch versions”，这是一键构建Patch用的，因为在demo里，不需要导出Apk文件。

关于打包这里得多说两句。 如果没有采用AssetBundle的方式打包，Unity会按各自格式，将所有场景和依赖输出到assets/bin/Data目录，这样子也是可以热更的。但是，不要这么做，因为这样做微小的改动会影响到多个文件，导致热更文件过大。最好是自己用AssetBundle的方式将资源做一个清晰的划分，打包好的AssetBundle放在assets下的其他目录。需要注意和libil2cpp库和assets/bin/Data的文件向匹配（保证是同一个版本的输出）。运行时可以重写AssetBundleManager.overrideBaseDownloadingURL加载最新的AssetBundle。

## 3.3.	运行时应用热更文件
我们回顾一下第二章的流程图，结合打包过程和Demo的代码，做进一步的说明.

![运行时流程图](https://github.com/noodle1983/private/raw/master/UnityAndroidIl2cppPatch/patch_workflow.png)

打包过程2里，需要在UnityPlayerActivity.java文件头导入一个库，在Unity的游戏逻辑之前，插入了一行Java代码。
```
        import android.view.WindowManager;
+       import io.github.noodle1983.Boostrap;
```
```
+       Boostrap.InitNativeLibBeforeUnityPlay(getApplication().getApplicationContext().getFilesDir().getPath());
        mUnityPlayer = new UnityPlayer(this);
```
这三行代码保证了上图中步骤1-2能在步骤3之前执行，下一行mUnityPlayer的代码即开始了步骤3的执行。步骤3之后所有的逻辑，都是已热更过的il2cpp库里的Unity Script（c#，...）了。热更部分的逻辑如果有修改，会在热更后体现，如果这部分的bug不影响下次热更，则可以通过热更修复，否则应指引用户清除本地数据，以母包热更逻辑更新到最新。所以，在方案的应用中，仍需尽量保证热更部分的代码稳定，不能随意更改。

如前所述，Demo里没有步骤4和步骤5的相关逻辑，步骤6中Patch的准备，Demo只是简单地将全量压缩包解压，相关逻辑在Script/VersionSettor.cs文件中。准备更新目录时，应保证libil2cpp部分被解压，命名方式和Demo保持一致，而assets_bin_Data下的文件不需要解压，应保证目录结构和Demo保持一致。如果是增量更新，Patch目录下的文件应该是相对于母包的修改文件。在持续热更中，应保证在步骤7前，本地当前Patch目录的完整性（保证运行中的App还能正常执行），新的Patch应新建目录，通过硬链接的形式从当前Patch目录中提取所需要的没变化的文件，准备好后执行步骤7，重启后将老Patch目录删除. 步骤7和步骤8的代码也在Script/VersionSettor.cs文件中，样子如下
```
        //4. tell libboostrap.so to use the right patch after reboot
        string error = Bootstrap.use_data_dir(runtimePatchPath);
        if (!string.IsNullOrEmpty(error))
        {
            messageBox.Show("use failed. path:" + zipLibil2cppPath + ", error:" + error, "ok", () => { messageBox.Close(); });
            yield break;
        }

        //5. reboot app
        yield return StartCoroutine(Restart());
```


# 4. Verify 和 Build

## 4.1. Verify
安装预编译的Apk文件，点击按钮可以切换各个版本。

[release版本](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo/releases)

## 4.2. Build

### 依赖

*	Unity (我用Unity2017/Unity2018/Unity2019)
*	Unity2019.4 自带的JDK，SDK，NDK
*	Git 
	
### Build指引

* 1. 在Unity中（**Edit**->**Preference**->**External tools**）设置好 JDK/SDK/NDK 路径，打包代码里会从Unity中读取。
* 2. 确保gradle能正常工作（Unity内能正常导出apk）
* 3. 出母包，执行菜单 **AndroidBuilder**->**Run Step 1, 2, 4, 5 for base version**, 成功后会弹出文件管理器显示apk所在的目录.
* 4. 一般来说你不需要打Patch文件，如果要打，用git checkout version1或version2，执行菜单 **AndroidBuilder**->**Run Step 1-4 for Patch Version**。PrebuildPatches目录下的相应文件会被更新。

# 5. 剩下的工作和建议
打包部分
* 设置部分需要根据项目实际做修改。
* 热更文件的增量版本化管理。

运行时部分
* 检查新版本和下载热更文件。
* 持续增量更新的Patch目录的准备。
* 用Asset Bundle管理资源。

另外，打包的工作尽量自动的一键化，一次化，除非你想在打包当晚集体晒月亮。另外，低成本的打包流程，大家都愿意在真机上看结果，利于产品的稳定。Demo其实提供了一套自动化的框架和脚本，理解透，化为己用，也是幸事一件。如果有更好的方式，欢迎讨论。

# 6. 许可

MIT license.


# 7.主要贡献
* 1. [noodle1983(Noodle)](https://github.com/noodle1983)完成了第一版， MIT开源
* 2. [DesperateZero(_DespAir.)](https://github.com/DesperateZero)贡献了首次上线机会，协助解决了一下问题
	- 文件句柄泄漏
	- 适配：oppo手机，/data/data/appid/files 访问不了
	- Google abb打包的适配问题（原因是abb的方式会有多个apk包，原来是patch第一个，目前方案是patch含路径asset/bin/Data的那个）
* 3. [qq：墨(2900180710)<abram_ped@126.com>]发现并解决了原读写锁不生效的问题，测试文件[test_mutex.cpp](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo-libboostrap/blob/master/unittest/test_mutex.cpp)
* 4. [sisong(e侯子.net)](https://github.com/sisong)实现了基于hook和ApkDiffPatch的另一种实现方式，实现了mono和Unity版本的热更，并做了大量的机型和各个API Level的测试。[项目地址](https://github.com/sisong/UnityAndroidHotUpdate)

* 5. qq：木子(406351314)、西's my baby(871967866)<quange0314@163.com>团队和Sin(446541451)yudimo@vip.qq.com团队，帮忙发现和验证随机崩溃问题：https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo/issues/12

* 6. qq：Enough.(1098839800) 实践文档：[通过IL2CPP热更代码](https://www.jianshu.com/p/7b205ebcb54a)

* 7. qq:粽仔001(1144916875) 发现的Demo中回退base version，脚本常量仍然是patch版本的问题。（il2cpp cache没清）

* 8. qq:みなの総長(53957801) 发现的新版本没有更新文件，仍走了没必要的更新逻辑的问题。

# 8.调试和遗留问题
* 1. 集成中如果有问题，先看看Demo的[release版本](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo/releases)有没有问题。如果Demo有问题，直接找我吧，QQ或邮件都行。
* 2. 如果感兴趣，可以在[log.h](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo-libboostrap/blob/master/log.h)里，把日志全开，会有一些很有意思的日志，对理解原理和调试都很有用。
* 3. Android obb的支持，欢迎pull request，提供支持
* 4. Q群：593413410
* 5. 遗留问题见[Github Issues](https://github.com/noodle1983/UnityAndroidIl2cppPatchDemo/issues)

# 9.支持

- 邮件: noodle1983@126.com.
- Q群：593413410（广告免扰）

# 10.随缘

[PayPal:https://www.paypal.me/noodle1983](https://www.paypal.me/noodle1983)

Or Alipay:![avatar](https://raw.githubusercontent.com/noodle1983/private/master/qr_icon/noodle1983_ali.png)
Or Wechat:![avatar](https://raw.githubusercontent.com/noodle1983/private/master/qr_icon/noodle1983_wx.png)
