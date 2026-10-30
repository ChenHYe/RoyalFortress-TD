# 双塔地图接入说明

打开 Assets/Scenes/RoyalFortressGameplay.unity，点击 Play，从底部拖出箭塔、投石塔、法师塔或冰霜塔卡片，放到空石台上购买建造，再点击开始波次。经济参数见 TowerBuilding.md。

箭塔快速单体射击（低穿甲）；投石塔命中后半径 1.5 溅射；法师塔发射追踪飞弹并穿透 75% 护甲；冰霜塔对范围内全部敌人减速 45%，出圈即恢复，多个光环取最强效果。建塔扣金币，击杀敌人获得金币；升级仍未实现。

接入文件：
- Assets/_Project/TwinTowers：塔模块、参数、原始预制体和演示场景（含箭塔/投石塔）。
- Assets/Prefabs/IntegratedTowers：地图使用的四座塔与投射物预制体，塔体高度调整为约 2 世界单位，使用地图现有 URP Sprite 材质；光环范围与塔体缩放分开。
- Assets/Scripts/Gameplay/WaypointEnemy.cs：保留地图路径移动、方向动画及终点扣血，使用 EnemyAgent 管理生命、护甲和速度，死亡注销当前波次。
- EnemyAgent 新增 SetExternalProgress，使地图路径进度参与优先索敌；路径为 null 时不覆盖外部进度。
- TowerBuildController：四座塔建造入口及预制体引用。
- SkeletonEnemy：增加 EnemyAgent。当前骷髅起步数值为生命 125、护甲 15%、速度 1.25，尚未做关卡平衡。

Unity 版本：6000.3.27f1，URP 2D。原始 ZIP 未修改。原地图场景与脚本备份在工程外 ../IntegrationWork。

这些更改保存到 Unity 开发工程；原 RoyalFortress-Windows-v1.zip 不会自动更新，需要重新构建才能分享新可执行文件。
