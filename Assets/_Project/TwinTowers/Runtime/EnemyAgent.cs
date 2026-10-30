using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    // 不依赖碰撞层、Tag、NavMesh 或主工程管理器；以后可用本组件适配实际敌人。
    public sealed class EnemyAgent : MonoBehaviour
    {
        public static readonly List<EnemyAgent> Active = new List<EnemyAgent>();
        [Min(1), Tooltip("最大生命值。")] public float maxHealth = 160;
        [Range(0, 95), Tooltip("减伤百分比，范围 0—95。60 表示未穿透时减少 60% 伤害。")] public float armorPercent = 50;
        [Min(0), Tooltip("基础移动速度，单位：世界单位/秒。设为 0 可作静止靶。")] public float baseSpeed = 1.55f;
        public SpriteRenderer body;
        public bool destroyOnDeath = true;
        public event Action<EnemyAgent> Died;
        public event Action<EnemyAgent> ReachedExit;
        public float Health { get; private set; }
        public bool IsAlive { get { return !finished && Health > 0 && isActiveAndEnabled; } }
        public float Progress { get; private set; }
        public float CurrentSpeed { get { return Mathf.Max(0, baseSpeed) * SpeedMultiplier * WorldCombatModifiers.EnemyMovementSpeed; } }
        public float SpeedMultiplier
        {
            get
            {
                RemoveExpired();
                float strongest = 0;
                for (int i = 0; i < slows.Count; i++) strongest = Mathf.Max(strongest, slows[i].fraction);
                return 1 - strongest;
            }
        }

        private struct Slow { public int source; public float fraction, until; }
        private readonly List<Slow> slows = new List<Slow>();
        private Vector3[] path;
        private int node;
        private float traveled, totalLength;
        private bool finished;
        private Color idleColor = Color.white;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() { Active.Clear(); }
        private void OnEnable()
        {
            Health = Mathf.Max(1, maxHealth);
            finished = false;
            slows.Clear();
            if (!Active.Contains(this)) Active.Add(this);
            if (body) idleColor = body.color;
        }
        private void OnDisable() { Active.Remove(this); }

        public void Initialize(float health, float armor, float speed, Vector3[] route)
        {
            maxHealth = Mathf.Max(1, health);
            Health = maxHealth;
            armorPercent = Mathf.Clamp(armor, 0, 95);
            baseSpeed = Mathf.Max(0, speed);
            path = route;
            node = 1;
            traveled = totalLength = Progress = 0;
            finished = false;
            slows.Clear();
            if (path != null && path.Length > 0)
            {
                transform.position = path[0];
                for (int i = 1; i < path.Length; i++) totalLength += Vector3.Distance(path[i-1], path[i]);
            }
        }

        public void SetExternalProgress(float value) { if (path == null) Progress = Mathf.Clamp01(value); }

        public static float CalculateDamage(float damage, float armor, float penetration)
        {
            float effectiveArmor = Mathf.Clamp(armor, 0, 95) * (1 - Mathf.Clamp01(penetration));
            return Mathf.Max(0, damage) * (1 - effectiveArmor / 100f);
        }
        public float TakeDamage(float damage, float penetration)
        {
            if (!IsAlive) return 0;
            float actual = CalculateDamage(damage, armorPercent, penetration);
            Health = Mathf.Max(0, Health - actual);
            if (Health <= 0)
            {
                finished = true;
                Active.Remove(this);
                Died?.Invoke(this);
                if (destroyOnDeath) Destroy(gameObject);
            }
            return actual;
        }
        public void ApplySlow(float fraction, float seconds, int sourceId)
        {
            if (!IsAlive || seconds <= 0 || fraction <= 0) return;
            RemoveExpired();
            Slow slow = new Slow { source = sourceId, fraction = Mathf.Clamp(fraction, 0, 0.9f), until = Time.time + seconds };
            for (int i = 0; i < slows.Count; i++)
                if (slows[i].source == sourceId) { slows[i] = slow; return; }
            slows.Add(slow);
        }
        public void SetAuraSlow(float fraction, int sourceId)
        {
            if (fraction <= 0) { RemoveSlow(sourceId); return; }
            ApplySlow(fraction, float.PositiveInfinity, sourceId);
        }
        public void RemoveSlow(int sourceId)
        {
            for (int i = slows.Count - 1; i >= 0; i--)
                if (slows[i].source == sourceId) slows.RemoveAt(i);
        }
        private void RemoveExpired()
        {
            for (int i = slows.Count - 1; i >= 0; i--)
                if (slows[i].until <= Time.time) slows.RemoveAt(i);
        }
        private void Update()
        {
            if (!IsAlive) return;
            float multiplier = SpeedMultiplier;
            if (body) body.color = multiplier < 1 ? Color.Lerp(idleColor, new Color(0.3f, 0.9f, 1), 0.7f) : idleColor;
            float remaining = CurrentSpeed * Time.deltaTime;
            while (path != null && node < path.Length && remaining > 0)
            {
                float distance = Vector3.Distance(transform.position, path[node]);
                if (distance > remaining)
                {
                    transform.position = Vector3.MoveTowards(transform.position, path[node], remaining);
                    traveled += remaining;
                    remaining = 0;
                }
                else
                {
                    transform.position = path[node++];
                    remaining -= distance;
                    traveled += distance;
                }
            }
            if (path != null) Progress = totalLength > 0 ? Mathf.Clamp01(traveled / totalLength) : 0;
            if (path != null && path.Length > 1 && node >= path.Length)
            {
                finished = true;
                Active.Remove(this);
                ReachedExit?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}

