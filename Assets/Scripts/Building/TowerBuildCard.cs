using KingdomBorder.TwinTowers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class TowerBuildCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TowerKind Kind { get; private set; }
    public bool Available { get; private set; }
    private TowerBuildController owner;
    private CanvasGroup group;
    private Image border;
    private Text price;
    private Color accent;
    private bool dragging, hovered;
    private int pointerId;

    public void Initialize(TowerBuildController controller, TowerKind kind, CanvasGroup canvasGroup,
        Image outline, Text priceLabel, Color color)
    {
        owner = controller; Kind = kind; group = canvasGroup; border = outline; price = priceLabel; accent = color;
        Refresh();
    }
    public void Refresh()
    {
        if (!owner) return;
        Available = owner.CanAfford(Kind);
        group.alpha = Available ? (dragging ? 0.65f : 1f) : 0.45f;
        Color outlineColor = accent;
        outlineColor.a = 0.8f;
        border.color = Available && (hovered || dragging) ? outlineColor : new Color32(66, 74, 80, 130);
        price.text = owner.Cost(Kind) + " 金币";
        price.color = Available ? new Color32(244, 210, 133, 255) : new Color32(213, 159, 149, 255);
    }
    public void OnBeginDrag(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left || !owner || owner.IsDragging) return;
        dragging = owner.BeginDrag(Kind, data.position);
        pointerId = data.pointerId;
        Refresh();
    }
    public void OnDrag(PointerEventData data)
    { if (dragging && pointerId == data.pointerId) owner.UpdateDrag(data.position); }
    public void OnEndDrag(PointerEventData data)
    {
        if (!dragging || pointerId != data.pointerId) return;
        if (owner.IsDragging) owner.EndDrag(data.position);
        dragging = false;
        Refresh();
    }
    public void OnPointerEnter(PointerEventData data) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData data) { hovered = false; Refresh(); }
    public void OnPointerClick(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left && !Available && owner && !owner.IsDragging)
        { owner.BeginDrag(Kind, data.position); }
    }
    private void Update()
    {
        if (dragging && !owner.IsDragging) { dragging = false; Refresh(); }
    }
    private void OnDisable()
    {
        if (dragging && owner) owner.Cancel();
        dragging = hovered = false;
    }
}
