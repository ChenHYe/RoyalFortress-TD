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
        [Min(0.05f)] public float spawnInterval = 0.9f;
    }

    [Min(1)] public int startingLives = 20;
    [Min(1)] public int branchUnlockWave = 3;
    public WaypointPath routeA;
    public WaypointPath routeB;
    public WaypointPath routeC;
    public WaypointEnemy enemyPrefab;
    public WaypointEnemy orcPrefab;
    public Transform enemiesRoot;
    public WaveDefinition[] waves;
    public event Action StateChanged;
    private readonly HashSet<WaypointEnemy> activeEnemies = new HashSet<WaypointEnemy>();
    public int Lives { get; private set; }
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
    public bool CanStartWave => !IsGameOver && !IsWaveActive && CurrentWave < TotalWaves;

    private void Awake()
    {
        Lives = startingLives;
    }

    public void StartNextWave() { TryStartNextWave(); }

    public bool TryStartNextWave()
    {
        if (!CanStartWave || enemyPrefab == null) return false;
        WaveDefinition plan = waves[CurrentWave];
        if ((plan.orcsA + plan.orcsB + plan.orcsC > 0 && orcPrefab == null)
            || !ValidRoute(routeA, plan.countA + plan.orcsA) || !ValidRoute(routeB, plan.countB + plan.orcsB)
            || !ValidRoute(routeC, plan.countC + plan.orcsC) || (plan.countC + plan.orcsC > 0 && CurrentWave + 1 < branchUnlockWave))
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
        int rounds = Mathf.Max(plan.countA + plan.orcsA, Mathf.Max(plan.countB + plan.orcsB, plan.countC + plan.orcsC));
        for (int i = 0; i < rounds && !IsGameOver; i++)
        {
            if (i < plan.countA + plan.orcsA) { Spawn(routeA, i >= plan.countA); SpawnedA++; }
            if (i < plan.countB + plan.orcsB) { Spawn(routeB, i >= plan.countB); SpawnedB++; }
            if (i < plan.countC + plan.orcsC) { Spawn(routeC, i >= plan.countC); SpawnedC++; }
            StateChanged?.Invoke();
            if (i + 1 < rounds) yield return new WaitForSeconds(Mathf.Max(0.05f, plan.spawnInterval));
        }
        IsSpawning = false;
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    private void Spawn(WaypointPath path, bool orc)
    {
        var enemy = Instantiate(orc ? orcPrefab : enemyPrefab, enemiesRoot);
        activeEnemies.Add(enemy);
        if (orc) SpawnedOrcs++; else SpawnedSkeletons++;
        enemy.name = enemy.DisplayName + " - " + path.name;
        enemy.Initialize(this, path);
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
        DefeatedEnemies++;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
