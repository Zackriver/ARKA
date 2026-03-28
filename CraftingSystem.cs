using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages crafting logic with transaction pattern to prevent item loss
/// Place in: Assets/Scripts/Systems/CraftingSystem.cs
/// </summary>
[DefaultExecutionOrder(-30)]
public class CraftingSystem : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static CraftingSystem _instance;
    
    public static CraftingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CraftingSystem>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[CraftingSystem]");
                    _instance = go.AddComponent<CraftingSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Available Recipes")]
    [SerializeField] private CraftingRecipe[] availableRecipes;
    
    [Header("Available Power-Ups")]
    [SerializeField] private PowerUpData[] availablePowerUps;
    
    // ═══════════════════════════════════════════════════════════════
    // CRAFTING STATE
    // ═══════════════════════════════════════════════════════════════
    
    private ItemType[] craftingSlots = new ItemType[Constants.Inventory.CRAFTING_SLOTS];
    private bool[] slotsFilled = new bool[Constants.Inventory.CRAFTING_SLOTS];
    
    public int FilledSlotsCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < slotsFilled.Length; i++)
            {
                if (slotsFilled[i]) count++;
            }
            return count;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeSlots();
        LoadRecipes();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void InitializeSlots()
    {
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            craftingSlots[i] = ItemType.None;
            slotsFilled[i] = false;
        }
    }
    
    private void LoadRecipes()
    {
        if (availableRecipes == null || availableRecipes.Length == 0)
        {
            Debug.LogWarning("[CraftingSystem] No crafting recipes assigned!");
        }
        
        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            Debug.LogWarning("[CraftingSystem] No power-ups assigned!");
        }
        
        Debug.Log($"[CraftingSystem] Loaded {availableRecipes?.Length ?? 0} recipes and {availablePowerUps?.Length ?? 0} power-ups");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SLOT MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    public bool AddItemToSlot(int slotIndex, ItemType itemType)
    {
        if (slotIndex < 0 || slotIndex >= craftingSlots.Length)
        {
            Debug.LogError($"[CraftingSystem] Invalid slot index: {slotIndex}");
            return false;
        }
        
        if (slotsFilled[slotIndex])
        {
            Debug.LogWarning($"[CraftingSystem] Slot {slotIndex} already filled!");
            return false;
        }
        
        if (itemType == ItemType.None)
        {
            Debug.LogWarning("[CraftingSystem] Cannot add None item type!");
            return false;
        }
        
        craftingSlots[slotIndex] = itemType;
        slotsFilled[slotIndex] = true;
        
        // Fire event
        GameEvents.CraftingSlotChanged(slotIndex, null);
        
        Debug.Log($"[CraftingSystem] Added {itemType} to slot {slotIndex}");
        
        // Auto-check for recipe match
        CheckRecipeMatch();
        
        return true;
    }
    
    public bool RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= craftingSlots.Length)
        {
            Debug.LogError($"[CraftingSystem] Invalid slot index: {slotIndex}");
            return false;
        }
        
        if (!slotsFilled[slotIndex])
        {
            Debug.LogWarning($"[CraftingSystem] Slot {slotIndex} is already empty!");
            return false;
        }
        
        ItemType removedItem = craftingSlots[slotIndex];
        craftingSlots[slotIndex] = ItemType.None;
        slotsFilled[slotIndex] = false;
        
        // Fire event
        GameEvents.CraftingSlotChanged(slotIndex, null);
        
        Debug.Log($"[CraftingSystem] Removed {removedItem} from slot {slotIndex}");
        
        return true;
    }
    
    public ItemType GetItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= craftingSlots.Length)
        {
            return ItemType.None;
        }
        
        return slotsFilled[slotIndex] ? craftingSlots[slotIndex] : ItemType.None;
    }
    
    public bool IsSlotFilled(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= craftingSlots.Length)
        {
            return false;
        }
        
        return slotsFilled[slotIndex];
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CRAFTING LOGIC
    // ═══════════════════════════════════════════════════════════════
    
    public void AttemptCraft()
    {
        // Check if all slots filled
        if (FilledSlotsCount != Constants.Inventory.CRAFTING_SLOTS)
        {
            GameEvents.ShowMessage($"Need {Constants.Inventory.CRAFTING_SLOTS} items to craft!", 2f);
            return;
        }
        
        // Check for recipe match
        PowerUpData matchedPowerUp = FindMatchingPowerUp();
        
        if (matchedPowerUp != null)
        {
            CraftPowerUp(matchedPowerUp);
        }
        else
        {
            CraftFailed("No matching recipe found!");
        }
    }
    
    private PowerUpData FindMatchingPowerUp()
    {
        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            return null;
        }
        
        // Get current items
        ItemType[] currentItems = new ItemType[Constants.Inventory.CRAFTING_SLOTS];
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            currentItems[i] = craftingSlots[i];
        }
        
        // Check each power-up
        foreach (PowerUpData powerUp in availablePowerUps)
        {
            if (powerUp != null && powerUp.MatchesRecipe(currentItems))
            {
                return powerUp;
            }
        }
        
        return null;
    }
    
    private void CheckRecipeMatch()
    {
        if (FilledSlotsCount == Constants.Inventory.CRAFTING_SLOTS)
        {
            PowerUpData match = FindMatchingPowerUp();
            
            if (match != null)
            {
                GameEvents.ShowMessage($"Recipe ready: {match.powerUpName}!", 2f);
            }
        }
    }
    
    private void CraftPowerUp(PowerUpData powerUp)
    {
        Debug.Log($"[CraftingSystem] Crafting {powerUp.powerUpName}");
        
        // TRANSACTION PATTERN: Remove items from inventory BEFORE clearing slots
        bool itemsRemoved = RemoveItemsFromInventory();
        
        if (!itemsRemoved)
        {
            CraftFailed("Failed to remove items from inventory!");
            return;
        }
        
        // Clear crafting slots
        ClearSlots();
        
        // Activate power-up
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ActivatePowerUp(powerUp);
        }
        
        // Fire events
        GameEvents.CraftingSuccess(null, null);
        GameEvents.ShowMessage($"Crafted: {powerUp.powerUpName}!", 3f);
        GameEvents.PlaySound("CraftSuccess", Vector3.zero);
        
        Debug.Log($"[CraftingSystem] Successfully crafted {powerUp.powerUpName}");
    }
    
    private void CraftFailed(string reason)
    {
        Debug.LogWarning($"[CraftingSystem] Craft failed: {reason}");
        
        // TRANSACTION PATTERN: Return items to inventory
        ReturnItemsToInventory();
        
        // Clear slots
        ClearSlots();
        
        // Fire events
        GameEvents.CraftingFailed(null, reason);
        GameEvents.ShowMessage($"Crafting failed: {reason}", 2f);
        GameEvents.PlaySound("CraftFail", Vector3.zero);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // TRANSACTION HELPERS
    // ═══════════════════════════════════════════════════════════════
    
    private bool RemoveItemsFromInventory()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[CraftingSystem] PlayerInventory not found!");
            return false;
        }
        
        // Try to remove all items
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            if (slotsFilled[i])
            {
                bool removed = PlayerInventory.Instance.RemoveItem(craftingSlots[i], 1);
                
                if (!removed)
                {
                    Debug.LogError($"[CraftingSystem] Failed to remove {craftingSlots[i]} from inventory!");
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void ReturnItemsToInventory()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[CraftingSystem] PlayerInventory not found!");
            return;
        }
        
        // Return all items in crafting slots
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            if (slotsFilled[i] && craftingSlots[i] != ItemType.None)
            {
                PlayerInventory.Instance.AddItem(craftingSlots[i], 1);
                Debug.Log($"[CraftingSystem] Returned {craftingSlots[i]} to inventory");
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CLEAR SLOTS
    // ═══════════════════════════════════════════════════════════════
    
    public void ClearSlots()
    {
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            if (slotsFilled[i])
            {
                RemoveItemFromSlot(i);
            }
        }
        
        Debug.Log("[CraftingSystem] All crafting slots cleared");
    }
    
    public void CancelCrafting()
    {
        // Return items to inventory before clearing
        ReturnItemsToInventory();
        
        // Clear slots
        ClearSlots();
        
        GameEvents.ShowMessage("Crafting cancelled", 1.5f);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY
    // ═══════════════════════════════════════════════════════════════
    
    public bool CanCraft()
    {
        return FilledSlotsCount == Constants.Inventory.CRAFTING_SLOTS;
    }
    
    public PowerUpData GetCurrentRecipeMatch()
    {
        if (FilledSlotsCount != Constants.Inventory.CRAFTING_SLOTS)
        {
            return null;
        }
        
        return FindMatchingPowerUp();
    }
    
    public int GetSlotCount()
    {
        return craftingSlots.Length;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(10, 380, 300, 200));
        GUILayout.Label("=== Crafting System ===");
        GUILayout.Label($"Filled Slots: {FilledSlotsCount}/{Constants.Inventory.CRAFTING_SLOTS}");
        
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            string slotInfo = slotsFilled[i] ? craftingSlots[i].ToString() : "Empty";
            GUILayout.Label($"Slot {i}: {slotInfo}");
        }
        
        PowerUpData match = GetCurrentRecipeMatch();
        if (match != null)
        {
            GUILayout.Label($"Match: {match.powerUpName}");
        }
        
        GUILayout.EndArea();
    }
}