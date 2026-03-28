using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Individual crafting slot UI
/// Place in: Assets/Scripts/UI/CraftingSlotUI.cs
/// </summary>
public class CraftingSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    [SerializeField] private int slotIndex;
    
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject emptyIndicator;
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Start()
    {
        RefreshSlot();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UI UPDATE
    // ═══════════════════════════════════════════════════════════════
    
    public void RefreshSlot()
    {
        if (CraftingSystem.Instance == null) return;
        
        bool isFilled = CraftingSystem.Instance.IsSlotFilled(slotIndex);
        
        if (isFilled)
        {
            ItemType itemType = CraftingSystem.Instance.GetItemInSlot(slotIndex);
            
            // TODO: Get icon from ItemType (you'll need an ItemDatabase)
            if (iconImage != null)
            {
                iconImage.enabled = true;
                // iconImage.sprite = GetSpriteForItemType(itemType);
            }
            
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(false);
            }
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }
            
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(true);
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DROP HANDLER
    // ═══════════════════════════════════════════════════════════════
    
    public void OnDrop(PointerEventData eventData)
    {
        if (CraftingSystem.Instance == null) return;
        
        // Check if slot is already filled
        if (CraftingSystem.Instance.IsSlotFilled(slotIndex))
        {
            Debug.LogWarning($"[CraftingSlotUI] Slot {slotIndex} already filled!");
            return;
        }
        
        // Get dragged item
        GameObject draggedObject = eventData.pointerDrag;
        
        if (draggedObject != null)
        {
            DragDropHandler dragHandler = draggedObject.GetComponent<DragDropHandler>();
            
            if (dragHandler != null && dragHandler.GetItemType() != ItemType.None)
            {
                ItemType itemType = dragHandler.GetItemType();
                
                // Add to crafting system
                bool added = CraftingSystem.Instance.AddItemToSlot(slotIndex, itemType);
                
                if (added)
                {
                    // Remove from inventory
                    if (PlayerInventory.Instance != null)
                    {
                        PlayerInventory.Instance.RemoveItem(itemType, 1);
                    }
                    
                    RefreshSlot();
                }
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CLICK HANDLER
    // ═══════════════════════════════════════════════════════════════
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (CraftingSystem.Instance == null) return;
        
        // Right click to remove item
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (CraftingSystem.Instance.IsSlotFilled(slotIndex))
            {
                ItemType itemType = CraftingSystem.Instance.GetItemInSlot(slotIndex);
                
                // Remove from crafting
                bool removed = CraftingSystem.Instance.RemoveItemFromSlot(slotIndex);
                
                if (removed)
                {
                    // Return to inventory
                    if (PlayerInventory.Instance != null)
                    {
                        PlayerInventory.Instance.AddItem(itemType, 1);
                    }
                    
                    RefreshSlot();
                }
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════
    
    public int GetSlotIndex() => slotIndex;
}