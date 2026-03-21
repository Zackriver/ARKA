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
    public GameObject dragItemPrefab; // For drag & drop
    
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
        
        // Subscribe to events
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
        
        // Setup craft button
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
        
        UpdateResultDisplay();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
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
        // Clear existing
        foreach (var slot in itemSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        itemSlots.Clear();
        
        if (PlayerInventory.Instance == null || itemSlotPrefab == null || itemsGridContainer == null)
            return;
        
        // Create slot for each item type
        foreach (System.Enum itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemType type = (ItemType)itemType;
            int quantity = PlayerInventory.Instance.GetItemCount(type);
            
            // Create slot
            GameObject slotObj = Instantiate(itemSlotPrefab, itemsGridContainer);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
            
            if (slotUI != null)
            {
                // Get item data from CraftingSystem (it has all items reference)
                ItemData itemData = GetItemDataByType(type);
                
                slotUI.SetItem(itemData, quantity);
                slotUI.dragPrefab = dragItemPrefab;
                itemSlots.Add(slotUI);
            }
        }
    }
    
    private void PopulatePowerUpsList()
    {
        // Clear existing
        foreach (var entry in powerUpEntries)
        {
            if (entry != null) Destroy(entry.gameObject);
        }
        powerUpEntries.Clear();
        
        if (PlayerInventory.Instance == null || powerUpEntryPrefab == null || powerUpsListContainer == null)
            return;
        
        // Get all unlocked power-ups
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
        // Update item quantities
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.itemData != null)
            {
                int quantity = PlayerInventory.Instance.GetItemCount(slot.itemData.itemType);
                slot.SetItem(slot.itemData, quantity);
            }
        }
        
        // Refresh power-ups list
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
            // Show what will be crafted
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
            // No match or incomplete recipe
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
        
        // Clear crafting slots
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
    
    private ItemData GetItemDataByType(ItemType type)
    {
        // This is a helper to get ItemData from ItemDropper's list
        // You might need to adjust based on your setup
        
        if (ItemDropper.Instance != null)
        {
            foreach (var item in ItemDropper.Instance.allItems)
            {
                if (item.itemType == type)
                {
                    return item;
                }
            }
        }
        
        return null;
    }
    
    public void OnBackButtonClicked()
    {
        // Return to menu
        SceneManager.LoadScene("Menu");
    }
}