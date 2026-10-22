using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Camera))]
public sealed class MapCameraController : MonoBehaviour
{
    [Tooltip("The background sprite used to keep the camera within the map.")]
    public SpriteRenderer mapRenderer;
    public float minimumSize = 3f;
    public float maximumSize = 8f;
    public float startSize = 8f;
    public float zoomSpeed = 0.16f;
    public float keyboardSpeed = 8f;

    private Camera viewCamera;
    private Vector2 lastMousePosition;
    private bool wasDragging;

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();
        ResetView();
    }

    private void Update()
    {
        Vector2 mousePosition = Vector2.zero;
        Vector2 direction = Vector2.zero;
        float wheel = 0f;
        bool dragging = false;
        bool reset = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
            wheel = Mouse.current.scroll.ReadValue().y / 120f;
            dragging = Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
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
        dragging = Input.GetMouseButton(2) || Input.GetMouseButton(1);
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        reset = Input.GetKeyDown(KeyCode.Home);
#endif

        if (reset) ResetView();
        if (Mathf.Abs(wheel) > 0.001f)
        {
            Vector3 before = viewCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -transform.position.z));
            viewCamera.orthographicSize = Mathf.Clamp(viewCamera.orthographicSize * Mathf.Exp(-wheel * zoomSpeed), minimumSize, maximumSize);
            Vector3 after = viewCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -transform.position.z));
            transform.position += before - after;
        }
        if (dragging && wasDragging && viewCamera.pixelHeight > 0)
        {
            Vector2 pixels = mousePosition - lastMousePosition;
            float worldUnitsPerPixel = viewCamera.orthographicSize * 2f / viewCamera.pixelHeight;
            transform.position -= new Vector3(pixels.x, pixels.y, 0f) * worldUnitsPerPixel;
        }
        if (direction.sqrMagnitude > 0f)
            transform.position += (Vector3)direction.normalized * keyboardSpeed * Time.unscaledDeltaTime;

        lastMousePosition = mousePosition;
        wasDragging = dragging;
        ClampToMap();
    }

    public void ResetView()
    {
        if (viewCamera == null) viewCamera = GetComponent<Camera>();
        viewCamera.orthographicSize = Mathf.Clamp(startSize, minimumSize, maximumSize);
        Vector3 center = mapRenderer != null ? mapRenderer.bounds.center : Vector3.zero;
        transform.position = new Vector3(center.x, center.y, -10f);
        ClampToMap();
    }

    public void ClampToMap()
    {
        if (mapRenderer == null || viewCamera == null) return;
        Bounds bounds = mapRenderer.bounds;
        float halfHeight = viewCamera.orthographicSize;
        float halfWidth = halfHeight * viewCamera.aspect;
        Vector3 position = transform.position;
        position.x = bounds.size.x <= halfWidth * 2f ? bounds.center.x
            : Mathf.Clamp(position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        position.y = bounds.size.y <= halfHeight * 2f ? bounds.center.y
            : Mathf.Clamp(position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);
        transform.position = position;
    }
}
