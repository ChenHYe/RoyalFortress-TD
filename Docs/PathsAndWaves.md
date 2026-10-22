# 路径、波次与王宫生命：操作说明

已继续加入点击地基建造法师塔，见 [地基建塔操作说明](unity-tower-building-guide.md)。本页的波次和路线操作继续适用。

项目：C:/Users/chy/My project；Unity 6000.3.26f1。

新场景：Assets/Scenes/RoyalFortressGameplay.unity。原 RoyalFortress.unity 保留为素材展示。请打开新场景运行。

## 为什么不用道路掩膜

敌人只沿固定路线前进，用“出生点 → 道路中心拐点 → 共同路段 → Home”的路径点即可。图片里的道路负责视觉，Unity 中的位置点负责移动；不必描道路两侧边界，也不需要可行走掩膜、NavMesh 或道路碰撞体。出生点、Home 也是空对象位置标记，不要求另外画 PNG。

## 运行

1. 底部 Project → Assets → Scenes，双击 RoyalFortressGameplay。
2. 顶部 Play，点 Game，点击右下角“开始第 1 波”。
3. 骷髅从墓地和废村出发，沿路汇合后走向王宫。
4. 每只到 Home 后消失，生命减 1。本波出怪完成、场上敌人全部离开后，下一波按钮才可用。
5. 第 3 波墓穴入口加入出怪。生命为 0 显示王宫失守并停止关卡；全部波次结束后可重新开始。

演示值：初始生命 20，三波为 4、6、8 只。前两波 A/B 分别各 2、各 3 只；第 3 波 A/B 各 3 只、C 为 2 只。魔法塔目前仍待机，没有攻击或伤害逻辑，所以敌人会漏进 Home；数值尚未做平衡。

相机滚轮缩放，中键/右键拖动，WASD/方向键移动，Home 恢复。概览时先放大再拖动。左上 UI 使用屏幕坐标，不随地图移动、缩放。

## 修改出生点和 Home

1. 退出 Play，切 Scene，打开 2D 与 Gizmos。
2. 左侧 Hierarchy 展开 Level Markers，选 Spawn A - Graveyard、Spawn B - Ruined Village、Spawn C - Sealed Crypt 或 Home - Palace Gate。
3. 鼠标移到 Scene 按 F 聚焦，按 W 用箭头拖动标记，保持 Z=0。
4. Ctrl+S 保存，再 Play 检查。移动出生点后检查与下一个点的连线，移动 Home 会修改三条路线的最终目标。

## 修改路线

Hierarchy → Routes：Route A - Graveyard 为墓地路线，Route B - Village 为废村路线，Route C - Crypt (wave 3) 为墓穴路线，Shared Road to Home 为共同路段。

Scene 中青色为 A、金色为 B、紫色为 C；共同部分的线会重叠。线与小圆点是编辑辅助，不会显示在正常游戏中。

展开路线，选 A 00、B 00、C 00 或 Shared 00 等子节点，按 W 拖到道路中央。拐弯处用多个点让折线贴合道路。不必画掩膜。不要整体拖动 Routes 或路线父物体，否则会移动一批节点。

Inspector 的 Points 数组决定行走顺序，节点名只是标记。增加节点时可创建空对象，再把它插到 Points 正确位置。C 复用 A 的后半段，所有路线共用 Shared 路段，所以改共同点会同时影响其他路线。

现在的点位根据背景估算，你们可以微调。敌人出生时复制路线位置，因此请退出 Play 后改点，再重新测试。角色按主要移动方向切换四方向行走动画；斜向路段暂用最接近方向。

## 修改波次和 UI

- 选 Game Controller：Starting Lives 为初始生命，Branch Unlock Wave 为墓穴开放波次，Waves 为各波配置。
- 展开每波：Count A/B/C 为入口数量，Spawn Interval 为每轮出怪间隔。前两波 Count C 保持 0，不可在开放波次前安排 C 出怪。
- 展开 HUD Canvas：Lives and Wave Panel 包含生命、波次、状态和墓穴说明；Start Next Wave 是开始按钮。
- 字体运行时使用系统微软雅黑/黑体，其他平台可改为团队统一字体。

墓穴封印仍画在背景里，第 3 波开放目前表示开始出怪，尚未做封印消失动画；树墙遮挡也还没拆层。

## 已检查

在 Unity Play 模式中检查了 UI 事件开始出怪、实际移动、波次数量、重复开始拦截、到 Home 只扣 1、重复到达保护、UI 更新、等待清场、第 3 波支路出怪、全部波次结束、零生命失败。单独让第一波在真实 Update 中自然走完整条路，观察生命从 20 到 19，随后为 18，场上仍有 2 只。Console 无编译与运行错误。

部分边界检查使用合成 UI 点击事件、显式走完路线；未进行真实鼠标点击验收。记录与截图在 D:/游戏/output/unity-gameplay/。

