using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KingdomBorder.TwinTowers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Runs only when the player is explicitly launched with -fortress-smoke-test.
public sealed class FortressPlayerSmokeTest : MonoBehaviour
{
    [Serializable] private sealed class Report
    {
        public string result = "running";
        public string unityVersion, scene, error;
        public List<string> checks = new List<string>();
        public string method = "Built Windows player; synthetic UI drag events; real enemy damage, movement and wave lifecycle.";
    }
    private Report report;
    private string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-fortress-smoke-test") < 0) return;
        new GameObject("Economy Verification").AddComponent<FortressPlayerSmokeTest>();
    }
    private bool Check(bool condition, string message)
    {
        if (condition) { report.checks.Add(message); Debug.Log("PASS: " + message); return true; }
        report.result = "failed"; report.error = message; WriteReport();
        Debug.LogError("FAIL: " + message); Application.Quit(2); return false;
    }
    private void WriteReport() { File.WriteAllText(Path.Combine(output, "verification.json"), JsonUtility.ToJson(report, true)); }
    private PointerEventData Pointer(Vector2 position)
    {
        return new PointerEventData(EventSystem.current) { position = position, button = PointerEventData.InputButton.Left, pointerId = -1 };
    }
    private void Drag(TowerBuildCard card, Vector2 position)
    {
        ExecuteEvents.Execute(card.gameObject, Pointer(position), ExecuteEvents.beginDragHandler);
        ExecuteEvents.Execute(card.gameObject, Pointer(position), ExecuteEvents.dragHandler);
    }
    private void Drop(TowerBuildCard card, Vector2 position)
    { ExecuteEvents.Execute(card.gameObject, Pointer(position), ExecuteEvents.endDragHandler); }
    private bool CoversViewport(Camera camera, SpriteRenderer map)
    {
        Bounds bounds = map.bounds;
        Vector3 min = camera.ViewportToWorldPoint(new Vector3(0, 0, -camera.transform.position.z));
        Vector3 max = camera.ViewportToWorldPoint(new Vector3(1, 1, -camera.transform.position.z));
        return min.x >= bounds.min.x - 0.01f && min.y >= bounds.min.y - 0.01f
            && max.x <= bounds.max.x + 0.01f && max.y <= bounds.max.y + 0.01f;
    }
    private IEnumerator Capture(string name)
    {
        yield return new WaitForEndOfFrame();
        string path = Path.Combine(output, name + ".png");
        ScreenCapture.CaptureScreenshot(path);
        float deadline = Time.realtimeSinceStartup + 5;
        while (!File.Exists(path) && Time.realtimeSinceStartup < deadline) yield return null;
    }
    private IEnumerator Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-fortress-test-output");
        output = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.persistentDataPath, "EconomyVerification");
        Directory.CreateDirectory(output);
        report = new Report { unityVersion = Application.unityVersion, scene = SceneManager.GetActiveScene().path };
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        yield return null;
        yield return new WaitForSecondsRealtime(0.4f);
        var build = FindFirstObjectByType<TowerBuildController>();
        var game = FindFirstObjectByType<WaveGameController>();
        var hud = FindFirstObjectByType<FortressHud>();
        if (!Check(build && game && hud && hud.Cards != null && hud.Cards.Length == 4, "Four tower cards and gameplay components initialized")) yield break;
        Canvas.ForceUpdateCanvases();
        Physics2D.SyncTransforms();
        if (!Check(game.Gold == 300 && hud.GoldText.text == "300", "Starting gold and visible balance are 300")) yield break;
        foreach (var card in hud.Cards) if (!Check(card.Available, card.Kind + " initially affordable")) yield break;
        var camera = build.worldCamera;
        var navigation = camera.GetComponent<MapCameraController>();
        if (!Check(navigation && navigation.mapRenderer && camera.rect == new Rect(0, 0, 1, 1),
            "Map camera fills the whole window behind floating UI")) yield break;
        foreach (var resolution in new[] { new Vector2Int(1600, 900), new Vector2Int(1024, 768), new Vector2Int(1920, 810) })
        {
            Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.35f);
            Canvas.ForceUpdateCanvases();
            navigation.ResetView();
            if (!Check(Screen.width == resolution.x && Screen.height == resolution.y,
                "Requested window size applied: " + Screen.width + "x" + Screen.height)) yield break;
            if (!Check(CoversViewport(camera, navigation.mapRenderer),
                "Map covers viewport at " + resolution.x + "x" + resolution.y)) yield break;
            foreach (var site in build.sites)
            {
                navigation.PanByWorld((Vector2)(site.transform.position - camera.transform.position));
                Vector2 point = camera.WorldToScreenPoint(site.transform.position);
                if (!Check(navigation.CanUsePointer(point), site.siteLabel + " accessible by panning at " + resolution.x + "x" + resolution.y)) yield break;
            }
        }
        Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
        yield return new WaitForSecondsRealtime(0.35f);
        navigation.ResetView();
        Vector2 anchor = camera.pixelRect.center + new Vector2(Screen.width * 0.1f, Screen.height * 0.05f);
        Vector3 anchorBefore = camera.ScreenToWorldPoint(new Vector3(anchor.x, anchor.y, -camera.transform.position.z));
        float initialSize = camera.orthographicSize;
        navigation.ZoomAt(anchor, 2);
        yield return new WaitForSecondsRealtime(0.7f);
        Vector3 anchorAfter = camera.ScreenToWorldPoint(new Vector3(anchor.x, anchor.y, -camera.transform.position.z));
        if (!Check(camera.orthographicSize < initialSize && Vector3.Distance(anchorBefore, anchorAfter) < 0.02f,
            "Smooth zoom keeps the world point beneath the pointer")) yield break;
        navigation.PanByWorld(new Vector2(10000, 10000));
        if (!Check(CoversViewport(camera, navigation.mapRenderer), "Pan clamps at top and right edges")) yield break;
        navigation.PanByWorld(new Vector2(-10000, -10000));
        if (!Check(CoversViewport(camera, navigation.mapRenderer), "Pan clamps at bottom and left edges")) yield break;
        navigation.ZoomAt(camera.pixelRect.center, 100);
        yield return new WaitForSecondsRealtime(0.7f);
        if (!Check(Mathf.Abs(camera.orthographicSize - navigation.MinimumViewSize) < 0.01f, "Zoom-in respects its minimum size")) yield break;
        navigation.ZoomAt(camera.pixelRect.center, -100);
        yield return new WaitForSecondsRealtime(0.7f);
        if (!Check(Mathf.Abs(camera.orthographicSize - navigation.MaximumViewSize) < 0.01f && CoversViewport(camera, navigation.mapRenderer),
            "Zoom-out stops before exposing empty map borders")) yield break;
        navigation.ResetView();
        Vector2 cardCenter = RectTransformUtility.WorldToScreenPoint(null,
            hud.Cards[0].GetComponent<RectTransform>().TransformPoint(hud.Cards[0].GetComponent<RectTransform>().rect.center));
        if (!Check(!navigation.CanUsePointer(cardCenter), "Floating tower cards block map wheel and pan input")) yield break;
        yield return Capture("initial");
        Vector2 site0 = build.worldCamera.WorldToScreenPoint(build.sites[0].transform.position);
        Vector2 site1 = build.worldCamera.WorldToScreenPoint(build.sites[1].transform.position);
        Vector2 dock = RectTransformUtility.WorldToScreenPoint(null, hud.Cards[2].transform.position);
        Drag(hud.Cards[2], site0);
        if (!Check(build.IsDragging && build.HoveredSite == build.sites[0] && build.BuiltCount == 0
            && game.Gold == 300 && FindObjectsByType<TowerBehaviour>(FindObjectsSortMode.None).Length == 0,
            "Dragging previews a tower without spending or creating a combat tower")) yield break;
        Vector3 frozenPosition = camera.transform.position;
        float frozenSize = camera.orthographicSize;
        navigation.PanByWorld(Vector2.one);
        navigation.ZoomAt(camera.pixelRect.center, 1);
        yield return new WaitForSecondsRealtime(0.15f);
        if (!Check(camera.transform.position == frozenPosition && Mathf.Abs(camera.orthographicSize - frozenSize) < 0.001f,
            "Dragging a tower freezes map navigation")) yield break;
        yield return Capture("drag-preview");
        Drop(hud.Cards[2], dock);
        if (!Check(game.Gold == 300 && build.BuiltCount == 0 && !build.IsDragging, "Dropping on UI cancels without charging")) yield break;
        Drag(hud.Cards[2], site0);
        build.Cancel();
        Drop(hud.Cards[2], site0);
        if (!Check(game.Gold == 300 && build.BuiltCount == 0, "Cancel followed by release cannot place a tower")) yield break;
        Drag(hud.Cards[2], site0); Drop(hud.Cards[2], site0);
        if (!Check(game.Gold == 160 && build.BuiltCount == 1, "Mage card drag places one tower and spends exactly 140")) yield break;
        Drag(hud.Cards[2], site0); Drop(hud.Cards[2], site0);
        if (!Check(game.Gold == 160 && build.BuiltCount == 1, "Occupied site rejects placement without charging")) yield break;
        Drag(hud.Cards[3], site1); Drop(hud.Cards[3], site1);
        if (!Check(game.Gold == 0 && build.BuiltCount == 2, "Artillery spends the remaining 160")) yield break;
        foreach (var card in hud.Cards) if (!Check(!card.Available, card.Kind + " dims when unaffordable")) yield break;
        Drag(hud.Cards[0], site0); Drop(hud.Cards[0], site0);
        if (!Check(!build.IsDragging && game.Gold == 0 && build.BuiltCount == 2, "Insufficient balance cannot drag or purchase")) yield break;
        yield return Capture("insufficient-gold");
        ExecuteEvents.Execute(hud.waveButton.gameObject, Pointer(Vector2.zero), ExecuteEvents.pointerClickHandler);
        if (!Check(game.CurrentWave == 1 && game.AliveCount > 0, "Wave action starts the first wave")) yield break;
        var enemy = FindFirstObjectByType<WaypointEnemy>();
        Vector3 start = enemy.transform.position;
        yield return new WaitForSeconds(0.3f);
        if (!Check(enemy && Vector3.Distance(start, enemy.transform.position) > 0.01f, "Enemies move naturally after starting a wave")) yield break;
        int before = game.Gold, defeated = game.DefeatedEnemies, reward = enemy.GoldReward;
        enemy.Combat.TakeDamage(10000, 1);
        if (!Check(game.Gold == before + reward && reward == 18 && game.DefeatedEnemies == defeated + 1,
            "Skeleton combat death awards 18 gold and increments kills")) yield break;
        game.EnemyDefeated(enemy);
        if (!Check(game.Gold == before + reward && game.DefeatedEnemies == defeated + 1, "Duplicate death callback cannot pay twice")) yield break;
        Time.timeScale = 5;
        float deadline = Time.realtimeSinceStartup + 30;
        while (game.IsWaveActive && Time.realtimeSinceStartup < deadline) yield return null;
        if (!Check(!game.IsWaveActive && game.CompletedWaves == 1, "First wave resolves with the paid towers")) yield break;
        game.StartNextWave();
        WaypointEnemy orc = null;
        deadline = Time.realtimeSinceStartup + 15;
        while (!orc && Time.realtimeSinceStartup < deadline)
        {
            foreach (var candidate in FindObjectsByType<WaypointEnemy>(FindObjectsSortMode.None))
                if (!candidate.Resolved && candidate.definition && candidate.definition.kind == FortressEnemyKind.ArmoredOrc) { orc = candidate; break; }
            if (!orc) yield return null;
        }
        if (!Check(orc, "Second wave spawns an armored orc")) yield break;
        before = game.Gold;
        orc.Combat.TakeDamage(10000, 1);
        if (!Check(game.Gold == before + 40, "Armored orc combat death awards 40 gold")) yield break;
        WaypointEnemy leak = null;
        foreach (var candidate in FindObjectsByType<WaypointEnemy>(FindObjectsSortMode.None))
            if (!candidate.Resolved) { leak = candidate; break; }
        if (!Check(leak, "Live enemy available for escape verification")) yield break;
        before = game.Gold;
        int lives = game.Lives, damage = leak.BaseDamage;
        leak.ReachHome();
        if (!Check(game.Gold == before && game.Lives == Mathf.Max(0, lives - damage), "Escape deducts lives without awarding gold")) yield break;
        leak.ReachHome();
        if (!Check(game.Gold == before && game.Lives == Mathf.Max(0, lives - damage), "Duplicate escape cannot deduct lives twice")) yield break;
        Time.timeScale = 1;
        yield return Capture("combat");
        report.result = "restarting"; WriteReport();
        game.RestartLevel();
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);
        game = FindFirstObjectByType<WaveGameController>();
        build = FindFirstObjectByType<TowerBuildController>();
        if (!Check(game && build && game.Gold == game.startingGold && game.Lives == game.startingLives
            && build.BuiltCount == 0 && game.CurrentWave == 0, "Restart resets coins, lives, towers and waves")) yield break;
        yield return Capture("restart");
        report.result = "passed"; WriteReport();
        Debug.Log("FORTRESS_ECONOMY_VERIFICATION_PASSED");
        Application.Quit(0);
    }
}
