using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FortressMapDesigner
{
    public const string OriginalScene = "Assets/Scenes/RoyalFortressGameplay.unity";
    public const string ForestScene = "Assets/Scenes/EmeraldSwitchback.unity";
    public const string MenuScene = "Assets/Scenes/MainMenu.unity";
    public const string ForestArt = "Assets/Art/Maps/emerald-switchback-v1.png";
    public const string TitleArt = "Assets/Art/Menu/royal-fortress-title-v1.png";
    private const float MapWidth = 32f;
    private static float imageWidth, imageHeight, mapHeight;

    [MenuItem("Royal Fortress/接入首页专用背景")]
    public static void ConfigureTitleBackground()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(TitleArt) as TextureImporter;
        if (!importer) throw new System.IO.FileNotFoundException(TitleArt);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
        var scene = EditorSceneManager.OpenScene(MenuScene);
        var menu = Object.FindFirstObjectByType<FortressMainMenu>();
        menu.titleBackground = AssetDatabase.LoadAssetAtPath<Sprite>(TitleArt);
        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Royal Fortress/创建或更新翡翠回廊与开始界面")]
    public static void CreateContent()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before creating scenes.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(ForestArt) as TextureImporter;
        if (!importer) throw new System.IO.FileNotFoundException(ForestArt);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        imageWidth = width; imageHeight = height; mapHeight = MapWidth * height / width;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = width / MapWidth;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
        var forest = AssetDatabase.LoadAssetAtPath<Sprite>(ForestArt);
        var original = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/royal-fortress-concept-v1.png");
        CreateForest(forest);
        CreateMenu(original, forest);
        var paths = new List<string> { MenuScene, OriginalScene, ForestScene };
        foreach (var scene in EditorBuildSettings.scenes)
            if (!paths.Contains(scene.path)) paths.Add(scene.path);
        EditorBuildSettings.scenes = paths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(MenuScene);
        Debug.Log("Created Emerald Switchback and MainMenu. Open MainMenu and press Play.");
    }
    private static Vector3 World(Vector2 pixel)
    { return new Vector3((pixel.x / imageWidth - 0.5f) * MapWidth, (0.5f - pixel.y / imageHeight) * mapHeight, 0); }

    private static void CreateForest(Sprite art)
    {
        // Save the copy before editing; the original gameplay scene stays on disk unchanged.
        var scene = EditorSceneManager.OpenScene(OriginalScene);
        EditorSceneManager.SaveScene(scene, ForestScene);
        var game = Object.FindFirstObjectByType<WaveGameController>();
        var build = Object.FindFirstObjectByType<TowerBuildController>();
        var map = GameObject.Find("Map Background").GetComponent<SpriteRenderer>();
        map.sprite = art;
        map.transform.position = Vector3.zero;
        map.transform.localScale = Vector3.one;
        var camera = Object.FindFirstObjectByType<MapCameraController>();
        camera.mapRenderer = map;
        camera.maximumSize = mapHeight / 2;
        camera.startSize = mapHeight / 2;
        camera.minimumSize = 3;
        camera.zoomSpeed = 1.4f;
        camera.zoomResponse = 24;
        camera.ResetView();
        var markers = GameObject.Find("Level Markers");
        if (markers) markers.SetActive(false);
        var oldRoutes = GameObject.Find("Routes");
        if (oldRoutes) Object.DestroyImmediate(oldRoutes);
        var routeRoot = new GameObject("Routes");
        var route = new GameObject("Emerald Switchback - Serpentine Road").AddComponent<WaypointPath>();
        route.transform.SetParent(routeRoot.transform);
        route.routeColor = new Color(0.5f, 1, 0.7f);
        // Centerline traced from the generated art, in original image pixels (1448 x 1086).
        // Catmull-Rom samples keep travel smooth through the wide hairpin bends.
        Vector2[] centers = {
            new Vector2(95,155), new Vector2(170,170), new Vector2(270,210), new Vector2(380,232),
            new Vector2(485,225), new Vector2(565,192), new Vector2(625,139), new Vector2(717,119),
            new Vector2(795,125), new Vector2(885,152), new Vector2(974,190), new Vector2(1070,194),
            new Vector2(1170,193), new Vector2(1260,213), new Vector2(1320,249), new Vector2(1358,289),
            new Vector2(1350,323), new Vector2(1305,353), new Vector2(1220,379), new Vector2(1110,382),
            new Vector2(1010,385), new Vector2(936,409), new Vector2(852,439), new Vector2(780,430),
            new Vector2(696,390), new Vector2(621,366), new Vector2(539,351), new Vector2(451,358),
            new Vector2(356,376), new Vector2(265,401), new Vector2(187,441), new Vector2(143,487),
            new Vector2(138,522), new Vector2(170,556), new Vector2(241,583), new Vector2(326,595),
            new Vector2(412,594), new Vector2(501,568), new Vector2(584,553), new Vector2(665,563),
            new Vector2(753,596), new Vector2(835,619), new Vector2(937,621), new Vector2(1051,627),
            new Vector2(1157,650), new Vector2(1258,696), new Vector2(1315,742), new Vector2(1342,782),
            new Vector2(1331,825), new Vector2(1290,855), new Vector2(1216,877), new Vector2(1126,878),
            new Vector2(1022,859), new Vector2(934,832), new Vector2(860,801), new Vector2(783,797),
            new Vector2(695,825), new Vector2(610,852), new Vector2(521,866), new Vector2(419,863),
            new Vector2(316,853), new Vector2(238,854), new Vector2(166,868)
        };
        var samples = Smooth(centers);
        route.points = new Transform[samples.Count];
        for (int i = 0; i < samples.Count; i++)
        {
            var node = new GameObject("Road " + i.ToString("D3")).transform;
            node.SetParent(route.transform);
            node.position = World(samples[i]);
            route.points[i] = node;
        }
        game.levelName = "翡翠回廊";
        game.routeA = route;
        game.routeB = game.routeC = null;
        game.startingGold = 380;
        game.startingLives = 20;
        game.branchUnlockWave = 3;
        game.waves = new[] { Wave(6,0,1.2f), Wave(8,1,1.1f), Wave(9,2,1.0f), Wave(10,3,0.95f), Wave(12,5,0.85f) };
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(TempestSorcererDesigner.Prefab);
        if (boss) { game.bossPrefab = boss.GetComponent<WaypointEnemy>(); game.waves[4].bossesA = 1; }

        Vector2[] pads = {
            new Vector2(335,290), new Vector2(584,269), new Vector2(953,255), new Vector2(1218,288),
            new Vector2(327,510), new Vector2(611,456), new Vector2(940,518), new Vector2(1176,535),
            new Vector2(360,752), new Vector2(600,726), new Vector2(952,730), new Vector2(1164,797)
        };
        BuildSite template = build.sites[0];
        var oldSites = template.transform.parent.gameObject;
        var newSites = new GameObject("Build Sites");
        var sites = new BuildSite[pads.Length];
        for (int i = 0; i < pads.Length; i++)
        {
            var site = Object.Instantiate(template, newSites.transform);
            site.name = "Build Site " + (i + 1).ToString("D2");
            site.siteLabel = "回廊塔位 " + (i + 1).ToString("D2");
            site.transform.position = World(pads[i]);
            site.transform.localScale = Vector3.one;
            if (site.buildAnchor) site.buildAnchor.localPosition = new Vector3(0, -0.12f, 0);
            var polygon = site.clickArea as PolygonCollider2D;
            if (polygon)
            {
                polygon.offset = Vector2.zero;
                polygon.points = Ellipse(0.86f, 0.49f, 32);
            }
            if (site.selectionRing)
            {
                site.selectionRing.transform.localPosition = new Vector3(0,0,-0.01f);
                site.selectionRing.transform.localScale = Vector3.one;
                site.selectionRing.useWorldSpace = false;
                site.selectionRing.loop = true;
                site.selectionRing.positionCount = 48;
                var shape = Ellipse(0.87f, 0.5f, 48);
                for (int j = 0; j < shape.Length; j++) site.selectionRing.SetPosition(j, shape[j]);
            }
            site.Select(false); site.Hover(false); site.ShowPlacementHint(false);
            sites[i] = site;
        }
        Object.DestroyImmediate(oldSites);
        build.sites = sites;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
    private static Vector2[] Ellipse(float x, float y, int count)
    {
        var points = new Vector2[count];
        for (int i = 0; i < count; i++)
        { float a = i * Mathf.PI * 2 / count; points[i] = new Vector2(Mathf.Cos(a) * x, Mathf.Sin(a) * y); }
        return points;
    }
    private static List<Vector2> Smooth(Vector2[] anchors)
    {
        var result = new List<Vector2>();
        for (int i = 0; i < anchors.Length - 1; i++)
        {
            Vector2 p0 = anchors[Mathf.Max(0, i - 1)], p1 = anchors[i], p2 = anchors[i + 1], p3 = anchors[Mathf.Min(anchors.Length - 1, i + 2)];
            for (int j = 0; j < 5; j++)
            {
                float t = j / 5f;
                result.Add(0.5f * ((2 * p1) + (-p0 + p2) * t + (2*p0 - 5*p1 + 4*p2 - p3)*t*t + (-p0 + 3*p1 - 3*p2 + p3)*t*t*t));
            }
        }
        result.Add(anchors[anchors.Length - 1]);
        return result;
    }
    private static WaveGameController.WaveDefinition Wave(int skeletons, int orcs, float interval)
    { return new WaveGameController.WaveDefinition { countA = skeletons, countB = 0, countC = 0, orcsA = orcs, spawnInterval = interval }; }
    private static void CreateMenu(Sprite original, Sprite forest)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true; camera.cullingMask = 0;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(8, 18, 24, 255);
        var menu = new GameObject("Fortress Main Menu").AddComponent<FortressMainMenu>();
        menu.titleBackground = AssetDatabase.LoadAssetAtPath<Sprite>(TitleArt);
        menu.maps = new[] {
            new FortressMainMenu.MapOption {
                title = "王国边境", subtitle = "城堡关隘 · 双线来袭", sceneName = "RoyalFortressGameplay",
                description = "保卫王宫前的最后一道关隘。两路敌人逐步汇合，第三波开启墓穴支路。",
                preview = original, startingGold = 300, towerSites = 7, waves = 3, routeLabel = "边境试炼",
                accent = new Color32(201,169,112,255)
            },
            new FortressMainMenu.MapOption {
                title = "翡翠回廊", subtitle = "森林峡谷 · 四段折返", sceneName = "EmeraldSwitchback",
                description = "蜿蜒长路穿过森林与瀑布。最终波灾厄巫师来袭，暴雪压制塔的攻速，沙尘暴加速全体怪物。",
                preview = forest, startingGold = 380, towerSites = 12, waves = 5, routeLabel = "曲径远征",
                accent = new Color32(121,206,158,255)
            }
        };
        EditorSceneManager.SaveScene(scene, MenuScene);
    }
}
