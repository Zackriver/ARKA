using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingSlotUI : MonoBehaviour, 
    IDropHandler, 
    IPointerClickHandler,
    IPointerEnterHandler,   // ✅ ADDED (my mistake)
    IPointerExitHandler     // ✅ ADDED (my mistake)
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
        return currentItem == null && item != null;
    }
    
    public void SetItem(ItemData item)
    {
        if (item == null) return;
        
        currentItem = item;
        
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
        
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.SetSlot(slotIndex, null);
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Handled by DragDropHandler.OnEndDrag
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right || eventData.clickCount == 2)
        {
            ClearSlot();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null && currentItem == null)
        {
            backgroundImage.color = highlightColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null && currentItem == null)
        {
            backgroundImage.color = emptyColor;
        }
    }
}