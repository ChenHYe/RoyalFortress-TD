using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class WaveGameController : MonoBehaviour
{
    [Serializable]
    public sealed class SpawnGroup
    {
        public string label;
        public WaypointPath route;
        public WaypointEnemy prefab;
        [Min(0)] public int count = 1;
        [Min(0.05f)] public float spawnInterval = 1;
        [Min(0)] public float startDelay;
        public bool requiresBranch;
    }

    [Serializable]
    public sealed class WaveDefinition
    {
        public string displayName;
        [Tooltip("When non-empty, these groups configure route, enemy prefab, count and timing for this wave.")]
        public SpawnGroup[] groups;
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
    public string homeDisplayName = "王宫";
    public string branchDisplayName = "墓穴";
    public string preparationHint = "准备防守 · 第2波起出现重甲兽人";
    public event Action StateChanged;
    private readonly HashSet<WaypointEnemy> activeEnemies = new HashSet<WaypointEnemy>();
    public int Lives { get; private set; }
    public int CurrentWave { get; private set; }
    public int CompletedWaves { get; private set; }
    public int LeakedEnemies { get; private set; }
    public int DefeatedEnemies { get; private set; }
    public int SpawnedSkeletons { get; private set; }
    public int SpawnedOrcs { get; private set; }
    public int SpawnedSpecials { get; private set; }
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
        if (!CanStartWave) return false;
        WaveDefinition plan = waves[CurrentWave];
        bool configured = plan.groups != null && plan.groups.Length > 0;
        if (configured)
        {
            int total = 0;
            foreach (var group in plan.groups)
            {
                if (group == null || group.count < 0) return false;
                total += group.count;
                if (group.count > 0 && (group.prefab == null || !ValidRoute(group.route, group.count)
                    || (group.requiresBranch && CurrentWave + 1 < branchUnlockWave))) return false;
            }
            if (total == 0) return false;
        }
        else if (enemyPrefab == null || (plan.orcsA + plan.orcsB + plan.orcsC > 0 && orcPrefab == null)
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
        if (plan.groups != null && plan.groups.Length > 0)
            yield return SpawnConfigured(plan.groups);
        else
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
        }
        IsSpawning = false;
        FinishWaveIfEmpty();
        StateChanged?.Invoke();
    }

    private IEnumerator SpawnConfigured(SpawnGroup[] groups)
    {
        var spawned = new int[groups.Length];
        int remaining = 0;
        foreach (var group in groups) remaining += group.count;
        float started = Time.time;
        while (remaining > 0 && !IsGameOver)
        {
            float elapsed = Time.time - started;
            bool changed = false;
            for (int i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                float due = Mathf.Max(0, group.startDelay) + spawned[i] * Mathf.Max(.05f, group.spawnInterval);
                if (spawned[i] >= group.count || elapsed < due) continue;
                Spawn(group.route, group.prefab);
                if (group.route == routeA) SpawnedA++;
                else if (group.route == routeB) SpawnedB++;
                else if (group.route == routeC) SpawnedC++;
                spawned[i]++; remaining--; changed = true;
            }
            if (changed) StateChanged?.Invoke();
            if (remaining > 0) yield return null;
        }
    }

    private void Spawn(WaypointPath path, bool orc)
        => Spawn(path, orc ? orcPrefab : enemyPrefab);

    private void Spawn(WaypointPath path, WaypointEnemy prefab)
    {
        var enemy = Instantiate(prefab, enemiesRoot);
        activeEnemies.Add(enemy);
        var kind = enemy.definition != null ? enemy.definition.kind : FortressEnemyKind.Skeleton;
        if (kind == FortressEnemyKind.ArmoredOrc) SpawnedOrcs++;
        else if (kind == FortressEnemyKind.RiftGuard) SpawnedSpecials++;
        else SpawnedSkeletons++;
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
