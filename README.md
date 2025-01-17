# UnityIconSwitcher

>Unity项目实现的iOS和Android平台下，应用图标切换功能的插件

该库位Demo程序。其中`Assets/IconSwitcher`目录是插件，拷贝该目录到自己项目即可。

【演示视频】 https://www.bilibili.com/video/BV1SHwneKEHc/?share_source=copy_web&vd_source=dabc4849ffa10d0cfa57ccd69607f8a4


# 使用文档

## 简介

在Unity项目中实现iOS和Android平台的多套图标配置，可以通过代码修改应用图标的功能。

## 使用方法

### 开发配置

#### 准备图标

在工程IconSwitcher/Icons目录下按照图标名称创建文件夹来存放额外的图标。Demo中分别创建了icon_1和icon_2两个文件夹。
图标文件目录结构参考Demo，保持一致即可。

#### 修改代码

修改IconSwitcher.cs类的IconNameConfigs字段，将放置的图片文件夹名称填入数组，一定要和IconSwitcher/Icons目录下的文件夹名称一致。

Demo中值为:
```
/// <summary>
/// 图标名字配置
/// </summary>
public static readonly string[] IconNameConfigs = new string[] { "icon_1", "icon_2" };
```

#### 调整模版(仅针对Android平台)

>如果Android项目自己继承并重写了启动Activity，需要执行该步骤，否则可以忽略。

打开工程IconSwitcher/Templates目录下的alias_template.xml文件。修改 `android:targetActivity="com.unity3d.player.UnityPlayerActivity"` 中的值为自定义的Activity。

### 运行时

通过IconSwicher.cs的UseIcon方法，指定要使用的图标。索引0表示的是默认图标。索引1开始依次对应IconNameConfigs中的值。

如果在Android环境下，想暂时屏蔽切入后台时图标刷新导致的APP闪退，可以将refreshEnable字段设置为false。

## 使用限制

### Android

#### 闪退问题

Android下切换图标不能立刻生效，因为会导致APP闪退。这里的处理方式是当APP进入后台时进行处理。可以通过IconSwicher.refreshEnable来设置是否允许进入后台时进行图标更换，在某些情况下避免APP闪退。

>以上两个平台，应用商店中的图标无法代码处理，需要运营层面解决。
玩家从商店下载APP后，显示的是默认图标，要启动运行后，图标才会改变。

#### 直接构建Apk或Abb包体

如果Android构建时是直接导出包体，那么需要确保Plugins/Android目录下，至少存在LauncherManifest.xml或者AndroidManifest.xml其中之一。

## 附录

【在线Icon生成工具】https://icon.wuruihong.com/

