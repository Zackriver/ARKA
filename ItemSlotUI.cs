using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual inventory slot UI
/// Place in: Assets/Scripts/UI/ItemSlotUI.cs
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject emptyIndicator;
    
    private ItemType itemType = ItemType.None;
    private int quantity = 0;
    private ItemData cachedItemData; // Cache the ItemData reference
    
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════
    
    public ItemType ItemType => itemType;
    public int Quantity => quantity;
    public bool IsEmpty => itemType == ItemType.None || quantity <= 0;
    public ItemData itemData => cachedItemData; // Public accessor for ItemData
    
    // ═══════════════════════════════════════════════════════════════
    // UPDATE SLOT
    // ═══════════════════════════════════════════════════════════════
    
    public void SetItem(ItemType type, int amount, ItemData data = null)
    {
        itemType = type;
        quantity = amount;
        cachedItemData = data;
        
        UpdateVisuals();
    }
    
    public void ClearSlot()
    {
        itemType = ItemType.None;
        quantity = 0;
        cachedItemData = null;
        
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        bool isEmpty = IsEmpty;
        
        // Update icon
        if (iconImage != null)
        {
            iconImage.enabled = !isEmpty && cachedItemData != null && cachedItemData.icon != null;
            
            if (!isEmpty && cachedItemData != null && cachedItemData.icon != null)
            {
                iconImage.sprite = cachedItemData.icon;
            }
        }
        
        // Update quantity text
        if (quantityText != null)
        {
            quantityText.enabled = !isEmpty;
            
            if (!isEmpty)
            {
                quantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
        }
        
        // Update empty indicator
        if (emptyIndicator != null)
        {
            emptyIndicator.SetActive(isEmpty);
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════
    
    public bool CanAddItem(ItemType type)
    {
        if (IsEmpty) return true;
        
        if (itemType == type && quantity < Constants.Inventory.MAX_STACK_SIZE)
        {
            return true;
        }
        
        return false;
    }
    
    public bool CanRemoveItem(int amount = 1)
    {
        return quantity >= amount;
    }
    
    public bool RemoveItem(int amount = 1)
    {
        if (!CanRemoveItem(amount)) return false;
        
        quantity -= amount;
        
        if (quantity <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateVisuals();
        }
        
        return true;
    }
}