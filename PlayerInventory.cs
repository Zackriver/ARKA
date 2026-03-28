using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player inventory system with events and save-ready structure
/// Place in: Assets/Scripts/Systems/PlayerInventory.cs
/// </summary>
[DefaultExecutionOrder(-60)]
public class PlayerInventory : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static PlayerInventory _instance;
    
    public static PlayerInventory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerInventory>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[PlayerInventory]");
                    _instance = go.AddComponent<PlayerInventory>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY DATA
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = Constants.Inventory.DEFAULT_SLOTS;
    
    // Item storage: ItemType -> Quantity
    private Dictionary<ItemType, int> inventory = new Dictionary<ItemType, int>();
    
    public int MaxSlots => maxSlots;
    public int UsedSlots => inventory.Count;
    public int FreeSlots => maxSlots - UsedSlots;
    
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
        
        InitializeInventory();
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
    
    private void InitializeInventory()
    {
        inventory.Clear();
        Debug.Log($"[PlayerInventory] Initialized with {maxSlots} slots");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ADD ITEMS
    // ═══════════════════════════════════════════════════════════════
    
    public bool AddItem(ItemType itemType, int amount = 1)
    {
        if (itemType == ItemType.None)
        {
            Debug.LogWarning("[PlayerInventory] Cannot add None item type!");
            return false;
        }
        
        if (amount <= 0)
        {
            Debug.LogWarning($"[PlayerInventory] Invalid amount: {amount}");
            return false;
        }
        
        // Check if we already have this item
        if (inventory.ContainsKey(itemType))
        {
            // Check stack limit
            int currentAmount = inventory[itemType];
            int newAmount = currentAmount + amount;
            
            if (newAmount > Constants.Inventory.MAX_STACK_SIZE)
            {
                Debug.LogWarning($"[PlayerInventory] Stack limit reached for {itemType}!");
                return false;
            }
            
            inventory[itemType] = newAmount;
        }
        else
        {
            // New item - check slot availability
            if (UsedSlots >= maxSlots)
            {
                Debug.LogWarning("[PlayerInventory] Inventory full!");
                GameEvents.ShowMessage("Inventory full!", 2f);
                return false;
            }
            
            inventory.Add(itemType, amount);
        }
        
        // Fire event
        GameEvents.ItemAdded(null, amount);
        
        Debug.Log($"[PlayerInventory] Added {amount}x {itemType}. Total: {inventory[itemType]}");
        
        return true;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // REMOVE ITEMS
    // ═══════════════════════════════════════════════════════════════
    
    public bool RemoveItem(ItemType itemType, int amount = 1)
    {
        if (itemType == ItemType.None)
        {
            Debug.LogWarning("[PlayerInventory] Cannot remove None item type!");
            return false;
        }
        
        if (amount <= 0)
        {
            Debug.LogWarning($"[PlayerInventory] Invalid amount: {amount}");
            return false;
        }
        
        // Check if we have this item
        if (!inventory.ContainsKey(itemType))
        {
            Debug.LogWarning($"[PlayerInventory] Item not found: {itemType}");
            return false;
        }
        
        int currentAmount = inventory[itemType];
        
        if (currentAmount < amount)
        {
            Debug.LogWarning($"[PlayerInventory] Not enough {itemType}. Have: {currentAmount}, Need: {amount}");
            return false;
        }
        
        int newAmount = currentAmount - amount;
        
        if (newAmount <= 0)
        {
            // Remove completely
            inventory.Remove(itemType);
        }
        else
        {
            inventory[itemType] = newAmount;
        }
        
        // Fire event
        GameEvents.ItemRemoved(null, amount);
        
        Debug.Log($"[PlayerInventory] Removed {amount}x {itemType}. Remaining: {newAmount}");
        
        return true;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // QUERY ITEMS
    // ═══════════════════════════════════════════════════════════════
    
    public bool HasItem(ItemType itemType, int amount = 1)
    {
        if (!inventory.ContainsKey(itemType))
        {
            return false;
        }
        
        return inventory[itemType] >= amount;
    }
    
    public int GetItemCount(ItemType itemType)
    {
        if (!inventory.ContainsKey(itemType))
        {
            return 0;
        }
        
        return inventory[itemType];
    }
    
    public Dictionary<ItemType, int> GetAllItems()
    {
        return new Dictionary<ItemType, int>(inventory);
    }
    
    public List<ItemType> GetItemTypes()
    {
        return new List<ItemType>(inventory.Keys);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    public void ClearInventory()
    {
        inventory.Clear();
        GameEvents.InventoryChanged();
        
        Debug.Log("[PlayerInventory] Inventory cleared");
    }
    
    public bool IsFull()
    {
        return UsedSlots >= maxSlots;
    }
    
    public bool IsEmpty()
    {
        return inventory.Count == 0;
    }
    
    public void SetMaxSlots(int slots)
    {
        maxSlots = Mathf.Max(1, slots);
        Debug.Log($"[PlayerInventory] Max slots set to {maxSlots}");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SAVE/LOAD SUPPORT
    // ═══════════════════════════════════════════════════════════════
    
    [System.Serializable]
    public class InventorySaveData
    {
        public ItemType[] itemTypes;
        public int[] amounts;
        public int maxSlots;
    }
    
    public InventorySaveData GetSaveData()
    {
        InventorySaveData data = new InventorySaveData();
        data.maxSlots = maxSlots;
        
        List<ItemType> types = new List<ItemType>();
        List<int> amounts = new List<int>();
        
        foreach (var kvp in inventory)
        {
            types.Add(kvp.Key);
            amounts.Add(kvp.Value);
        }
        
        data.itemTypes = types.ToArray();
        data.amounts = amounts.ToArray();
        
        return data;
    }
    
    public void LoadSaveData(InventorySaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[PlayerInventory] Cannot load null save data!");
            return;
        }
        
        ClearInventory();
        
        maxSlots = data.maxSlots;
        
        if (data.itemTypes != null && data.amounts != null)
        {
            int count = Mathf.Min(data.itemTypes.Length, data.amounts.Length);
            
            for (int i = 0; i < count; i++)
            {
                inventory[data.itemTypes[i]] = data.amounts[i];
            }
        }
        
        GameEvents.InventoryChanged();
        
        Debug.Log($"[PlayerInventory] Loaded {inventory.Count} items from save data");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 310, 420, 300, 300));
        GUILayout.Label("=== Player Inventory ===");
        GUILayout.Label($"Slots: {UsedSlots}/{maxSlots}");
        GUILayout.Label("");
        GUILayout.Label("Items:");
        
        if (inventory.Count == 0)
        {
            GUILayout.Label("  Empty");
        }
        else
        {
            foreach (var kvp in inventory)
            {
                GUILayout.Label($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        GUILayout.EndArea();
    }
}