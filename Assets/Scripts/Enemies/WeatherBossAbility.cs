using UnityEngine;
using KingdomBorder.TwinTowers;

[RequireComponent(typeof(WaypointEnemy), typeof(EnemyAgent))]
public sealed class WeatherBossAbility : MonoBehaviour
{
    [Header("灾厄巫师 · 天气轮换")]
    [Min(1)] public float snowDuration = 12;
    [Min(1)] public float sandDuration = 12;
    [Min(0.2f)] public float calmDuration = 3;
    [Range(0.1f, 1)] public float snowAttackSpeed = 0.6f;
    [Range(1, 3)] public float sandMovementSpeed = 1.35f;
    public Material auraMaterial;
    public WaypointEnemy Enemy { get; private set; }
    public bool IsActive => Enemy && !Enemy.Resolved && Enemy.Combat && Enemy.Combat.IsAlive;
    public FortressWeatherKind RequestedWeather { get { ReadPhase(out var kind, out _, out _); return kind; } }
    public float SecondsRemaining { get { ReadPhase(out _, out var seconds, out _); return seconds; } }
    public FortressWeatherKind NextWeather { get { ReadPhase(out _, out _, out var next); return next; } }
    private float elapsed;
    private bool registered;
    private LineRenderer aura;

    private void Start()
    {
        Enemy = GetComponent<WaypointEnemy>();
        if (!IsActive) return;
        Enemy.Combat.Died += OnKilled;
        if (FortressWeatherController.Instance)
        {
            FortressWeatherController.Instance.Register(this);
            registered = true;
        }
        CreateAura();
    }
    private void Update()
    {
        if (!IsActive || (Enemy.Owner && Enemy.Owner.IsGameOver)) { Unregister(); return; }
        elapsed += Time.deltaTime;
        if (aura)
        {
            Color color = RequestedWeather == FortressWeatherKind.Snow ? new Color(0.5f, 0.85f, 1, 0.8f)
                : RequestedWeather == FortressWeatherKind.Sandstorm ? new Color(1, 0.7f, 0.28f, 0.8f)
                : new Color(0.75f, 0.4f, 1, 0.5f);
            aura.startColor = aura.endColor = color;
            aura.transform.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 3) * 0.08f);
            aura.transform.localRotation = Quaternion.Euler(0, 0, elapsed * 18);
        }
    }
    private void ReadPhase(out FortressWeatherKind kind, out float seconds, out FortressWeatherKind next)
    {
        float snow = Mathf.Max(1, snowDuration), sand = Mathf.Max(1, sandDuration), calm = Mathf.Max(0.2f, calmDuration);
        float t = Mathf.Repeat(elapsed, snow + sand + 2 * calm);
        if (t < snow) { kind = FortressWeatherKind.Snow; seconds = snow - t; next = FortressWeatherKind.Clear; }
        else if ((t -= snow) < calm) { kind = FortressWeatherKind.Clear; seconds = calm - t; next = FortressWeatherKind.Sandstorm; }
        else if ((t -= calm) < sand) { kind = FortressWeatherKind.Sandstorm; seconds = sand - t; next = FortressWeatherKind.Clear; }
        else { kind = FortressWeatherKind.Clear; seconds = calm - (t - sand); next = FortressWeatherKind.Snow; }
    }
    private void CreateAura()
    {
        if (!auraMaterial) return;
        var node = new GameObject("Weather Ritual");
        node.transform.SetParent(transform, false);
        aura = node.AddComponent<LineRenderer>();
        aura.sharedMaterial = auraMaterial;
        aura.useWorldSpace = false; aura.loop = true; aura.positionCount = 64;
        aura.widthMultiplier = 0.035f; aura.sortingOrder = 9;
        for (int i = 0; i < 64; i++)
        {
            float angle = i * Mathf.PI * 2 / 64;
            float radius = i % 8 == 0 ? 0.84f : 0.7f;
            aura.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.55f, 0));
        }
    }
    private void OnKilled(EnemyAgent ignored) { Unregister(); if (aura) aura.enabled = false; }
    private void Unregister()
    {
        if (!registered) return;
        registered = false;
        if (FortressWeatherController.Instance) FortressWeatherController.Instance.Unregister(this);
    }
    private void OnDisable()
    {
        Unregister();
        if (Enemy && Enemy.Combat) Enemy.Combat.Died -= OnKilled;
    }
}
