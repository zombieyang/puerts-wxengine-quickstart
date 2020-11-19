# 在unity里用puerts写微信小游戏

本项目基于 [Typescript解决方案puerts](https://github.com/Tencent/puerts) 和 [微信早前开源的minigame-adaptor](https://github.com/wechat-miniprogram/minigame-adaptor) 的js-adaptor部分。

你可以通过该项目尝试在unity里直接编写微信小游戏：）

用一张简单的架构图表明整个运作机制。
![arch](./doc/arch.png)

### 优势在哪？
目前把unity转小游戏有几种方案
1. wasm方案。
wasm转出来的游戏有比较好的还原度。性能也不错。
但劣势在于
    1. ios支持并不好
    2. 转出来的wasm文件无法调试
    3. 在加上流式加载能力之前，wasm文件非常大，不符合小游戏的产品形态。
2. 微信早前开源的[minigame-adaptor](https://github.com/wechat-miniprogram/minigame-adaptor)
采用的是Bridge将c#代码转为js，再加以js适配器适配到微信引擎的方案。
但劣势在于
    1. 语法转换有一些严格的限制，容易出错。
    2. 语法转换出来的代码能调试，但可读性较弱。
    3. js适配层有一定性能损耗。
3. 云游戏方案
比较超前，市面上目前没有开放的成熟方案。

本项目提供的方案里，客观地说：
1. 逻辑代码都是Typescript/Javascript，不存在调试和阅读的问题。
2. 如果你是有存量的unity游戏想转化，还是需要你用Typescript重写。但由于调试过程还在熟悉的unity引擎里，成本相对较低。
3. 依然存在js适配层带来的性能消耗。

### 如何使用？
你可以参考[使用指南](./doc/quickstart.md)

### 刚上线，持续施工中
1. 对于重载函数，在小游戏侧支持尚不完全。目前所有重载函数都只支持其中一种格式。需要考虑将[minigame-adaptor](https://github.com/wechat-miniprogram/minigame-adaptor)的js-adaptor抽出来重做
2. unity侧工作流有待完善。准备参考[Geequlim的startkit](https://github.com/Geequlim/puerts-starter-kit)
3. unity侧对behaviour的管理有待优化。
4. 完善远端资源管理能力，目前还是使用的resources目录机制。

### 贡献代码
可以提issue或者先找我聊方案，你在puerts官方群能找到我。
> QQ群：942696334

该方案在小游戏侧需要H5引擎支持，因此也欢迎任何H5引擎扶持该方案。