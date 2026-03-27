using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;          
    public TextMeshProUGUI quantityText;
    public GameObject dragItemPrefab; 

    [Header("Slot Settings")]
    public bool isCraftingSlot = false; 
    public bool isResultSlot = false;
    public int slotID; // CRITICAL: Assign 0-14 in Inspector for your 15 Mother Slots

    [Header("Current Data")]
    [SerializeField] private ItemData _itemData;
    public ItemData itemData { get { return _itemData; } set { _itemData = value; } }
    [SerializeField] private int _quantity;
    public int quantity { get { return _quantity; } set { _quantity = value; } }

    private Canvas mainCanvas;

    private void Awake() {
        mainCanvas = GetComponentInParent<Canvas>();
        LoadSlotData(); 
        UpdateSlotDisplay();
    }

    public void SetItem(ItemData data, int qty) {
        _itemData = data;
        _quantity = qty;
        SaveSlotData(); 
        UpdateSlotDisplay();
    }

    public void ClearSlot() {
        _itemData = null;
        _quantity = 0;
        SaveSlotData();
        UpdateSlotDisplay();
    }

    public void UpdateSlotDisplay() {
        if (iconImage == null) return;
        bool hasItem = _itemData != null && _quantity > 0;
        
        iconImage.sprite = hasItem ? _itemData.icon : null;
        iconImage.enabled = hasItem;
        iconImage.color = Color.white; 

        if (quantityText != null) {
            quantityText.text = _quantity > 1 ? _quantity.ToString() : "";
            quantityText.enabled = _quantity > 1;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (_itemData == null || isResultSlot) return;
        SpawnDragIcon(eventData);
        // Only dim the original; don't decrease yet
        iconImage.color = new Color(1, 1, 1, 0.5f); 
    }

    public void OnDrag(PointerEventData eventData) { }

    private void SpawnDragIcon(PointerEventData eventData) {
        GameObject dragObj = Instantiate(dragItemPrefab, mainCanvas.transform);
        dragObj.transform.position = eventData.position;
        
        DragDropHandler handler = dragObj.GetComponent<DragDropHandler>();
        handler.itemData = _itemData;
        handler.sourceSlot = this; 
        handler.iconImage.sprite = _itemData.icon;
        eventData.pointerDrag = dragObj;
    }

    public void OnDrop(PointerEventData eventData) {
        if (isResultSlot) return;

        DragDropHandler handler = eventData.pointerDrag?.GetComponent<DragDropHandler>();
        if (handler == null) return;

        // Logic for Stacking vs Swapping
        if (_itemData == handler.itemData) {
            SetItem(_itemData, _quantity + 1);
            handler.sourceSlot.RemoveItem(1);
            handler.wasDroppedSuccessfully = true;
        }
        else if (_itemData == null) {
            SetItem(handler.itemData, 1);
            handler.sourceSlot.RemoveItem(1);
            handler.wasDroppedSuccessfully = true;
        }
        else {
            // Swap items between slots
            ItemData oldItem = _itemData;
            int oldQty = _quantity;
            SetItem(handler.itemData, 1);
            handler.sourceSlot.SetItem(oldItem, oldQty);
            handler.wasDroppedSuccessfully = true;
        }

        if (isCraftingSlot) Object.FindFirstObjectByType<CraftingManager>()?.CheckRecipe();
    }

    public void RemoveItem(int amount) {
        _quantity -= amount;
        if (_quantity <= 0) ClearSlot();
        else { SaveSlotData(); UpdateSlotDisplay(); }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_itemData != null) Object.FindFirstObjectByType<CraftingManager>()?.ShowDescription(_itemData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Object.FindFirstObjectByType<CraftingManager>()?.ShowDescription(null);
    }

    // SAVING LOGIC: Uses unique keys for each slotID
    private void SaveSlotData() {
        if (isCraftingSlot || isResultSlot) return;
        PlayerPrefs.SetString("InvSlotItem_" + slotID, _itemData != null ? _itemData.name : "");
        PlayerPrefs.SetInt("InvSlotQty_" + slotID, _quantity);
        PlayerPrefs.Save();
    }

    private void LoadSlotData() {
        if (isCraftingSlot || isResultSlot) return;
        string itemName = PlayerPrefs.GetString("InvSlotItem_" + slotID, "");
        if (!string.IsNullOrEmpty(itemName)) {
            // This requires your ItemData files to be in Assets/Resources/Items/
            _itemData = Resources.Load<ItemData>("Items/" + itemName);
            _quantity = PlayerPrefs.GetInt("InvSlotQty_" + slotID, 0);
        }
    }
}