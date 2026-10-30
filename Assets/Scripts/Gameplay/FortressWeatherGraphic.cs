using UnityEngine;
using UnityEngine.UI;

// Geometry-only weather: no particle texture or extra render pipeline dependency.
public sealed class FortressWeatherGraphic : MaskableGraphic
{
    public FortressWeatherController controller;
    private float snow, sand;
    protected override void Awake() { base.Awake(); raycastTarget = false; }
    private void Update()
    {
        float before = snow + sand;
        snow = Mathf.MoveTowards(snow, controller && controller.Weather == FortressWeatherKind.Snow ? 1 : 0, Time.deltaTime * 1.6f);
        sand = Mathf.MoveTowards(sand, controller && controller.Weather == FortressWeatherKind.Sandstorm ? 1 : 0, Time.deltaTime * 1.6f);
        if (before > 0 || snow + sand > 0) SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        if (snow > 0) AddWeather(vh, rect, snow, false);
        if (sand > 0) AddWeather(vh, rect, sand, true);
    }
    private void AddWeather(VertexHelper vh, Rect rect, float intensity, bool dust)
    {
        Quad(vh, rect.center, rect.size, dust ? new Color(0.72f,0.43f,0.16f,0.15f*intensity) : new Color(0.45f,0.7f,0.9f,0.085f*intensity), 0);
        int count = dust ? 110 : 160;
        for (int i = 0; i < count; i++)
        {
            float seed = Mathf.Repeat(i * 0.6180339887f, 1);
            float lane = Mathf.Repeat(i * 0.41421356f, 1);
            float x = dust ? Mathf.Repeat(seed + Time.time * (0.20f + lane * 0.2f), 1)
                : Mathf.Repeat(seed + Mathf.Sin(Time.time * 0.6f + i) * 0.018f - Time.time * 0.015f, 1);
            float y = dust ? Mathf.Repeat(lane + Mathf.Sin(Time.time * 1.2f + i) * 0.015f, 1)
                : Mathf.Repeat(lane - Time.time * (0.045f + seed * 0.055f), 1);
            Vector2 position = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, x), Mathf.Lerp(rect.yMin, rect.yMax, y));
            Vector2 size = dust ? new Vector2(14 + seed * 32, 1 + lane) : Vector2.one * (2 + seed * 3);
            Color color = dust ? new Color(0.98f, 0.78f, 0.42f, (0.18f + seed * 0.28f) * intensity)
                : new Color(0.9f, 0.97f, 1, (0.4f + seed * 0.45f) * intensity);
            Quad(vh, position, size, color, dust ? -0.14f : Mathf.PI * 0.25f);
        }
    }
    private static void Quad(VertexHelper vh, Vector2 center, Vector2 size, Color color, float angle)
    {
        int start = vh.currentVertCount;
        Vector2 x = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size.x * 0.5f;
        Vector2 y = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * size.y * 0.5f;
        var vertex = UIVertex.simpleVert; vertex.color = color;
        vertex.position = center - x - y; vh.AddVert(vertex);
        vertex.position = center - x + y; vh.AddVert(vertex);
        vertex.position = center + x + y; vh.AddVert(vertex);
        vertex.position = center + x - y; vh.AddVert(vertex);
        vh.AddTriangle(start, start+1, start+2); vh.AddTriangle(start+2, start+3, start);
    }
}
