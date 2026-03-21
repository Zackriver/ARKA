using UnityEngine;
using System.Collections.Generic;

public class ItemDropper : MonoBehaviour
{
    public static ItemDropper Instance { get; private set; }
    
    [Header("Drop Settings")]
    public GameObject itemDropPrefab;
    public List<ItemData> allItems = new List<ItemData>();
    public List<DebuffData> allDebuffs = new List<DebuffData>();
    
    [Header("Drop Chances")]
    [Range(0f, 1f)]
    public float baseDropChance = 0.3f;
    
    [Range(0f, 1f)]
    public float debuffDropChance = 0.05f;
    
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
    
    public void TryDropItem(Vector3 position)
    {
        // First check if we should drop anything at all
        if (Random.value > baseDropChance) return;
        
        // Check if power-up is active - might drop debuff
        bool powerUpActive = PowerUpManager.Instance != null && PowerUpManager.Instance.HasActivePowerUp();
        
        if (powerUpActive && Random.value < debuffDropChance)
        {
            DropDebuff(position);
            return;
        }
        
        // Drop normal item based on rarity
        DropItem(position);
    }
    
    private void DropItem(Vector3 position)
    {
        if (allItems.Count == 0 || itemDropPrefab == null) return;
        
        // Select item based on rarity weights
        ItemData selectedItem = SelectItemByRarity();
        
        if (selectedItem != null)
        {
            SpawnItemDrop(position, selectedItem, null);
        }
    }
    
    private void DropDebuff(Vector3 position)
    {
        if (allDebuffs.Count == 0 || itemDropPrefab == null) return;
        
        // Select random debuff
        DebuffData selectedDebuff = allDebuffs[Random.Range(0, allDebuffs.Count)];
        
        SpawnItemDrop(position, null, selectedDebuff);
    }
    
    private ItemData SelectItemByRarity()
    {
        // Build weighted list
        float totalWeight = 0f;
        foreach (var item in allItems)
        {
            totalWeight += item.GetDropChance();
        }
        
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;
        
        foreach (var item in allItems)
        {
            currentWeight += item.GetDropChance();
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }
        
        // Fallback to first item
        return allItems.Count > 0 ? allItems[0] : null;
    }
    
    private void SpawnItemDrop(Vector3 position, ItemData item, DebuffData debuff)
    {
        GameObject dropObj = Instantiate(itemDropPrefab, position, Quaternion.identity);
        ItemDrop drop = dropObj.GetComponent<ItemDrop>();
        
        if (drop != null)
        {
            if (debuff != null)
            {
                drop.InitializeAsDebuff(debuff);
            }
            else if (item != null)
            {
                drop.Initialize(item);
            }
        }
    }
    
    // Called from Brick when destroyed
    public void OnBrickDestroyed(Vector3 brickPosition)
    {
        TryDropItem(brickPosition);
    }
}