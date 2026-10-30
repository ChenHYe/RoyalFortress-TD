using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Camera))]
public sealed class MapCameraController : MonoBehaviour
{
    [Tooltip("地图边界来源。换关卡时指定该关的背景 SpriteRenderer。")]
    public SpriteRenderer mapRenderer;
    [Min(0.1f)] public float minimumSize = 3f;
    [Min(0.1f)] public float maximumSize = 8f;
    [Min(0.1f)] public float startSize = 8f;
    [Min(0.01f)] public float zoomSpeed = 1.4f;
    [Min(1f)] public float zoomResponse = 24f;
    [Min(0f)] public float keyboardSpeed = 8f;

    public float ZoomPercent => viewCamera ? MaximumViewSize / viewCamera.orthographicSize * 100f : 100f;
    public float MaximumViewSize
    {
        get
        {
            float limit = Mathf.Max(0.1f, maximumSize);
            if (!mapRenderer || !viewCamera) return limit;
            Bounds bounds = mapRenderer.bounds;
            // Cover the whole viewport: crop the surplus axis instead of exposing empty borders.
            return Mathf.Max(0.01f, Mathf.Min(limit, bounds.extents.y, bounds.extents.x / Mathf.Max(0.01f, viewCamera.aspect)));
        }
    }
    public float MinimumViewSize => Mathf.Min(Mathf.Max(0.1f, minimumSize), MaximumViewSize);
    private Camera viewCamera;
    private TowerBuildController build;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private Vector2 lastMousePosition, zoomAnchor;
    private bool wasDragging, pointerPanActive;
    private float targetSize;
    private bool wasFocused = true;

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();
        build = FindFirstObjectByType<TowerBuildController>();
        ResetView();
    }
    private void Update()
    {
        ClampToMap();
        if (build && build.game && build.game.IsPaused) { StopNavigation(); return; }
        if (build && build.IsDragging) { StopNavigation(); return; }
        if (!wasFocused) { AnimateZoom(); return; }
        Vector2 mousePosition = Vector2.zero, direction = Vector2.zero;
        float wheel = 0;
        bool dragging = false, pointerPressed = false, reset = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
            wheel = Mouse.current.scroll.ReadValue().y / 120f;
            dragging = Mouse.current.leftButton.isPressed;
            pointerPressed = Mouse.current.leftButton.wasPressedThisFrame;
        }
        if (Keyboard.current != null)
        {
            var keys = Keyboard.current;
            direction.x = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f)
                - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
            direction.y = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1f : 0f)
                - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1f : 0f);
            reset = keys.homeKey.wasPressedThisFrame;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        mousePosition = Input.mousePosition;
        wheel = Input.mouseScrollDelta.y;
        dragging = Input.GetMouseButton(0);
        pointerPressed = Input.GetMouseButtonDown(0);
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        reset = Input.GetKeyDown(KeyCode.Home);
#endif
        bool overMap = CanUsePointer(mousePosition);
        if (pointerPressed) pointerPanActive = overMap;
        if (!dragging) pointerPanActive = false;
        dragging &= pointerPanActive;
        if (!overMap) { wheel = 0; dragging = false; }
        if (reset) ResetView();
        if (Mathf.Abs(wheel) > 0.001f) ZoomAt(mousePosition, wheel);
        if (dragging && wasDragging) PanByPixels(mousePosition - lastMousePosition);
        if (direction.sqrMagnitude > 0)
            PanByWorld(direction.normalized * keyboardSpeed * Time.unscaledDeltaTime);
        AnimateZoom();
        lastMousePosition = mousePosition;
        wasDragging = dragging;
    }
    private void OnApplicationFocus(bool focus) { wasFocused = focus; StopNavigation(); }
    private void OnDisable() { StopNavigation(); }
    private void StopNavigation()
    {
        wasDragging = false;
        pointerPanActive = false;
        if (viewCamera) targetSize = viewCamera.orthographicSize;
    }
    public bool CanUsePointer(Vector2 screenPosition)
    {
        if (!viewCamera || !viewCamera.pixelRect.Contains(screenPosition)) return false;
        if (!EventSystem.current) return true;
        uiHits.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPosition }, uiHits);
        foreach (var hit in uiHits) if (hit.module is GraphicRaycaster) return false;
        return true;
    }
    public void ZoomAt(Vector2 screenPosition, float steps)
    {
        if (!viewCamera || (build && (build.IsDragging || (build.game && build.game.IsPaused)))) return;
        targetSize = Mathf.Clamp(targetSize * Mathf.Exp(-steps * zoomSpeed), MinimumViewSize, MaximumViewSize);
        zoomAnchor = screenPosition;
    }
    public void ZoomIn() { if (viewCamera) ZoomAt(viewCamera.pixelRect.center, 1f); }
    public void ZoomOut() { if (viewCamera) ZoomAt(viewCamera.pixelRect.center, -1f); }
    private void AnimateZoom()
    {
        if (Mathf.Abs(targetSize - viewCamera.orthographicSize) < 0.0001f) return;
        Vector3 before = ScreenPoint(zoomAnchor);
        float size = Mathf.Lerp(viewCamera.orthographicSize, targetSize, 1f - Mathf.Exp(-zoomResponse * Time.unscaledDeltaTime));
        if (Mathf.Abs(targetSize - size) < 0.001f) size = targetSize;
        viewCamera.orthographicSize = size;
        transform.position += before - ScreenPoint(zoomAnchor);
        ClampToMap();
    }
    private Vector3 ScreenPoint(Vector2 position)
    { return viewCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, -transform.position.z)); }
    public void PanByPixels(Vector2 delta)
    {
        if (!viewCamera || viewCamera.pixelHeight <= 0) return;
        PanByWorld(-delta * (viewCamera.orthographicSize * 2f / viewCamera.pixelHeight));
    }
    public void PanByWorld(Vector2 delta)
    {
        if (!viewCamera || (build && (build.IsDragging || (build.game && build.game.IsPaused)))) return;
        targetSize = viewCamera.orthographicSize;
        transform.position += new Vector3(delta.x, delta.y, 0);
        ClampToMap();
    }
    public void ResetView()
    {
        if (!viewCamera) viewCamera = GetComponent<Camera>();
        if (build && build.IsDragging) return;
        viewCamera.rect = new Rect(0, 0, 1, 1);
        targetSize = viewCamera.orthographicSize = Mathf.Clamp(startSize, MinimumViewSize, MaximumViewSize);
        Vector3 center = mapRenderer ? mapRenderer.bounds.center : Vector3.zero;
        transform.position = new Vector3(center.x, center.y, -10f);
        wasDragging = false;
        ClampToMap();
    }
    public void ClampToMap()
    {
        if (!viewCamera) return;
        viewCamera.orthographicSize = Mathf.Clamp(viewCamera.orthographicSize, MinimumViewSize, MaximumViewSize);
        targetSize = Mathf.Clamp(targetSize, MinimumViewSize, MaximumViewSize);
        if (!mapRenderer) return;
        Bounds bounds = mapRenderer.bounds;
        float halfHeight = viewCamera.orthographicSize;
        float halfWidth = halfHeight * viewCamera.aspect;
        Vector3 p = transform.position;
        p.x = bounds.size.x <= halfWidth * 2f ? bounds.center.x
            : Mathf.Clamp(p.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        p.y = bounds.size.y <= halfHeight * 2f ? bounds.center.y
            : Mathf.Clamp(p.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);
        transform.position = p;
    }
}
