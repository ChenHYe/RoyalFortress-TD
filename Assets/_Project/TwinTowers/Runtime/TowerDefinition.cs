using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    public enum TowerKind { Mage, Frost, Arrow, Artillery }

    [CreateAssetMenu(menuName = "Kingdom Border/双塔/塔参数", fileName = "TowerSettings")]
    public sealed class TowerDefinition : ScriptableObject
    {
        [Tooltip("显示名称，仅影响演示文字。")] public string displayName = "法师塔";
        public TowerKind kind;
        [Min(0.1f), Tooltip("索敌半径，单位：Unity 世界单位。")] public float range = 4.2f;
        [Min(0f), Tooltip("每发基础伤害，命中时再结算护甲。")] public float damage = 28f;
        [Min(0.05f), Tooltip("两次发射最小间隔，单位：秒。")] public float attackInterval = 0.9f;
        [Min(0.1f), Tooltip("子弹移动速度，单位：世界单位/秒。")] public float projectileSpeed = 9f;
        [Range(0f, 1f), Tooltip("忽略的护甲比例。0.75 表示忽略 75% 护甲，不是额外增加 75% 伤害。")] public float armorPenetration = 0.75f;
        [Range(0f, 0.9f), Tooltip("减去的移动速度比例。0.45 表示减速 45%；最高 90%。")] public float slowFraction;
        [Min(0f), Tooltip("命中型临时减速的持续秒数。冰霜光环不使用此字段，圈内持续、出圈解除。")] public float slowDuration;
        [Min(0f), Tooltip("投石塔溅射半径。0 表示仅命中主目标；命中点周围该半径内全部敌人受到同等伤害。")] public float splashRadius;
        public Color magicColor = new Color(0.8f, 0.5f, 1f);

        private void OnValidate()
        {
            range = Mathf.Max(0.1f, range);
            damage = Mathf.Max(0, damage);
            attackInterval = Mathf.Max(0.05f, attackInterval);
            projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            armorPenetration = Mathf.Clamp01(armorPenetration);
            slowFraction = Mathf.Clamp(slowFraction, 0, 0.9f);
            slowDuration = Mathf.Max(0, slowDuration);
            splashRadius = Mathf.Max(0, splashRadius);
        }
    }
}
