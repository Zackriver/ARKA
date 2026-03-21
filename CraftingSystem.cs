using UnityEngine;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }
    
    [Header("Power-Up Database")]
    public List<PowerUpData> allPowerUps = new List<PowerUpData>();
    
    [Header("Crafting Slots")]
    public ItemType?[] craftingSlots = new ItemType?[4];
    
    // Events
    public event System.Action<int, ItemType?> OnSlotChanged;
    public event System.Action<PowerUpData, int> OnCraftSuccess;
    public event System.Action<string> OnCraftFailed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetSlot(int slotIndex, ItemType? itemType)
    {
        if (slotIndex < 0 || slotIndex >= 4) return;
        
        craftingSlots[slotIndex] = itemType;
        OnSlotChanged?.Invoke(slotIndex, itemType);
    }
    
    public void ClearSlot(int slotIndex)
    {
        SetSlot(slotIndex, null);
    }
    
    public void ClearAllSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            craftingSlots[i] = null;
            OnSlotChanged?.Invoke(i, null);
        }
    }
    
    public bool AreAllSlotsFilled()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!craftingSlots[i].HasValue) return false;
        }
        return true;
    }
    
    public PowerUpData FindMatchingPowerUp()
    {
        if (!AreAllSlotsFilled()) return null;
        
        ItemType[] items = new ItemType[4];
        for (int i = 0; i < 4; i++)
        {
            items[i] = craftingSlots[i].Value;
        }
        
        foreach (var powerUp in allPowerUps)
        {
            if (powerUp.MatchesRecipe(items))
            {
                return powerUp;
            }
        }
        
        return null;
    }
    
    public bool TryCraft()
    {
        if (!AreAllSlotsFilled())
        {
            OnCraftFailed?.Invoke("Fill all 4 slots first!");
            return false;
        }
        
        // Check if player has the items
        ItemType[] items = new ItemType[4];
        for (int i = 0; i < 4; i++)
        {
            items[i] = craftingSlots[i].Value;
        }
        
        if (!PlayerInventory.Instance.HasItems(items))
        {
            OnCraftFailed?.Invoke("Not enough items!");
            return false;
        }
        
        // Find matching power-up
        PowerUpData matchingPowerUp = FindMatchingPowerUp();
        
        if (matchingPowerUp == null)
        {
            OnCraftFailed?.Invoke("Invalid recipe! No power-up matches.");
            return false;
        }
        
        // Remove items from inventory
        foreach (var item in items)
        {
            PlayerInventory.Instance.RemoveItem(item, 1);
        }
        
        // Unlock or upgrade power-up
        PlayerInventory.Instance.UnlockOrUpgradePowerUp(matchingPowerUp.powerUpType);
        
        int newLevel = PlayerInventory.Instance.GetPowerUpLevel(matchingPowerUp.powerUpType);
        
        OnCraftSuccess?.Invoke(matchingPowerUp, newLevel);
        
        // Clear slots
        ClearAllSlots();
        
        return true;
    }
    
    public PowerUpData GetPowerUpData(PowerUpType type)
    {
        foreach (var powerUp in allPowerUps)
        {
            if (powerUp.powerUpType == type)
            {
                return powerUp;
            }
        }
        return null;
    }
    
    public List<PowerUpData> GetCraftablePowerUps()
    {
        List<PowerUpData> craftable = new List<PowerUpData>();
        
        foreach (var powerUp in allPowerUps)
        {
            ItemType[] recipe = powerUp.GetRecipe();
            if (PlayerInventory.Instance.HasItems(recipe))
            {
                craftable.Add(powerUp);
            }
        }
        
        return craftable;
    }
}