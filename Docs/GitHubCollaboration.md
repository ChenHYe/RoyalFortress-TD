# 同学 Fork 与协作

1. 在 GitHub 仓库页面点击 Fork，创建自己账号下的副本。
2. Clone 自己的副本到本地。Unity Hub 从磁盘添加仓库根目录，应直接看到 Assets、Packages、ProjectSettings。
3. 统一使用 Unity 6000.3.27f1，等待依赖下载与资源导入。
4. 打开 Assets/Scenes/RoyalFortressGameplay.unity，再点击 Play。
5. 修改前创建功能分支，例如 feature/arrow-tower。提交时保留对应 .meta 文件。
6. Push 到自己的 Fork，再向原仓库 main 分支创建 Pull Request，写明改动与测试结果。

开始新任务前同步上游仓库。尽量分工修改不同 Prefab 和脚本；多人需要修改同一个场景时先协调，避免互相覆盖场景文件。

提交 Assets、Packages、ProjectSettings、Docs、README.md 和仓库配置。不要提交 Library、Temp、Logs、UserSettings、IDE 缓存或旧的 ZIP。分享可执行程序时单独使用 Release，开发协作使用源码。

两种怪物的配置与验证记录见 EnemyDelivery.md、EnemyVerification.txt。兽人素材的生成提示词见 ArmoredOrcArtPrompt.txt。
