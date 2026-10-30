using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    // Shared environment multipliers, separate from enemy slow sources and tower assets.
    public static class WorldCombatModifiers
    {
        public static float TowerAttackSpeed { get; private set; } = 1;
        public static float EnemyMovementSpeed { get; private set; } = 1;
        public static void Set(float towerSpeed, float enemySpeed)
        {
            TowerAttackSpeed = Mathf.Clamp(towerSpeed, 0.05f, 3f);
            EnemyMovementSpeed = Mathf.Clamp(enemySpeed, 0.1f, 3f);
        }
        public static void Restore() { Set(1, 1); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Restore(); }
    }
}
