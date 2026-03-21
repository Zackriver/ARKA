using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("References")]
    public Image iconImage;
    public Image backgroundImage;
    
    [Header("Data")]
    public ItemData currentItem;
    
    [Header("Visual")]
    public Color emptyColor = new Color(0.16f, 0.16f, 0.16f, 1f);
    public Color filledColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color highlightColor = new Color(0.4f, 0.6f, 1f, 0.3f);
    
    private int slotIndex;
    
    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }
    
    public bool CanAcceptItem(ItemData item)
    {
        // Can only accept if slot is empty
        return currentItem == null && item != null;
    }
    
    public void SetItem(ItemData item)
    {
        if (item == null) return;
        
        currentItem = item;
        
        // Update visual
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = item.itemColor;
            iconImage.enabled = true;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = filledColor;
        }
        
        // Notify crafting system
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.SetSlot(slotIndex, item.itemType);
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = emptyColor;
        }
        
        // Notify crafting system
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.SetSlot(slotIndex, null);
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // This is called when something is dropped on this slot
        // The actual logic is handled by DragDropHandler.OnEndDrag
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click or double-click to clear slot
        if (eventData.button == PointerEventData.InputButton.Right || eventData.clickCount == 2)
        {
            ClearSlot();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight when hovering
        if (backgroundImage != null && currentItem == null)
        {
            backgroundImage.color = highlightColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Remove highlight
        if (backgroundImage != null && currentItem == null)
        {
            backgroundImage.color = emptyColor;
        }
    }
}