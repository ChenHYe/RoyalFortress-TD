using System.Linq;
using KingdomBorder.TwinTowers;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TempestSorcererDesigner
{
    private const string Art = "Assets/Art/Enemies/TempestSorcerer/tempest-sorcerer-sheet-v1.png";
    public const string Prefab = "Assets/Prefabs/TempestSorcerer.prefab";
    [MenuItem("Royal Fortress/接入灾厄巫师 Boss")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = width / 4f / 2.6f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        string[] directions = { "south", "east", "north", "west" };
        float[] rows = { 0, .248f, .49f, .734f, 1 };
        Vector2[] pivots = { new Vector2(.55f,.054f), new Vector2(.68f,.065f), new Vector2(.53f,.042f), new Vector2(.45f,.13f) };
        var slices = new SpriteMetaData[16];
        for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++)
        {
            int top = Mathf.RoundToInt(rows[r] * height), bottom = Mathf.RoundToInt(rows[r+1] * height);
            slices[r*4+c] = new SpriteMetaData {
                name = "Tempest_" + directions[r] + "_" + c,
                rect = new Rect(c * width / 4, height - bottom, width / 4, bottom - top),
                alignment = (int)SpriteAlignment.Custom, pivot = pivots[r]
            };
        }
#pragma warning disable 618
        importer.spritesheet = slices;
#pragma warning restore 618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Art).OfType<Sprite>().ToArray();
        const string animations = "Assets/Animations/TempestSorcerer";
        System.IO.Directory.CreateDirectory(animations);
        string controllerPath = animations + "/TempestSorcerer.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine = controller.layers[0].stateMachine;
        foreach (string direction in directions)
        {
            string name = "Tempest_Walk_" + direction;
            string path = animations + "/" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (!clip) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            clip.frameRate = 8;
            var keys = new ObjectReferenceKeyframe[5];
            for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe {
                time = i * .15f, value = sprites.Single(s => s.name == "Tempest_" + direction + "_" + i % 4)
            };
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name);
            if (!state) state = machine.AddState(name);
            state.motion = clip;
            if (direction == "south") machine.defaultState = state;
            EditorUtility.SetDirty(clip);
        }
        const string definitionPath = "Assets/Data/Enemies/TempestSorcerer.asset";
        var definition = AssetDatabase.LoadAssetAtPath<FortressEnemyDefinition>(definitionPath);
        if (!definition) { definition = ScriptableObject.CreateInstance<FortressEnemyDefinition>(); AssetDatabase.CreateAsset(definition, definitionPath); }
        definition.displayName = "灾厄巫师"; definition.kind = FortressEnemyKind.TempestSorcerer;
        definition.health = 2400; definition.armorPercent = 30; definition.speed = .72f;
        definition.baseDamage = 8; definition.goldReward = 180; definition.walkAnimationPrefix = "Tempest_Walk_";
        EditorUtility.SetDirty(definition);
        var scene = EditorSceneManager.OpenScene(FortressMapDesigner.ForestScene);
        var game = Object.FindFirstObjectByType<WaveGameController>();
        var build = Object.FindFirstObjectByType<TowerBuildController>();
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/ArmoredOrcEnemy.prefab");
        try
        {
            root.name = "Tempest Sorcerer"; root.transform.localScale = Vector3.one;
            var renderer = root.GetComponent<SpriteRenderer>();
            renderer.sprite = sprites.Single(s => s.name == "Tempest_south_0");
            root.GetComponent<Animator>().runtimeAnimatorController = controller;
            var enemy = root.GetComponent<WaypointEnemy>();
            enemy.definition = definition; enemy.maxHealth = definition.health; enemy.moveSpeed = definition.speed; enemy.armorPercent = definition.armorPercent;
            var agent = root.GetComponent<EnemyAgent>();
            agent.maxHealth = definition.health; agent.armorPercent = definition.armorPercent; agent.baseSpeed = definition.speed; agent.body = renderer;
            var ability = root.AddComponent<WeatherBossAbility>();
            ability.auraMaterial = build.sites[0].selectionRing.sharedMaterial;
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        game.bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<WaypointEnemy>();
        game.waves[game.waves.Length - 1].bossesA = 1;
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        scene = EditorSceneManager.OpenScene(FortressMapDesigner.MenuScene);
        var menu = Object.FindFirstObjectByType<FortressMainMenu>();
        foreach (var map in menu.maps) if (map.sceneName == "EmeraldSwitchback")
            map.description = "蜿蜒长路穿过森林与瀑布。最终波灾厄巫师来袭，暴雪压制塔的攻速，沙尘暴加速全体怪物。";
        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Tempest Sorcerer ready: EmeraldSwitchback wave 5.");
    }
}
