# 在unity里用Typescript写微信小游戏

本项目基于 [Typescript解决方案puerts](https://github.com/Tencent/puerts) 和 [微信早前开源的minigame-adaptor](https://github.com/wechat-miniprogram/minigame-adaptor) 的js-adaptor部分。

让你可以通过该项目在unity里用Typescript直接编写微信小游戏，代码完全复用：）

用一张简单的架构图表明整个运作机制。

![arch](./doc/arch.png)

## 如何使用？
你可以参考[使用指南](./doc/quickstart.md)

## 有更复杂的例子吗
目前有一个稍复杂一些的[坦克demo](https://github.com/xosuperpig/puerts-wxengine-example)

------------------------------------------------------------------------

### 刚上线，持续施工中
1. 对于重载函数，在小游戏侧支持尚不完全。目前所有重载函数都只支持其中一种格式。需要考虑将[minigame-adaptor](https://github.com/wechat-miniprogram/minigame-adaptor)的js-adaptor抽出来重做
2. unity侧工作流有待完善。准备参考[Geequlim的startkit](https://github.com/Geequlim/puerts-starter-kit)
3. unity侧对behaviour的管理有待优化。
4. 完善远端资源管理能力，目前还是使用的resources目录机制。

### 为本项目贡献
可以提issue或者先找我聊方案，你在puerts官方群能找到我。
> QQ群：942696334

该方案在小游戏侧需要JS游戏引擎支持，因此也欢迎任何JS游戏引擎扶持该方案。