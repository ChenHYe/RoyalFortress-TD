# 导出和分享可游玩版本

当前工程：`C:\Users\chy\My project`，Unity `6000.3.26f1`。本次选择 Windows 64 位版本，解压后可以运行，不要求游玩者安装 Unity。

## 打开这次的版本

导出文件夹为 `D:\游戏\Builds\RoyalFortress-Windows-v1`，分享包为同级的 `RoyalFortress-Windows-v1.zip`。

1. 收到 ZIP 后，右键选择“全部解压”，解压完整个文件夹。
2. 进入解压后的文件夹，双击 `RoyalFortress.exe`。
3. 左键点击地图石台上的加号，选择法师塔建造。
4. 点右下角“开始第 1 波”，敌人会沿固定路线前进，到 Home 后生命减 1。
5. 滚轮缩放地图，中键/右键拖动，WASD 或方向键平移，Home 恢复全图。
6. 点击窗口右上角关闭按钮或按 Alt+F4 退出。

当前是 1280×720 可调整大小的窗口。演示包含 7 个建塔点、法师塔建造与待机动画、三波敌人、生命与波次 UI。其他三种塔未开放；法师塔攻击、金币和升级尚未加入。

## 发给组员哪些文件

请发送整个 ZIP，而不是只发 exe。Unity Windows 程序需要旁边的 `UnityPlayer.dll`、`RoyalFortress_Data` 等运行文件；`MonoBleedingEdge` 等目录也应随构建结果一起保留。不要从 ZIP 预览里单独双击 exe，先全部解压。

组员只想玩时不需要工程，也不需要 `.cs`、原始 PNG、Unity Hub 或 Editor；组员需要继续开发时，才需要 Unity 工程与相同版本的编辑器。

## 以后自己再次导出

1. 退出 Unity Play 模式，按 Ctrl+S 保存场景。
2. 打开顶部菜单 File → Build Profiles。较旧版本显示 Build Settings。
3. 平台选择 Windows，架构选择 Intel 64-bit / x86_64。如果该平台需要切换，点 Switch Platform / Switch Profile 并等待完成；缺少支持模块时在 Unity Hub 的 Editor 安装项中添加相应模块。
4. 打开 Scene List，确认 `Assets/Scenes/RoyalFortressGameplay.unity` 被勾选并排在第一位。若只导出这个演示，可以取消勾选原始 RoyalFortress 与 SampleScene，避免额外打包展示场景。
5. 正常分享版本不勾选 Development Build。
6. 点击 Build，选择新的版本文件夹，例如 `D:\游戏\Builds\RoyalFortress-Windows-v2`，不要选择 Assets 或覆盖原始工程文件。
7. 构建结束后，进入输出目录运行 exe，检查建塔、波次与界面，再将整个目录压缩为 ZIP 分享。

Unity 工程里的修改不会自动同步到之前导出的 exe；每次需要分享新内容，都要重新构建。Windows 版给 Windows 电脑使用；如果以后要让别人直接在网页上玩，需要另选 Web 平台构建并部署到网页服务器。

官方参考：[Windows 构建文件说明](https://docs.unity3d.com/6000.3/Documentation/Manual/WindowsStandaloneBinaries.html)、[Build Profiles](https://docs.unity3d.com/6000.3/Documentation/Manual/build-profiles.html)。

## 本次验证方式

通过 Unity CLI 在已打开的编辑器中构建，明确只包含 RoyalFortressGameplay 场景。导出后另外启动独立 Windows 程序验证。构建结果、运行日志和检查记录位于 `D:\游戏\output\unity-export-verification`，具体通过情况以记录为准。
