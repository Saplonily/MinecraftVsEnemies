# MVE关卡文件格式

## 简单的介绍

嗯这里就是简单的介绍下目前关卡文件是什么样子的以及如何自制关卡  
首先呢我们需要进入自定义关卡, 方法很简单就是移动到关卡活板门上然后按**大键盘**上的1键即可选择一个.json文件(关卡文件)进行进入了.

## 错误处理

一个关卡文件肯定有出错的时候, 当mve无法解析这个关卡文件时会弹出一个异常对话框, 这表明你写入了一个错误的关卡文件或者json, 应该进行检查, 如果你确认没问题的话那你可以选择在群里提问, 因为不保证这不是屑sap的问题(

## 基础格式

一个最基础的关卡文件长这样:

```json
{
    "SceneId": "grasswalk",
    "TotalWaves": 20,
    "EnemiesSpawning": {
        "EnemyPool": [
            {
                "Id": "zombie",
                "Weight": 200,
                "Cost": 200
            }
        ],
        "PointsAddFactor": 50
    }
}
```
- 最外层的`SceneId`表示使用的场景, 目前只有`grasswalk`也就是默认的草坪.
- `TotalWaves`表示当前关卡的波次数量, 建议使用10的倍数
- `EnemiesSpawning`表示当前关卡的出怪设置
  - 其中的`PointsAddFactor`表示每波过去后总点数的增加量
  - `EnemyPool`表示出怪池
    - 其中一项出怪池配置单元中的`Id`表示这个所代指的僵尸, 目前有以下几项可选择:
      - `zombie` 普通 `leat_zombie` 皮革 `iron_zombie` 铁帽
    - `Weight`表示其出怪权重
    - `Cost`表示其出怪后的花费

至此最基础的一些配置就完工了, 如果你不清楚基础pvz中`Weight`,`Cost`这些词语你...可以去群里问(

未完待续... 2023-4-11 23:29:30