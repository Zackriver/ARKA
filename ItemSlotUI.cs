using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public Image rarityBorder;
    public GameObject dragPrefab; // Prefab to spawn when dragging
    
    [Header("Data")]
    public ItemData itemData;
    private int quantity;
    
    [Header("Settings")]
    public bool isDraggable = true;
    
    private Canvas canvas;
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }
    
    public void SetItem(ItemData data, int qty)
    {
        itemData = data;
        quantity = qty;
        
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (itemData != null && quantity > 0)
        {
            // Show icon
            if (iconImage != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.color = itemData.itemColor;
                iconImage.enabled = true;
            }
            
            // Show quantity
            if (quantityText != null)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            
            // Show rarity border
            if (rarityBorder != null)
            {
                rarityBorder.color = itemData.GetRarityColor();
                rarityBorder.enabled = true;
            }
        }
        else
        {
            // Empty slot
            if (iconImage != null) iconImage.enabled = false;
            if (quantityText != null) quantityText.enabled = false;
            if (rarityBorder != null) rarityBorder.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || itemData == null || quantity <= 0) return;
        
        // Create drag copy
        if (dragPrefab != null)
        {
            GameObject dragCopy = Instantiate(dragPrefab, canvas.transform);
            
            // Position at mouse
            RectTransform dragRect = dragCopy.GetComponent<RectTransform>();
            dragRect.position = eventData.position;
            
            // Setup drag handler
            DragDropHandler dragHandler = dragCopy.GetComponent<DragDropHandler>();
            if (dragHandler != null)
            {
                dragHandler.itemData = itemData;
                dragHandler.iconImage.sprite = itemData.icon;
                dragHandler.iconImage.color = itemData.itemColor;
                
                // Start drag immediately
                dragHandler.OnBeginDrag(eventData);
            }
        }
    }
}