using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Dormant during normal play. The exported player uses this only when explicitly
// launched with -fortress-smoke-test for a repeatable standalone verification.
public sealed class FortressPlayerSmokeTest : MonoBehaviour
{
    [Serializable]
    private sealed class Report
    {
        public string result;
        public string unityVersion;
        public string scene;
        public bool standalone;
        public int frameCount;
        public int buildSites;
        public int builtTowers;
        public bool menuOpened;
        public bool mageBuilt;
        public bool duplicateRejected;
        public bool waveStarted;
        public bool enemyMoved;
        public int livesBefore;
        public int livesAfter;
        public bool homeDamage;
        public string lifeLabel;
        public string screenshot;
        public string error;
        public string method = "Standalone Windows player; screen-point site selection and synthetic UI pointer clicks; natural Update-loop movement and Home damage.";
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-fortress-smoke-test") < 0) return;
        var node = new GameObject("Standalone Verification");
        node.AddComponent<FortressPlayerSmokeTest>();
    }

    private string output;
    private Report report;

    private IEnumerator Start()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        int location = Array.IndexOf(arguments, "-fortress-test-output");
        output = location >= 0 && location + 1 < arguments.Length ? arguments[location + 1]
            : Path.Combine(Application.persistentDataPath, "StandaloneVerification");
        Directory.CreateDirectory(output);
        report = new Report { result = "running", unityVersion = Application.unityVersion,
            scene = SceneManager.GetActiveScene().path, standalone = !Application.isEditor };
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        var build = FindFirstObjectByType<TowerBuildController>();
        var game = FindFirstObjectByType<WaveGameController>();
        var hud = FindFirstObjectByType<FortressHud>();
        if (build == null || game == null || hud == null || Camera.main == null)
        {
            Fail("Gameplay scene components are missing.");
            yield break;
        }
        Canvas.ForceUpdateCanvases();
        Physics2D.SyncTransforms();
        report.buildSites = build.sites.Length;
        report.menuOpened = build.HandleWorldClick(Camera.main.WorldToScreenPoint(build.sites[0].transform.position)) && build.MenuOpen;
        if (!report.menuOpened) { Fail("Foundation did not open the build menu."); yield break; }
        yield return new WaitForSecondsRealtime(0.1f);
        ExecuteEvents.Execute(build.mageButton.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        report.builtTowers = build.BuiltCount;
        report.mageBuilt = report.builtTowers == 1 && build.sites[0].BuiltTower != null && !build.MenuOpen;
        report.duplicateRejected = !build.sites[0].TryBuild(build.mageTowerPrefab, build.builtTowersRoot);
        if (!report.mageBuilt || !report.duplicateRejected) { Fail("Mage build or duplicate protection failed."); yield break; }

        ExecuteEvents.Execute(hud.waveButton.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        report.waveStarted = game.CurrentWave == 1 && game.AliveCount > 0;
        if (!report.waveStarted) { Fail("The wave button did not start the first wave."); yield break; }
        var enemy = FindFirstObjectByType<WaypointEnemy>();
        Vector3 start = enemy.transform.position;
        yield return new WaitForSecondsRealtime(0.4f);
        report.enemyMoved = enemy != null && Vector3.Distance(start, enemy.transform.position) > 0.01f;
        report.livesBefore = game.Lives;
        Time.timeScale = 5f;
        float deadline = Time.realtimeSinceStartup + 20f;
        while (game.Lives == report.livesBefore && Time.realtimeSinceStartup < deadline) yield return null;
        Time.timeScale = 1f;
        report.livesAfter = game.Lives;
        report.homeDamage = report.livesAfter < report.livesBefore;
        report.lifeLabel = hud.lifeText.text;
        report.frameCount = Time.frameCount;
        if (!report.enemyMoved || !report.homeDamage) { Fail("Enemy movement or natural Home damage failed."); yield break; }

        Camera.main.GetComponent<MapCameraController>().ResetView();
        report.screenshot = Path.Combine(output, "standalone-player.png");
        ScreenCapture.CaptureScreenshot(report.screenshot);
        float screenshotDeadline = Time.realtimeSinceStartup + 5f;
        while (!File.Exists(report.screenshot) && Time.realtimeSinceStartup < screenshotDeadline) yield return null;
        report.result = "passed";
        File.WriteAllText(Path.Combine(output, "standalone-verification.json"), JsonUtility.ToJson(report, true));
        Debug.Log("FORTRESS_STANDALONE_VERIFICATION_PASSED");
        yield return new WaitForSecondsRealtime(0.2f);
        Application.Quit(0);
    }

    private void Fail(string message)
    {
        report.result = "failed";
        report.error = message;
        File.WriteAllText(Path.Combine(output, "standalone-verification.json"), JsonUtility.ToJson(report, true));
        Debug.LogError(message);
        Application.Quit(2);
    }
}
