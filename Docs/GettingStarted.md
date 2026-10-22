# Unity 新手操作：从项目到要塞场景

后续已增加路径与波次场景 Assets/Scenes/RoyalFortressGameplay.unity，操作见 [路径、波次与生命说明](unity-paths-waves-guide.md)。下面的 RoyalFortress 场景仍保留为原始素材展示。

你当前已经有一个可打开的 Unity 项目。我们用 Unity CLI 在实际编辑器中导入了素材并生成场景，可以先操作它，再对照学习制作过程。

## 现在打开哪一个项目

- Unity 版本：`6000.3.26f1`。
- 项目名：`My project`。
- 项目目录：`C:\Users\chy\My project`。
- 场景：`Assets/Scenes/RoyalFortress.unity`。
- 原始素材仍保留在 `D:\游戏\output\`；Unity 工程使用的是复制到其 `Assets/Art/` 内的 PNG。

通过 Unity Hub 的 Projects 列表打开 My project。若列表找不到，在 Hub 使用 Add / 从磁盘添加项目，选择上述项目文件夹。进入编辑器后，在底部 Project 面板依次展开 Assets → Scenes，双击 RoyalFortress。

## 先认识五个位置

| 名称 | 通常的位置 | 你在这里做什么 |
|---|---|---|
| Hierarchy | 左侧 | 看当前场景有哪些对象，例如相机、背景、塔 |
| Scene | 中间的标签页 | 编辑地图，选中和移动对象 |
| Game | 中间的另一个标签页 | 看玩家通过相机看到的画面 |
| Project | 底部 | 找图片、场景、动画、脚本和预制体文件 |
| Inspector | 右侧 | 修改当前选中的对象或文件的属性 |

如果面板布局不同，以面板名称为准。Scene 里的滚轮是编辑视图操作；Game 里的滚轮才是我们写的游戏相机操作。

## 第一次运行

1. 双击 RoyalFortress 场景，点 Game 标签。
2. 点编辑器顶部的三角形 Play 按钮，进入运行模式。
3. 先在 Game 画面中点一下，让它获得键盘输入焦点。
4. 滚轮向前放大。放大后，按住鼠标中键或右键拖动地图；也可以用 WASD 或方向键移动。
5. 按 Home 回到全图视野。
6. 再点一次 Play，退出运行模式，然后按 Ctrl+S 保存编辑状态。

默认是全图视野。这时地图已经在视野内，相机边界会限制拖动；要体验平移，先放大。地图是 3:2，Game 如果是 16:9，概览两侧有深色留边是正常现象，地图没有被拉伸。

运行时可看到骷髅原地行走、塔顶法师待机。这个场景目前用来预览素材与相机，没有沿路移动、波次、攻击目标、投射物或伤害系统。

## 看看场景由什么组成

在左侧 Hierarchy 中依次点击：

- `Map Background`：整张背景图片。右侧 Transform 控制它的位置，Sprite Renderer 控制显示的图片及排序。
- `Main Camera`：游戏相机。Camera 的 Size 是正交相机显示范围，数值越小看起来越近。Map Camera Controller 是我们添加的拖动/缩放脚本。
- `Demo Units` → `Skeleton Preview`：骷髅，Sprite Renderer 显示图片，Animator 播放行走帧。
- `Demo Towers` → `Arcane Mage Tower`：魔法塔，Animator 默认播放 Idle。
- `Level Markers (positions only)`：三个出生点和 Home 的位置标记。它们是空对象，没有图像，也没有出生、波次或扣血逻辑；位置仍需后续关卡设计确认。

选中对象后，把鼠标移到 Scene 标签内，按 F 可以把编辑视图聚焦到这个对象。按 W 是移动工具，可拖动箭头改变对象位置。

## 再放一座塔试试

1. 先退出 Play 模式。
2. 在底部 Project 找到 Assets → Prefabs。
3. 把 `ArcaneMageTower` 从 Project 拖到 Scene 画面里。
4. 按 W，把塔底放在一个空石台上。当前背景石台只是画出来的图案，不会自动吸附。
5. 按 Ctrl+S 保存，再进入 Play 查看效果。

Prefab（预制体）是保存好的对象模板，里面已经配置图片、组件和动画。复制实例时不用重新设置所有素材。`SkeletonPreview` 也可以这样拖入。

退出 Play 后，运行时临时移动的对象会恢复。需要保留的摆放与参数修改，请在退出 Play 后完成。

## 图片的导入参数在哪里

不要点 Hierarchy 的背景对象，改为点底部 Project → Assets → Art → Maps 中的 PNG 文件。此时右侧 Inspector 显示的是图片导入设置：

| 参数 | 这次设置 |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 64 |
| Filter Mode | Point (no filter) |
| Compression | None |
| Generate Mip Maps | 关闭 |
| Max Size | 2048 |
| Pivot | 背景 Center；角色和整塔自定义 (0.5, 0.09375) |

背景 1536×1024 没有导入缩小，在当前 PPU 下是 24×16 个世界单位。修改导入设置后要点 Apply。背景与角色使用相同 PPU 作为当前比例起点，但美术比例还要通过实际画面判断。

素材目录：

```text
Assets/
  Art/
    Maps/
    Characters/Skeleton/Walk/
    Towers/ArcaneMage/Idle/
    Towers/ArcaneMage/Attack/
  Animations/
  Materials/
  Prefabs/
  Scenes/RoyalFortress.unity
  Scripts/MapCameraController.cs
