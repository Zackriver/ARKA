using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image iconImage;
    public ItemData itemData;
    
    [Header("Drag Settings")]
    public bool isDraggable = true;
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalPosition;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || itemData == null) return;
        
        // Store original position
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalPosition = rectTransform.anchoredPosition;
        
        // Make semi-transparent and ignore raycasts
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // Move to canvas root (so it renders on top)
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || itemData == null) return;
        
        // Follow mouse/touch
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable || itemData == null) return;
        
        // Restore transparency
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Check if dropped on a valid target
        bool droppedOnTarget = false;
        
        if (eventData.pointerEnter != null)
        {
            CraftingSlotUI craftingSlot = eventData.pointerEnter.GetComponent<CraftingSlotUI>();
            
            if (craftingSlot != null && craftingSlot.CanAcceptItem(itemData))
            {
                craftingSlot.SetItem(itemData);
                droppedOnTarget = true;
            }
        }
        
        // If not dropped on valid target, return to original position
        if (!droppedOnTarget)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }
        else
        {
            // Item was placed in crafting slot, destroy this drag copy
            Destroy(gameObject);
        }
    }
}