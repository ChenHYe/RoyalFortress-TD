using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class TowerBuildController : MonoBehaviour
{
    public Camera worldCamera;
    public WaveGameController game;
    public BuildSite[] sites;
    public GameObject mageTowerPrefab;
    public GameObject frostTowerPrefab;
    public GameObject arrowTowerPrefab;
    public GameObject artilleryTowerPrefab;
    public Transform builtTowersRoot;
    public Canvas hudCanvas;
    public RectTransform menu;
    public Text menuTitle;
    public Text menuHint;
    public Button mageButton;
    public Button arrowButton;
    public Button artilleryButton;
    public Button frostButton;
    public Button closeButton;
    private BuildSite selectedSite;
    private BuildSite hoveredSite;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    public BuildSite SelectedSite => selectedSite;
    public bool MenuOpen => menu != null && menu.gameObject.activeSelf;
    public int BuiltCount
    {
        get
        {
            int count = 0;
            if (sites != null) foreach (var site in sites) if (site != null && site.IsOccupied) count++;
            return count;
        }
    }

    private void Awake()
    {
        mageButton.onClick.AddListener(BuildMage);
        arrowButton.onClick.AddListener(BuildArrow);
        artilleryButton.onClick.AddListener(BuildArtillery);
        frostButton.onClick.AddListener(BuildFrost);
        closeButton.onClick.AddListener(Cancel);
        RefreshButtonAvailability();
        menu.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (game != null && game.IsGameOver) { Cancel(); UpdateHover(null); return; }
        Vector2 position = Vector2.zero;
        bool hasPointer = false, click = false, cancel = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            position = Mouse.current.position.ReadValue();
            click = Mouse.current.leftButton.wasPressedThisFrame;
            hasPointer = true;
        }
        cancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        position = Input.mousePosition;
        click = Input.GetMouseButtonDown(0);
        cancel = Input.GetKeyDown(KeyCode.Escape);
        hasPointer = true;
#endif
        if (cancel) Cancel();
        if (hasPointer)
        {
            UpdateHover(IsOverUI(position) ? null : SiteAt(position));
            if (click) HandleWorldClick(position);
        }
    }

    private void LateUpdate() { PositionMenu(); }

    private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;
        uiHits.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPosition }, uiHits);
        foreach (RaycastResult hit in uiHits)
            if (hit.module is GraphicRaycaster) return true;
        return false;
    }

    private BuildSite SiteAt(Vector2 screenPosition)
    {
        if (worldCamera == null || sites == null) return null;
        Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z));
        foreach (var site in sites)
            if (site != null && !site.IsOccupied && site.clickArea != null && site.clickArea.enabled
                && site.clickArea.OverlapPoint(world)) return site;
        return null;
    }

    private void UpdateHover(BuildSite next)
    {
        if (hoveredSite == next) return;
        if (hoveredSite != null) hoveredSite.Hover(false);
        hoveredSite = next;
        if (hoveredSite != null) hoveredSite.Hover(true);
    }

    public bool HandleWorldClick(Vector2 screenPosition)
    {
        if (game != null && game.IsGameOver) return false;
        if (IsOverUI(screenPosition)) return false;
        BuildSite hit = SiteAt(screenPosition);
        if (hit == null) { Cancel(); return false; }
        return SelectSite(hit);
    }

    public bool SelectSite(BuildSite site)
    {
        if (site == null || site.IsOccupied || (game != null && game.IsGameOver)) return false;
        Cancel();
        selectedSite = site;
        selectedSite.Select(true);
        menuTitle.text = "建造防御塔 · " + site.siteLabel;
        menuHint.text = "箭塔：单体；投石：溅射；法师：穿甲；冰霜：减速";
        RefreshButtonAvailability();
        menu.gameObject.SetActive(true);
        PositionMenu();
        return true;
    }

    private void RefreshButtonAvailability()
    {
        mageButton.interactable = mageTowerPrefab != null;
        frostButton.interactable = frostTowerPrefab != null;
        arrowButton.interactable = arrowTowerPrefab != null;
        artilleryButton.interactable = artilleryTowerPrefab != null;
    }

    public void BuildFrost() { TryBuildFrost(); }
    public void BuildMage() { TryBuildMage(); }
    public void BuildArrow() { TryBuildArrow(); }
    public void BuildArtillery() { TryBuildArtillery(); }

    public bool TryBuildFrost()
    {
        if (selectedSite == null || (game != null && game.IsGameOver)) return false;
        if (!selectedSite.TryBuild(frostTowerPrefab, builtTowersRoot)) return false;
        Cancel();
        return true;
    }

    public bool TryBuildMage()
    {
        if (selectedSite == null || (game != null && game.IsGameOver)) return false;
        if (!selectedSite.TryBuild(mageTowerPrefab, builtTowersRoot)) return false;
        Cancel();
        return true;
    }

    public bool TryBuildArrow()
    {
        if (selectedSite == null || (game != null && game.IsGameOver)) return false;
        if (!selectedSite.TryBuild(arrowTowerPrefab, builtTowersRoot)) return false;
        Cancel();
        return true;
    }

    public bool TryBuildArtillery()
    {
        if (selectedSite == null || (game != null && game.IsGameOver)) return false;
        if (!selectedSite.TryBuild(artilleryTowerPrefab, builtTowersRoot)) return false;
        Cancel();
        return true;
    }

    public void Cancel()
    {
        if (selectedSite != null) selectedSite.Select(false);
        selectedSite = null;
        if (menu != null) menu.gameObject.SetActive(false);
    }

    public void PositionMenu()
    {
        if (!MenuOpen || selectedSite == null || worldCamera == null || hudCanvas == null) return;
        var canvasRect = hudCanvas.transform as RectTransform;
        Vector3 screen = worldCamera.WorldToScreenPoint(selectedSite.transform.position);
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out local);
        Vector2 half = menu.rect.size * 0.5f;
        Vector2 target = local + new Vector2(half.x + 28f, 0f);
        Rect bounds = canvasRect.rect;
        target.x = Mathf.Clamp(target.x, bounds.xMin + half.x + 12f, bounds.xMax - half.x - 12f);
        target.y = Mathf.Clamp(target.y, bounds.yMin + half.y + 12f, bounds.yMax - half.y - 12f);
        menu.anchoredPosition = target;
    }
}