```

## 以后自己从零创建项目

1. 打开 Unity Hub。五名成员先统一 Editor 完整版本号，本例为 6000.3.26f1。
2. 若尚未安装，在 Installs → Install Editor 安装该版本。Hub 是项目管理器，Editor 是实际制作游戏的软件。
3. 到 Projects → New project，选对应 Editor 版本。
4. 模板选 `Universal 2D`；部分版本显示 `2D (URP)`。这是带二维渲染配置的模板，能制作当前这种斜俯视图片场景。
5. 输入项目名，例如 RoyalFortressTD，并选择项目存放位置。缺少模板时先 Download template，再 Create project。
6. 第一次打开要等待下载包、导入与编译完成。
7. 在 Project 的 Assets 下新建 Scenes、Art、Scripts、Prefabs 等文件夹。
8. File → Save As，将当前场景保存到 Assets/Scenes 下。
9. 把 PNG 拖进 Project 的 Assets/Art 目录，按上表设置并 Apply。
10. 把背景 PNG 从 Project 拖入 Scene，设置对象 Transform 的 Position=(0,0,0)、Scale=(1,1,1)，Sprite Renderer 的 Order in Layer=-100。
11. 相机使用 Orthographic，位置 (0,0,-10)、Rotation=(0,0,0)。同样的 1536×1024 背景、PPU64 时，Size=8 可先看全图。
12. 将角色和塔放到场景；动画、相机控制和关卡逻辑分别配置。空项目不会自动具备这些功能。本次工程中已代为配置素材动画和相机控制。

## 本次实际做过的验证

在 Unity Play 模式中确认游戏帧持续前进、骷髅精灵帧变化，以及塔的 Cast 动画可以播放后返回 Idle。41 张 PNG 的 Sprite、PPU、Point 过滤、无压缩和 mipmap 设置已检查，背景保持原生尺寸。通过内部 Input System 合成输入与显式组件更新检查了滚轮缩放、键盘平移、中键拖动和地图边界；直接调用检查 ResetView，未进行真实鼠标键盘操作验收。

检查结束时 Console 没有编译或运行错误，场景已保存并退出 Play。可复核记录在 `D:\游戏\output\unity-starter\unity-verification.json`，实际 Unity 画面在同目录的 `royal-fortress-unity-preview.png`。

背景仍为整图，树、墙和王宫的遮挡没有拆层；出生点是位置草稿。当前教学场景的用途是建立“项目—素材—场景对象—相机—运行”的认识。
