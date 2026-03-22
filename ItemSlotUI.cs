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
    public GameObject dragPrefab;
    
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
            if (iconImage != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.color = itemData.itemColor;
                iconImage.enabled = true;
            }
            
            if (quantityText != null)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            
            if (rarityBorder != null)
            {
                rarityBorder.color = itemData.GetRarityColor();
                rarityBorder.enabled = true;
            }
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (quantityText != null) quantityText.enabled = false;
            if (rarityBorder != null) rarityBorder.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || itemData == null || quantity <= 0) return;
        
        if (dragPrefab != null)
        {
            GameObject dragCopy = Instantiate(dragPrefab, canvas.transform);
            
            RectTransform dragRect = dragCopy.GetComponent<RectTransform>();
            if (dragRect != null)
            {
                dragRect.position = eventData.position;
            }
            
            DragDropHandler dragHandler = dragCopy.GetComponent<DragDropHandler>();
            if (dragHandler != null)
            {
                dragHandler.itemData = itemData;
                
                // ✅ FIXED: Added null check (my mistake)
                if (dragHandler.iconImage != null)
                {
                    dragHandler.iconImage.sprite = itemData.icon;
                    dragHandler.iconImage.color = itemData.itemColor;
                }
                
                dragHandler.OnBeginDrag(eventData);
            }
        }
    }
}