using UnityEngine;

public enum FortressEnemyKind { Skeleton, ArmoredOrc, RiftGuard }

[CreateAssetMenu(menuName = "Royal Fortress/Enemy Definition")]
public sealed class FortressEnemyDefinition : ScriptableObject
{
    public string displayName;
    public FortressEnemyKind kind;
    public bool undead;
    [Min(1)] public float health = 125;
    [Range(0, 95)] public float armorPercent = 15;
    [Min(0.01f)] public float speed = 1.25f;
    [Min(1)] public int baseDamage = 1;
    public string walkAnimationPrefix = "Skeleton_Walk_";
}
