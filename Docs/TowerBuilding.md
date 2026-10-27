# 金币与拖卡建塔

在 Unity 6000.3.27f1 打开 `Assets/Scenes/RoyalFortressGameplay.unity` 后点击 Play。

## 操作

顶部的小型悬浮条显示金币、基地生命和波次；底部常驻箭塔、冰霜塔、法师塔、投石塔四张半透明卡片。
按住卡片拖到地图上发光的空石台，松手购买。拖动时有塔体与攻击范围预览：
绿色表示有效坑位，红色表示不能放置。放到空白处、已占用坑位或 UI 上不扣钱。
右键、窗口失去焦点可取消拖动；Esc 会取消拖动并打开暂停菜单。金币不足的卡片变暗并禁止建造。
拖卡时暂停相机操作，避免与地图拖动冲突。地图铺满窗口，滚轮以鼠标位置为中心平滑缩放，
左键按住地图拖动，WASD/方向键平移，Home 或右下角“复位”恢复初始取景。
右下角还提供 + / − 按钮与缩放百分比。指针位于塔卡、状态条或按钮上时不触发地图缩放/拖动。
缩放或平移后若坑位暂时离开屏幕，可以拖动地图重新找到它。

右下方按钮开始下一波。准备阶段和战斗中都可以建塔；胜负结算后停止建造，
点击“重新开始”恢复初始金币、生命、空坑位和波次。

## 第一版经济参数

| 项目 | 金币 |
|---|---:|
| 王国边境开局资金 | 300 |
| 翡翠回廊开局资金 | 380 |
| 箭塔 | 90 |
| 冰霜塔 | 110 |
| 法师塔 | 140 |
| 投石塔 | 160 |
| 击杀骷髅兵奖励 | 18 |
| 击杀重甲兽人奖励 | 40 |
| 击杀灾厄巫师奖励 | 180 |

开局可以选三座箭塔（剩余30），法师加投石（剩余0），或法师加冰霜（剩余50）。
这是一版起步数值，尚未做完整关卡平衡。漏怪不奖励金币。
金币只在成功建造时扣除，失败退回；击杀奖励和扣血复用原有敌人去重结算。
尚未提供升级、拆塔、出售或退款操作。

## 在 Inspector 调整

- `Game Controller / WaveGameController.startingGold`：初始金币。
- `Tower Build Controller / TowerBuildController`：四种塔的价格。
- `Assets/Data/Enemies/Skeleton.asset` 与 `ArmoredOrc.asset`：`goldReward`。
- `Build Sites` 下七个 `BuildSite`：坑位碰撞体、塔底锚点与高亮轮廓。

四张塔卡使用已有塔立绘。HUD 在 Play 时由 `FortressHud` 创建，
旧场景 UI 子对象保留但停用；地图相机使用完整视口，UI 直接悬浮在场景上。
卡片间的透明空隙不阻挡地图操作。建成的塔位于 `Built Towers` 下。
新地图沿用该交互，只需配置地图边界、路线、坑位与波次；详见 MapNavigation.md。

## 验证入口

`FortressBuildVerification.Build` 可从 Unity 命令行构建 Windows 验证程序，
输出在工程外 `../ReviewArtifacts/TowerEconomy/Player/`。
程序加 `-fortress-smoke-test -fortress-test-output <目录>` 会执行金币、
UI 拖放、取消、占位、击杀去重、漏怪与重开检查，输出 JSON 和实际游戏截图。
测试入口在正常游玩时不运行；悬浮UI与地图导航版的63项独立程序验收已通过，详细范围见 EconomyVerification.txt。
