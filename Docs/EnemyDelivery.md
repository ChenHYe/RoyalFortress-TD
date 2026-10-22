# 骷髅兵与重甲兽人交付说明

在 Unity 6000.3.27f1 打开 Assets/Scenes/RoyalFortressGameplay.unity，点击 Play，在地图石台建造法师塔和冰霜塔，再开始波次。

## 两种敌人

| 参数 | 骷髅兵 | 重甲兽人 |
|---|---|---|
| 生命 | 125 | 320 |
| 护甲减伤 | 15% | 60% |
| 移速（世界单位/秒） | 1.25 | 0.72 |
| 漏过扣基地生命 | 1 | 2 |
| 亡灵标记 | 是 | 否 |
| 行走素材 | 原工程四方向、每方向8帧 | 新增四方向、每方向4帧 |

这些数值是可玩的起步配置，尚未经过完整关卡平衡。兽人普通40点伤害实际承受16点；75%穿甲的40点魔法伤害实际承受34点。冰霜减速45%后兽人速度0.396。亡灵标记已预留，目前没有额外亡灵特攻系统。

## 已实现

两种敌人有独立配置资源、预制体、四方向行走、路径移动、血条、受击闪色、减速变蓝、减速时动画同步变慢，以及死亡压扁淡出。击杀立即注销波次，尸体短暂淡出不阻塞下一波；到达终点按怪物类型扣血，单只敌人不会重复结算。死亡表现采用程序动画，没有额外绘制死亡序列。

第一波4只骷髅；第二波4只骷髅+2只兽人；第三波5只骷髅+3只兽人，墓穴入口在第三波开放。HUD 显示击败计数。

## 主要交付位置

- Assets/Prefabs/SkeletonEnemy.prefab
- Assets/Prefabs/ArmoredOrcEnemy.prefab
- Assets/Data/Enemies/Skeleton.asset
- Assets/Data/Enemies/ArmoredOrc.asset
- Assets/Art/Characters/ArmoredOrc/ArmoredOrcWalkSheet.png
- Assets/Animations/ArmoredOrc/
- Assets/Scripts/Enemies/
- Assets/Scripts/Gameplay/WaypointEnemy.cs
- Assets/Scripts/Gameplay/WaveGameController.cs

在 Inspector 修改 Data/Enemies 下的配置即可调整属性。动画前缀必须与 Animator 中状态名一致。WaveGameController 的 countA/B/C 表示骷髅数量，orcsA/B/C 表示兽人数量。

移动由 WaypointEnemy 负责；EnemyAgent 不接收内部路径，负责生命、护甲、减速与目标注册。EnemyAgent 新增 destroyOnDeath 开关，地图敌人关闭自动销毁，交由死亡表现结束后清理；双塔自带演示保持默认自动销毁行为。金币、掉落、升级、亡灵复活和近战拆塔不属于本次实现。

## 美术来源

骷髅沿用地图工程现有素材。兽人用内置 image_gen 工具生成，原始透明图1254×1254，按4×4网格导入为16张精灵，PPU240、Point过滤、无压缩。属于AI生成像素风素材，不是严格原生48×48手画像素；后续可替换图片与动画，保留接口和配置。

生成提示词见 ArmoredOrcArtPrompt.txt。原压缩包未改动；脚本改动前备份与运行验证记录位于工程外 ../EnemyWork。
