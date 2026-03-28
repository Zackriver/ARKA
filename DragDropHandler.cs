using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles drag and drop for inventory items
/// Place in: Assets/Scripts/UI/DragDropHandler.cs
/// </summary>
public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;
    private ItemSlotUI sourceSlot;
    
    private ItemType draggedItemType = ItemType.None;
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DRAG HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        sourceSlot = GetComponentInParent<ItemSlotUI>();
        
        if (sourceSlot == null || sourceSlot.IsEmpty)
        {
            eventData.pointerDrag = null;
            return;
        }
        
        draggedItemType = sourceSlot.ItemType;
        
        // Store original position and parent
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Move to canvas root for proper rendering
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
        }
        
        // Make semi-transparent while dragging
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore appearance
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Check if dropped on valid target
        if (eventData.pointerEnter != null)
        {
            // Check for crafting slot
            CraftingSlotUI craftingSlot = eventData.pointerEnter.GetComponent<CraftingSlotUI>();
            if (craftingSlot != null)
            {
                // CraftingSlotUI handles the drop
            }
        }
        
        // Return to original position
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }
        
        rectTransform.anchoredPosition = originalPosition;
        
        // Reset
        draggedItemType = ItemType.None ;
        sourceSlot = null;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PUBLIC ACCESSORS
    // ═══════════════════════════════════════════════════════════════
    
    public ItemType GetItemType() => draggedItemType;
    public ItemSlotUI GetSourceSlot() => sourceSlot;
}