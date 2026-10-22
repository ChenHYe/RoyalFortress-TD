using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    public sealed class TwinTowerDemo : MonoBehaviour
    {
        public TowerBehaviour mageTower, frostTower;
        public EnemyAgent enemyPrefab;
        public Camera sceneCamera;
        [Min(0.2f)] public float spawnInterval = 2.4f;
        public bool autoSpawn = true;
        public bool armoredEnemies = true;
        public bool showRanges = true;
        public int Defeated { get; private set; }
        public int Escaped { get; private set; }
        public bool IsPaused { get; private set; }
        private float nextSpawn;
        private TowerBehaviour selected;
        private Font chinese;
        private GUIStyle title, label, small, heading, button;
        private Texture2D white;
        private readonly Color panel = new Color(0.045f, 0.075f, 0.085f, 0.96f);
        private readonly Color muted = new Color(0.62f, 0.72f, 0.69f);
        private readonly Color gold = new Color(0.95f, 0.8f, 0.44f);

        private void Start()
        {
            selected = mageTower;
            nextSpawn = Time.time + 0.7f;
            chinese = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", "Arial" }, 20);
            white = Texture2D.whiteTexture;
        }
        private void Update()
        {
            if (autoSpawn && !IsPaused && Time.time >= nextSpawn && EnemyAgent.Active.Count < 35)
            { SpawnEnemy(); nextSpawn = Time.time + Mathf.Max(0.2f, spawnInterval); }
        }
        public EnemyAgent SpawnEnemy()
        {
            if (!enemyPrefab) return null;
            EnemyAgent enemy = Instantiate(enemyPrefab);
            enemy.name = armoredEnemies ? "ArmoredTrainingDummy" : "TrainingDummy";
            enemy.Initialize(armoredEnemies ? 220 : 125, armoredEnemies ? 60 : 0, 1.55f,
                new[] { new Vector3(-8.6f, -1.75f, 0), new Vector3(8.6f, -1.75f, 0) });
            enemy.Died += OnDefeated;
            enemy.ReachedExit += OnEscaped;
            return enemy;
        }
        private void OnDefeated(EnemyAgent e) { Defeated++; }
        private void OnEscaped(EnemyAgent e) { Escaped++; }
        public void SetPaused(bool value) { IsPaused = value; Time.timeScale = value ? 0 : 1; }
        public void ResetDemo()
        {
            SetPaused(false);
            foreach (EnemyAgent enemy in FindObjectsOfType<EnemyAgent>())
            { enemy.gameObject.SetActive(false); Destroy(enemy.gameObject); }
            foreach (MagicProjectile projectile in FindObjectsOfType<MagicProjectile>()) Destroy(projectile.gameObject);
            foreach (ArcaneBurst hit in FindObjectsOfType<ArcaneBurst>()) Destroy(hit.gameObject);
            mageTower.ResetCombat(); frostTower.ResetCombat();
            Defeated = Escaped = 0;
            nextSpawn = Time.time + 0.7f;
        }
        private void OnDisable() { Time.timeScale = 1; }
        private void OnDestroy() { if (chinese) Destroy(chinese); }

        private void InitStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { font = chinese, fontSize = 30, fontStyle = FontStyle.Bold };
            title.normal.textColor = Color.white;
            label = new GUIStyle(GUI.skin.label) { font = chinese, fontSize = 17 };
            label.normal.textColor = new Color(0.87f, 0.92f, 0.88f);
            small = new GUIStyle(label) { fontSize = 14 };
            small.normal.textColor = muted;
            heading = new GUIStyle(label) { fontSize = 20, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { font = chinese, fontSize = 17, alignment = TextAnchor.MiddleCenter };
            button.normal.textColor = Color.white;
        }
        private void Fill(Rect rect, Color color)
        { Color previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, white); GUI.color = previous; }
        private bool Btn(Rect rect, string text, Color color)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool clicked = GUI.Button(rect, text, button);
            GUI.backgroundColor = previous;
            return clicked;
        }
        private void OnGUI()
        {
            if (!mageTower || !frostTower || !sceneCamera) return;
            InitStyles();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            Fill(new Rect(0, 0, 1280, 102), panel);
            GUI.Label(new Rect(28, 15, 510, 42), "王国边境  /  双塔试验场", title);
            GUI.Label(new Rect(30, 63, 700, 28), "法师塔 · 奥术飞弹     冰霜塔 · 范围减速光环     /     独立 Unity 模块", small);
            GUI.Label(new Rect(930, 22, 320, 26), "击败 " + Defeated + "     抵达出口 " + Escaped, label);
            GUI.Label(new Rect(930, 54, 320, 26), "存活 " + EnemyAgent.Active.Count + "     " + (IsPaused ? "已暂停" : "正在演示"), small);

            if (showRanges) DrawRange(mageTower);
            LabelWorld(frostTower.transform.position + Vector3.down * 0.25f, "冰霜塔", new Color(0.45f, 0.9f, 1));
            LabelWorld(mageTower.transform.position + Vector3.down * 0.25f, "法师塔", new Color(0.85f, 0.64f, 1));
            foreach (EnemyAgent e in EnemyAgent.Active) DrawEnemy(e);

            Fill(new Rect(22, 117, 267, 200), panel);
            TowerDefinition cfg = selected ? selected.definition : mageTower.definition;
            GUI.Label(new Rect(38, 128, 230, 30), cfg.displayName + "  /  参数速览", heading);
            if (cfg.kind == TowerKind.Frost)
            {
                GUI.Label(new Rect(38, 168, 240, 26), "光环半径 " + cfg.range.ToString("0.##") + "  ·  减速 " + (cfg.slowFraction*100).ToString("0") + "%", label);
                GUI.Label(new Rect(38, 201, 230, 26), "当前圈内 " + frostTower.AffectedCount + " 个敌人", label);
                GUI.Label(new Rect(38, 234, 230, 26), "持续影响全部敌人，出圈恢复", small);
            }
            else
            {
                GUI.Label(new Rect(38, 168, 240, 26), "伤害 " + cfg.damage.ToString("0.#") + "  ·  间隔 " + cfg.attackInterval.ToString("0.##") + " 秒", label);
                GUI.Label(new Rect(38, 201, 230, 26), "射程 " + cfg.range.ToString("0.#") + "  ·  穿甲 " + (cfg.armorPenetration * 100).ToString("0") + "%", label);
                GUI.Label(new Rect(38, 234, 230, 26), "奥术核心 · 拖尾 · 命中扩散", small);
            }
            GUI.Label(new Rect(38, 275, 230, 25), "在 Unity 的 Data 文件夹中调参", small);

            Fill(new Rect(0, 532, 1280, 188), panel);
            GUI.Label(new Rect(28, 546, 700, 26), "控制台   /   先观察冰霜减速，再比较法师穿甲", heading);
            if (Btn(new Rect(28, 589, 138, 42), IsPaused ? "继续演示" : "暂停演示", new Color(0.24f, 0.36f, 0.32f))) SetPaused(!IsPaused);
            if (Btn(new Rect(178, 589, 138, 42), "生成一个敌人", new Color(0.24f, 0.36f, 0.32f))) SpawnEnemy();
            if (Btn(new Rect(328, 589, 116, 42), "重置试验", new Color(0.4f, 0.31f, 0.22f))) ResetDemo();
            if (Btn(new Rect(461, 589, 159, 42), "冰霜塔 " + (frostTower.attackEnabled ? "开启" : "关闭"), new Color(0.18f, 0.38f, 0.5f)))
            { frostTower.attackEnabled = !frostTower.attackEnabled; selected = frostTower; }
            if (Btn(new Rect(632, 589, 159, 42), "法师塔 " + (mageTower.attackEnabled ? "开启" : "关闭"), new Color(0.39f, 0.23f, 0.51f)))
            { mageTower.attackEnabled = !mageTower.attackEnabled; selected = mageTower; }
            if (Btn(new Rect(803, 589, 163, 42), armoredEnemies ? "护甲：60%" : "护甲：0%", new Color(0.3f, 0.34f, 0.31f))) armoredEnemies = !armoredEnemies;
            if (Btn(new Rect(978, 589, 132, 42), autoSpawn ? "自动生成：开" : "自动生成：关", new Color(0.3f, 0.34f, 0.31f))) autoSpawn = !autoSpawn;
            if (Btn(new Rect(1122, 589, 130, 42), "范围 " + (showRanges ? "显示" : "隐藏"), new Color(0.3f, 0.34f, 0.31f))) showRanges = !showRanges;
            GUI.Label(new Rect(30, 648, 1240, 26), "冰霜圆内全部敌人持续减速，出圈恢复；紫色奥术飞弹造成穿甲伤害。护甲选择仅影响新敌人。", small);
            GUI.Label(new Rect(30, 678, 1230, 25), "用途：双塔功能和接入验证。此场景为试验场，不代表完整游戏关卡。", small);
            GUI.matrix = Matrix4x4.identity;
        }
        private Vector2 ToUI(Vector3 world)
        {
            Vector3 v = sceneCamera.WorldToViewportPoint(world);
            return new Vector2(v.x * 1280, (1 - v.y) * 720);
        }
        private void LabelWorld(Vector3 position, string text, Color color)
        {
            Vector2 v = ToUI(position);
            GUIStyle centered = new GUIStyle(heading) { alignment = TextAnchor.MiddleCenter };
            centered.normal.textColor = color;
            GUI.Label(new Rect(v.x - 100, v.y, 200, 30), text, centered);
        }
        private void DrawEnemy(EnemyAgent enemy)
        {
            if (!enemy || !enemy.IsAlive) return;
            Vector2 p = ToUI(enemy.transform.position + Vector3.up * 0.65f);
            Fill(new Rect(p.x - 21, p.y, 42, 5), new Color(0.1f, 0.12f, 0.11f));
            Fill(new Rect(p.x - 21, p.y, 42 * enemy.Health / enemy.maxHealth, 5), enemy.SpeedMultiplier < 1 ? Color.cyan : gold);
        }
        private void DrawRange(TowerBehaviour tower)
        {
            if (!tower.definition) return;
            Vector2 prev = ToUI(tower.transform.position + Vector3.right * tower.definition.range);
            Color color = tower.definition.magicColor; color.a = 0.22f;
            for (int i = 1; i <= 80; i++)
            {
                float a = i * Mathf.PI * 2 / 80;
                Vector2 next = ToUI(tower.transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * tower.definition.range);
                Vector2 delta = next - prev;
                Matrix4x4 before = GUI.matrix;
                GUI.matrix = before * Matrix4x4.TRS(new Vector3(prev.x, prev.y, 0),
                    Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg), Vector3.one);
                Fill(new Rect(0, 0, delta.magnitude + 1, 1), color);
                GUI.matrix = before;
                prev = next;
            }
        }
    }
}
