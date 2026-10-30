# 灾厄巫师

翡翠回廊第 5 波，在骷髅和兽人之后出场。拥有独立 Boss 血条、天气倒计时、四方向行走动画与脚下施法光环。

## 天气循环

| 阶段 | 时间 | 全图效果 |
| --- | --- | --- |
| 暴雪 | 12 秒 | 箭塔、法师塔、投石塔攻击速度变为 60% |
| 暂歇 | 3 秒 | 恢复正常，预告沙尘暴 |
| 沙尘暴 | 12 秒 | 包括 Boss 在内的所有怪物移动速度变为 135% |
| 暂歇 | 3 秒 | 恢复正常，预告暴雪 |

循环持续到巫师死亡或到达终点。游戏失败、重新开始和切换地图也会清除天气效果。冰霜塔是持续减速光环，没有攻击间隔，因此不受暴雪影响；其减速与沙尘暴的加速相乘。

## 默认参数与调整位置

- 生命 2400、护甲 30%、基础移速 0.72、漏怪扣血 8、击败奖励 180 金币。
- 数值：`Assets/Data/Enemies/TempestSorcerer.asset`。
- 天气时长和倍率：`Assets/Prefabs/TempestSorcerer.prefab` 的 `WeatherBossAbility`。
- 波次：翡翠回廊场景的 `WaveGameController`，最后一波 `bossesA = 1`。
- 快速试玩：运行翡翠回廊，在 `WaveGameController` 组件右上角菜单选择「开发预览/立即召唤灾厄巫师」，不用等到第五波。

美术使用内置 image_gen 生成，透明原图保存在 `Assets/Art/Enemies/TempestSorcerer/tempest-sorcerer-sheet-v1.png`，提示词见 `Docs/TempestSorcererArtPrompt.txt`。编辑器菜单 `Royal Fortress/接入灾厄巫师 Boss` 可重新切片和接入资源，会重设默认 Boss 数值与翡翠回廊最终波配置。
