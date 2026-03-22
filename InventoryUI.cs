using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [Header("Item Grid")]
    public GameObject itemSlotPrefab;
    public Transform itemsGridContainer;
    public GameObject dragItemPrefab;
    public List<ItemData> allItemsDatabase; // ✅ ADDED: Assign all 15 items here
    
    [Header("Crafting")]
    public CraftingSlotUI[] craftingSlots = new CraftingSlotUI[4];
    public Image resultIconImage;
    public TextMeshProUGUI resultNameText;
    public Button craftButton;
    
    [Header("Power-Ups List")]
    public GameObject powerUpEntryPrefab;
    public Transform powerUpsListContainer;
    
    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 2f;
    
    private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
    private List<PowerUpEntryUI> powerUpEntries = new List<PowerUpEntryUI>();
    
    private void Start()
    {
        InitializeCraftingSlots();
        PopulateItemGrid();
        PopulatePowerUpsList();
        
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.OnSlotChanged += OnCraftingSlotChanged;
            CraftingSystem.Instance.OnCraftSuccess += OnCraftSuccess;
            CraftingSystem.Instance.OnCraftFailed += OnCraftFailed;
        }
        
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += RefreshDisplay;
        }
        
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
        
        UpdateResultDisplay();
    }
    
    private void OnDestroy()
    {
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.OnSlotChanged -= OnCraftingSlotChanged;
            CraftingSystem.Instance.OnCraftSuccess -= OnCraftSuccess;
            CraftingSystem.Instance.OnCraftFailed -= OnCraftFailed;
        }
        
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= RefreshDisplay;
        }
    }
    
    private void InitializeCraftingSlots()
    {
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            if (craftingSlots[i] != null)
            {
                craftingSlots[i].Initialize(i);
            }
        }
    }
    
    private void PopulateItemGrid()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        itemSlots.Clear();
        
        if (PlayerInventory.Instance == null || itemSlotPrefab == null || itemsGridContainer == null)
            return;
        
        foreach (System.Enum itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemType type = (ItemType)itemType;
            int quantity = PlayerInventory.Instance.GetItemCount(type);
            
            GameObject slotObj = Instantiate(itemSlotPrefab, itemsGridContainer);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
            
            if (slotUI != null)
            {
                ItemData itemData = GetItemDataByType(type);
                
                slotUI.SetItem(itemData, quantity);
                slotUI.dragPrefab = dragItemPrefab;
                itemSlots.Add(slotUI);
            }
        }
    }
    
    private void PopulatePowerUpsList()
    {
        foreach (var entry in powerUpEntries)
        {
            if (entry != null) Destroy(entry.gameObject);
        }
        powerUpEntries.Clear();
        
        if (PlayerInventory.Instance == null || powerUpEntryPrefab == null || powerUpsListContainer == null)
            return;
        
        foreach (var progress in PlayerInventory.Instance.powerUpProgress)
        {
            if (progress.unlocked)
            {
                PowerUpData data = CraftingSystem.Instance?.GetPowerUpData(progress.powerUpType);
                
                if (data != null)
                {
                    GameObject entryObj = Instantiate(powerUpEntryPrefab, powerUpsListContainer);
                    PowerUpEntryUI entryUI = entryObj.GetComponent<PowerUpEntryUI>();
                    
                    if (entryUI != null)
                    {
                        entryUI.Setup(data);
                        powerUpEntries.Add(entryUI);
                    }
                }
            }
        }
    }
    
    private void RefreshDisplay()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.itemData != null)
            {
                int quantity = PlayerInventory.Instance.GetItemCount(slot.itemData.itemType);
                slot.SetItem(slot.itemData, quantity);
            }
        }
        
        PopulatePowerUpsList();
    }
    
    private void OnCraftingSlotChanged(int slotIndex, ItemType? itemType)
    {
        UpdateResultDisplay();
    }
    
    private void UpdateResultDisplay()
    {
        if (CraftingSystem.Instance == null) return;
        
        PowerUpData matchingPowerUp = CraftingSystem.Instance.FindMatchingPowerUp();
        
        if (matchingPowerUp != null)
        {
            if (resultIconImage != null)
            {
                resultIconImage.sprite = matchingPowerUp.icon;
                resultIconImage.enabled = true;
            }
            
            if (resultNameText != null)
            {
                int currentLevel = PlayerInventory.Instance?.GetPowerUpLevel(matchingPowerUp.powerUpType) ?? 0;
                
                if (currentLevel == 0)
                {
                    resultNameText.text = $"{matchingPowerUp.powerUpName}\n<size=18>UNLOCK</size>";
                }
                else
                {
                    resultNameText.text = $"{matchingPowerUp.powerUpName}\nLv.{currentLevel} → Lv.{currentLevel + 1}";
                }
            }
            
            if (craftButton != null)
            {
                craftButton.interactable = true;
            }
        }
        else
        {
            if (resultIconImage != null)
            {
                resultIconImage.enabled = false;
            }
            
            if (resultNameText != null)
            {
                bool allFilled = CraftingSystem.Instance.AreAllSlotsFilled();
                resultNameText.text = allFilled ? "???\nInvalid Recipe" : "???\nAdd Items";
            }
            
            if (craftButton != null)
            {
                craftButton.interactable = false;
            }
        }
    }
    
    private void OnCraftButtonClicked()
    {
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.TryCraft();
        }
    }
    
    private void OnCraftSuccess(PowerUpData powerUp, int newLevel)
    {
        ShowFeedback($"Crafted {powerUp.powerUpName} Lv.{newLevel}!", Color.green);
        
        foreach (var slot in craftingSlots)
        {
            if (slot != null) slot.ClearSlot();
        }
        
        RefreshDisplay();
    }
    
    private void OnCraftFailed(string reason)
    {
        ShowFeedback(reason, Color.red);
    }
    
    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.enabled = true;
            
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), feedbackDuration);
        }
        else
        {
            Debug.Log($"[Inventory] {message}");
        }
    }
    
    private void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.enabled = false;
        }
    }
    
    // ✅ FIXED: Better item lookup (my mistake)
    private ItemData GetItemDataByType(ItemType type)
    {
        // Try allItemsDatabase first (assign in Inspector)
        if (allItemsDatabase != null)
        {
            foreach (var item in allItemsDatabase)
            {
                if (item != null && item.itemType == type)
                {
                    return item;
                }
            }
        }
        
        // Fallback: Try ItemDropper
        if (ItemDropper.Instance != null && ItemDropper.Instance.allItems != null)
        {
            foreach (var item in ItemDropper.Instance.allItems)
            {
                if (item != null && item.itemType == type)
                {
                    return item;
                }
            }
        }
        
        Debug.LogWarning($"[InventoryUI] ItemData not found for type: {type}");
        return null;
    }
    
    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}