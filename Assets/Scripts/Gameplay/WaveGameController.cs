using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class WaveGameController : MonoBehaviour
{
    [Serializable]
    public sealed class WaveDefinition
    {
        [Min(0)] public int countA = 2;
        [Min(0)] public int countB = 2;
        [Min(0)] public int countC;
        [Min(0)] public int orcsA;
        [Min(0)] public int orcsB;
        [Min(0)] public int orcsC;
        [Min(0)] public int bossesA, bossesB, bossesC;
        [Min(0.05f)] public float spawnInterval = 0.9f;
    }

    public string levelName = "王国边境";
    [Min(1)] public int startingLives = 20;
    [Min(0)] public int startingGold = 300;
    [Min(1)] public int branchUnlockWave = 3;
    public WaypointPath routeA;
    public WaypointPath routeB;
    public WaypointPath routeC;
    public WaypointEnemy enemyPrefab;
    public WaypointEnemy orcPrefab;
    public WaypointEnemy bossPrefab;
    public int SpawnedBosses { get; private set; }
    public Transform enemiesRoot;
    public WaveDefinition[] waves;
    public event Action StateChanged;
    private readonly HashSet<WaypointEnemy> activeEnemies = new HashSet<WaypointEnemy>();
    public int Lives { get; private set; }
    public int Gold { get; private set; }
    public bool IsPaused { get; private set; }
    public bool CanBuild => !IsPaused && !IsGameOver && !AllWavesFinished;
    public int CurrentWave { get; private set; }
    public int CompletedWaves { get; private set; }
    public int LeakedEnemies { get; private set; }
    public int DefeatedEnemies { get; private set; }
    public int SpawnedSkeletons { get; private set; }
    public int SpawnedOrcs { get; private set; }
    public int SpawnedA { get; private set; }
    public int SpawnedB { get; private set; }
    public int SpawnedC { get; private set; }
    public int AliveCount => activeEnemies.Count;
    public int TotalWaves => waves == null ? 0 : waves.Length;
    public bool IsSpawning { get; private set; }
    public bool IsWaveActive { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool AllWavesFinished => TotalWaves > 0 && CompletedWaves == TotalWaves;
    public bool BranchOpen => CurrentWave >= branchUnlockWave;
    public bool UsesBranch
    {
        get
        {
            if (!routeC || waves == null) return false;
            foreach (var wave in waves) if (wave.countC + wave.orcsC + wave.bossesC > 0) return true;
            return false;
        }
    }
    public bool CanStartWave => !IsPaused && !IsGameOver && !IsWaveActive && CurrentWave < TotalWaves;
    private float resumeTimeScale = 1;

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        if (paused) { resumeTimeScale = Time.timeScale; Time.timeScale = 0; }
        else Time.timeScale = resumeTimeScale;
        StateChanged?.Invoke();
    }

    private void OnDestroy() { if (IsPaused) Time.timeScale = resumeTimeScale; }

    private void Awake()
    {
        if (!GetComponent<FortressWeatherController>()) gameObject.AddComponent<FortressWeatherController>();
        Lives = startingLives;
        Gold = Mathf.Max(0, startingGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (!CanBuild || amount < 0 || Gold < amount) return false;
        Gold -= amount;
        StateChanged?.Invoke();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        StateChanged?.Invoke();
    }

    public void StartNextWave() { TryStartNextWave(); }

    public bool TryStartNextWave()
    {
        if (!CanStartWave || enemyPrefab == null) return false;
        WaveDefinition plan = waves[CurrentWave];
        if ((plan.orcsA + plan.orcsB + plan.orcsC > 0 && orcPrefab == null)
            || (plan.bossesA + plan.bossesB + plan.bossesC > 0 && !bossPrefab)
            || !ValidRoute(routeA, plan.countA + plan.orcsA + plan.bossesA) || !ValidRoute(routeB, plan.countB + plan.orcsB + plan.bossesB)
            || !ValidRoute(routeC, plan.countC + plan.orcsC + plan.bossesC) || (plan.countC + plan.orcsC + plan.bossesC > 0 && CurrentWave + 1 < branchUnlockWave))
        {
            Debug.LogError("Wave route is missing, or the branch is scheduled before its unlock wave.", this);
            return false;
        }
        CurrentWave++;
        IsWaveActive = true;
        IsSpawning = true;
        StateChanged?.Invoke();
        StartCoroutine(SpawnWave(plan));
        return true;
    }

    private static bool ValidRoute(WaypointPath path, int count)
        => count == 0 || (path != null && path.GetPositions().Length >= 2);

    private IEnumerator SpawnWave(WaveDefinition plan)
    {
        int rounds = Mathf.Max(plan.countA + plan.orcsA + plan.bossesA, Mathf.Max(plan.countB + plan.orcsB + plan.bossesB, plan.countC + plan.orcsC + plan.bossesC));
        for (int i = 0; i < rounds && !IsGameOver; i++)
        {
            if (i < plan.countA + plan.orcsA + plan.bossesA) { Spawn(routeA, i < plan.countA ? 0 : i < plan.countA + plan.orcsA ? 1 : 2); SpawnedA++; }
            if (i < plan.countB + plan.orcsB + plan.bossesB) { Spawn(routeB, i < plan.countB ? 0 : i < plan.countB + plan.orcsB ? 1 : 2); SpawnedB++; }
            if (i < plan.countC + plan.orcsC + plan.bossesC) { Spawn(routeC, i < plan.countC ? 0 : i < plan.countC + plan.orcsC ? 1 : 2); SpawnedC++; }
            StateChanged?.Invoke();
            if (i + 1 < rounds) yield return new WaitForSeconds(Mathf.Max(0.05f, plan.spawnInterval));
        }
        IsSpawning = false;
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    private void Spawn(WaypointPath path, int kind)
    {
        var enemy = Instantiate(kind == 2 ? bossPrefab : kind == 1 ? orcPrefab : enemyPrefab, enemiesRoot);
        activeEnemies.Add(enemy);
        if (kind == 2) SpawnedBosses++; else if (kind == 1) SpawnedOrcs++; else SpawnedSkeletons++;
        enemy.name = enemy.DisplayName + " - " + path.name;
        enemy.Initialize(this, path);
    }

    [ContextMenu("开发预览/立即召唤灾厄巫师")]
    private void PreviewWeatherBoss()
    {
        if (!Application.isPlaying || IsGameOver || AllWavesFinished || !bossPrefab || !ValidRoute(routeA, 1)) return;
        Spawn(routeA, 2);
        SpawnedA++;
        StateChanged?.Invoke();
    }

    public void EnemyReachedHome(WaypointEnemy enemy)
    {
        if (IsGameOver || !activeEnemies.Remove(enemy)) return;
        LeakedEnemies++;
        Lives = Mathf.Max(0, Lives - enemy.BaseDamage);
        if (Lives == 0)
        {
            IsGameOver = true;
            IsWaveActive = false;
            IsSpawning = false;
            StopAllCoroutines();
        }
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    public void UnregisterEnemy(WaypointEnemy enemy)
    {
        if (!activeEnemies.Remove(enemy)) return;
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    public void EnemyDefeated(WaypointEnemy enemy)
    {
        if (!activeEnemies.Remove(enemy)) return;
        if (IsGameOver) return;
        DefeatedEnemies++;
        Gold += enemy.GoldReward;
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    private void FinishWaveIfEmpty()
    {
        if (!IsGameOver && IsWaveActive && !IsSpawning && activeEnemies.Count == 0)
        {
            IsWaveActive = false;
            CompletedWaves++;
        }
    }

    public void RestartLevel()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
