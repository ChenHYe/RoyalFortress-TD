using UnityEngine;

public sealed class BuildSite : MonoBehaviour
{
    public string siteLabel;
    public Transform buildAnchor;
    public Collider2D clickArea;
    public GameObject emptyIndicators;
    public LineRenderer selectionRing;
    private GameObject builtTower;
    private bool building;
    private bool selected;
    private bool hovered;
    public bool IsOccupied => builtTower != null || building;
    public GameObject BuiltTower => builtTower;

    public void Select(bool value) { selected = value; RefreshIndicator(); }
    public void Hover(bool value) { hovered = value; RefreshIndicator(); }

    private void RefreshIndicator()
    {
        if (emptyIndicators != null) emptyIndicators.SetActive(!IsOccupied);
        if (selectionRing == null) return;
        selectionRing.enabled = !IsOccupied && (selected || hovered);
        Color color = selected ? new Color32(190, 153, 255, 255) : new Color32(245, 205, 116, 220);
        selectionRing.startColor = selectionRing.endColor = color;
    }

    public bool TryBuild(GameObject prefab, Transform towerRoot)
    {
        if (IsOccupied || prefab == null) return false;
        building = true;
        try
        {
            Vector3 position = buildAnchor != null ? buildAnchor.position : transform.position;
            builtTower = Instantiate(prefab, position, Quaternion.identity, towerRoot);
            builtTower.name = prefab.name + " - " + siteLabel;
            if (clickArea != null) clickArea.enabled = false;
            selected = hovered = false;
            RefreshIndicator();
            return true;
        }
        finally { building = false; }
    }
}

