# MinecraftVsEnemies

## Info

MVE 第三代的版本, 使用 *godot4.0 .NET ver* 制作
- 第一代 *\(2019\~2020\)* 使用 *gm8* 引擎, 截止目前**最完善**, 但**仍不可**正常游玩
- 第二代 *\(2021\~2022\)* 使用 *gms2* 引擎, 截止目前**不完善**, 但**可稍微**正常游玩

上面两代均已种种原因**废弃**, 不出意外的话可能第三代会成为最后一个长期开发的版本

截止2023-5-25, 使用的 godot 版本为 `4.0.3 .net official build`

## License

源码使用 **LGPL 3.0** 协议, 这意味着在你游玩、与其通信、制作mod、作为类库等行为所使用时无需开源你所使用的源码,
当你修改了任何一部分本仓库代码时**必须**将你的**修改部分**以相同协议开源


## Repository

其中大部分项目主要使用 c# 编写, 即游戏的主体, 项目的 c++ 部分为`NativeLib`子项目, 主要用途目前只有在windows上打开本地文件选择对话框,
注意该dll不会自动复制, 自行 publish 时注意根据对应导出框架架构选择对应的架构(通常只有x86_32(32位)与x86_64(64位)之分)

### TODO

一些TODOs:
- 删除`NodeNotFoundException`, 取而代之的使用godot的设计哲学
>我们相信无论如何游戏都不应该崩溃。如果发生意外情况，Godot将打印一个错误（甚至可以追溯到脚本），但之后它会尽可能优雅地恢复，并继续前进。
- 删除`LawnEntity`与`BoardEntity`使用的`FindParent`, 取而代之的是在`AddChild`时赋值对应的`lawn`与`board`
- 使用`gdextensions`重写`SalParticlesSys`

### 开发入门

如果你不需要使用vs进行c#调试那么你可以忽略下面这部分.  
为了方便在vs中进行断点调试所以我们使用了一些配置文件指向Godot可执行文件路径和项目路径,
所以你需要更改部分设置以适应你的环境.  
具体首先你需要找到`envconfig.props`文件, 然后将`MVEGodotExecutablePath`更改为你的 Godot 可执行文件的路径, 将`MVEGodotProjectPath`更改为本仓库的根目录位置. 如果你还需要向 Godot 传递更多参数的话你可以在`MVEGodotLaunchArguments`加上你所需要的参数.