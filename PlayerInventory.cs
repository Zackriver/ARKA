using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ItemStack
{
    public ItemType itemType;
    public int quantity;
    
    public ItemStack(ItemType type, int qty = 1)
    {
        itemType = type;
        quantity = qty;
    }
}

[Serializable]
public class PowerUpProgress
{
    public PowerUpType powerUpType;
    public int level;
    public bool unlocked;
    public float rechargeTimer;
    
    public PowerUpProgress(PowerUpType type)
    {
        powerUpType = type;
        level = 0;
        unlocked = false;
        rechargeTimer = 0f;
    }
}

[Serializable]
public class InventorySaveData
{
    public List<ItemStack> items = new List<ItemStack>();
    public List<PowerUpProgress> powerUps = new List<PowerUpProgress>();
    public int playerLevel = 1;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    
    [Header("Inventory")]
    public List<ItemStack> items = new List<ItemStack>();
    public int maxStackSize = 99;
    
    [Header("Power-Ups")]
    public List<PowerUpProgress> powerUpProgress = new List<PowerUpProgress>();
    
    [Header("Player Progress")]
    public int playerLevel = 1;
    
    [Header("Settings")]
    [Tooltip("Level required to use multiple power-ups at once")]
    public int multiPowerUpUnlockLevel = 50;
    
    // Events
    public event Action<ItemType, int> OnItemAdded;
    public event Action<ItemType, int> OnItemRemoved;
    public event Action<PowerUpType, int> OnPowerUpLevelUp;
    public event Action OnInventoryChanged;
    
    private const string SAVE_KEY = "PlayerInventory";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory();
            InitializePowerUpProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePowerUpProgress()
    {
        // Initialize progress for all power-up types if not exists
        foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
        {
            if (!HasPowerUpProgress(type))
            {
                powerUpProgress.Add(new PowerUpProgress(type));
            }
        }
    }
    
    private bool HasPowerUpProgress(PowerUpType type)
    {
        foreach (var progress in powerUpProgress)
        {
            if (progress.powerUpType == type) return true;
        }
        return false;
    }
    
    // ==================== ITEM MANAGEMENT ====================
    
    public void AddItem(ItemType type, int quantity = 1)
    {
        ItemStack existing = GetItemStack(type);
        
        if (existing != null)
        {
            existing.quantity = Mathf.Min(existing.quantity + quantity, maxStackSize);
        }
        else
        {
            items.Add(new ItemStack(type, Mathf.Min(quantity, maxStackSize)));
        }
        
        OnItemAdded?.Invoke(type, quantity);
        OnInventoryChanged?.Invoke();
        SaveInventory();
    }
    
    public bool RemoveItem(ItemType type, int quantity = 1)
    {
        ItemStack existing = GetItemStack(type);
        
        if (existing == null || existing.quantity < quantity)
        {
            return false;
        }
        
        existing.quantity -= quantity;
        
        if (existing.quantity <= 0)
        {
            items.Remove(existing);
        }
        
        OnItemRemoved?.Invoke(type, quantity);
        OnInventoryChanged?.Invoke();
        SaveInventory();
        return true;
    }
    
    public int GetItemCount(ItemType type)
    {
        ItemStack stack = GetItemStack(type);
        return stack != null ? stack.quantity : 0;
    }
    
    public ItemStack GetItemStack(ItemType type)
    {
        foreach (var stack in items)
        {
            if (stack.itemType == type) return stack;
        }
        return null;
    }
    
    public bool HasItems(ItemType[] types)
    {
        Dictionary<ItemType, int> required = new Dictionary<ItemType, int>();
        
        foreach (var type in types)
        {
            if (required.ContainsKey(type))
                required[type]++;
            else
                required[type] = 1;
        }
        
        foreach (var kvp in required)
        {
            if (GetItemCount(kvp.Key) < kvp.Value)
                return false;
        }
        
        return true;
    }
    
    // ==================== POWER-UP MANAGEMENT ====================
    
    public PowerUpProgress GetPowerUpProgress(PowerUpType type)
    {
        foreach (var progress in powerUpProgress)
        {
            if (progress.powerUpType == type) return progress;
        }
        return null;
    }
    
    public bool IsPowerUpUnlocked(PowerUpType type)
    {
        var progress = GetPowerUpProgress(type);
        return progress != null && progress.unlocked;
    }
    
    public int GetPowerUpLevel(PowerUpType type)
    {
        var progress = GetPowerUpProgress(type);
        return progress != null ? progress.level : 0;
    }
    
    public bool UnlockOrUpgradePowerUp(PowerUpType type)
    {
        var progress = GetPowerUpProgress(type);
        
        if (progress == null)
        {
            progress = new PowerUpProgress(type);
            powerUpProgress.Add(progress);
        }
        
        if (!progress.unlocked)
        {
            progress.unlocked = true;
            progress.level = 1;
        }
        else
        {
            progress.level++;
        }
        
        OnPowerUpLevelUp?.Invoke(type, progress.level);
        OnInventoryChanged?.Invoke();
        SaveInventory();
        
        return true;
    }
    
    public bool IsPowerUpReady(PowerUpType type)
    {
        var progress = GetPowerUpProgress(type);
        return progress != null && progress.unlocked && progress.rechargeTimer <= 0f;
    }
    
    public void StartRecharge(PowerUpType type, float rechargeTime)
    {
        var progress = GetPowerUpProgress(type);
        if (progress != null)
        {
            progress.rechargeTimer = rechargeTime;
        }
    }
    
    public void UpdateRechargeTimers(float deltaTime)
    {
        foreach (var progress in powerUpProgress)
        {
            if (progress.rechargeTimer > 0f)
            {
                progress.rechargeTimer -= deltaTime;
                if (progress.rechargeTimer < 0f)
                {
                    progress.rechargeTimer = 0f;
                }
            }
        }
    }
    
    public bool CanUseMultiplePowerUps()
    {
        return playerLevel >= multiPowerUpUnlockLevel;
    }
    
    // ==================== SAVE/LOAD ====================
    
    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData
        {
            items = new List<ItemStack>(items),
            powerUps = new List<PowerUpProgress>(powerUpProgress),
            playerLevel = playerLevel
        };
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }
    
    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
            
            if (data != null)
            {
                items = data.items ?? new List<ItemStack>();
                powerUpProgress = data.powerUps ?? new List<PowerUpProgress>();
                playerLevel = data.playerLevel;
            }
        }
    }
    
    public void ClearInventory()
    {
        items.Clear();
        powerUpProgress.Clear();
        playerLevel = 1;
        InitializePowerUpProgress();
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }
    
    // ==================== DEBUG ====================
    
    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            AddItem(type, 10);
        }
    }
    
    [ContextMenu("Clear All Items")]
    public void ClearAllItems()
    {
        items.Clear();
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }
}