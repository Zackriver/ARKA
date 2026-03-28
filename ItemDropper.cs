using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles item drops from destroyed bricks with pooling and event integration
/// Place in: Assets/Scripts/Systems/ItemDropper.cs
/// 
/// INSPECTOR SETUP:
/// 1. Attach to a GameObject in scene (or let it auto-create)
/// 2. Assign itemDropPrefab (prefab with ItemDrop component)
/// 3. Assign available items array (ItemData ScriptableObjects)
/// </summary>
[DefaultExecutionOrder(-40)]
public class ItemDropper : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static ItemDropper _instance;
    
    public static ItemDropper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemDropper>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[ItemDropper]");
                    _instance = go.AddComponent<ItemDropper>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Item Drop Prefab")]
    [SerializeField] private GameObject itemDropPrefab;
    
    [Header("Available Items")]
    [SerializeField] private ItemData[] availableItems;
    
    [Header("Drop Settings")]
    [SerializeField] private float dropChance = 0.3f; // 30% chance
    [SerializeField] private float dropFallSpeed = 3f;
    [SerializeField] private float dropLifetime = 10f; // Despawn after 10s if not collected
    
    [Header("Drop Weights (by rarity)")]
    [SerializeField] private float commonWeight = 50f;
    [SerializeField] private float uncommonWeight = 30f;
    [SerializeField] private float rareWeight = 15f;
    [SerializeField] private float legendaryWeight = 5f;
    
    // ═══════════════════════════════════════════════════════════════
    // OBJECT POOL
    // ═══════════════════════════════════════════════════════════════
    
    private const string ITEM_DROP_POOL = "ItemDrops";
    private const int POOL_SIZE = 20;
    
    // ═══════════════════════════════════════════════════════════════
    // CACHED REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    private Dictionary<ItemRarity, float> rarityWeights;
    private List<GameObject> activeDrops = new List<GameObject>();
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeRarityWeights();
        ValidateSettings();
    }
    
    private void OnEnable()
    {
        // Subscribe to events
        GameEvents.OnBrickDestroyed += HandleBrickDestroyed;
        GameEvents.OnItemDropped += HandleItemDropped;
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameEvents.OnBrickDestroyed -= HandleBrickDestroyed;
        GameEvents.OnItemDropped -= HandleItemDropped;
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
    }
    
    private void Start()
    {
        InitializePool();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            ClearAllDrops();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void InitializeRarityWeights()
    {
        rarityWeights = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Common, commonWeight },
            { ItemRarity.Uncommon, uncommonWeight },
            { ItemRarity.Rare, rareWeight },
            { ItemRarity.Legendary, legendaryWeight }
        };
    }
    
    private void ValidateSettings()
    {
        if (itemDropPrefab == null)
        {
            Debug.LogError("[ItemDropper] Item drop prefab not assigned!");
        }
        else if (itemDropPrefab.GetComponent<ItemDrop>() == null)
        {
            Debug.LogError("[ItemDropper] Item drop prefab missing ItemDrop component!");
        }
        
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("[ItemDropper] No available items assigned!");
        }
        else
        {
            // Remove null entries
            List<ItemData> validItems = new List<ItemData>();
            foreach (ItemData item in availableItems)
            {
                if (item != null)
                {
                    validItems.Add(item);
                }
            }
            availableItems = validItems.ToArray();
            
            Debug.Log($"[ItemDropper] Loaded {availableItems.Length} valid items");
        }
    }
    
    private void InitializePool()
    {
        if (itemDropPrefab == null)
        {
            Debug.LogError("[ItemDropper] Cannot initialize pool - prefab is null!");
            return;
        }
        
        // Create pool using PoolManager
        if (PoolManager.Instance != null && !PoolManager.Instance.HasPool(ITEM_DROP_POOL))
        {
            PoolManager.Instance.CreatePool(ITEM_DROP_POOL, itemDropPrefab, POOL_SIZE, true);
            Debug.Log($"[ItemDropper] Created item drop pool with {POOL_SIZE} objects");
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleBrickDestroyed(Brick brick, int scoreValue)
    {
        if (brick == null) return;
        
        // Roll for drop
        if (Random.value <= dropChance)
        {
            Vector3 dropPosition = brick.transform.position;
            ItemData randomItem = SelectRandomItem();
            
            if (randomItem != null)
            {
                DropItem(randomItem, dropPosition);
            }
        }
    }
    
    private void HandleItemDropped(ItemData item, Vector3 position)
    {
        // This can be called from other systems (like special bricks)
        if (item != null)
        {
            DropItem(item, position);
        }
        else
        {
            // Random item drop
            ItemData randomItem = SelectRandomItem();
            if (randomItem != null)
            {
                DropItem(randomItem, position);
            }
        }
    }
    
    private void HandleItemPickedUp(ItemData item)
    {
        // Item was collected - cleanup happens in ItemDrop component
        Debug.Log($"[ItemDropper] Item picked up: {item?.itemName ?? "Unknown"}");
    }
    
    private void HandleLevelLoaded(int levelIndex)
    {
        // Clear all drops when new level loads
        ClearAllDrops();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ITEM SELECTION
    // ═══════════════════════════════════════════════════════════════
    
    private ItemData SelectRandomItem()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("[ItemDropper] No available items to drop!");
            return null;
        }
        
        // First, select rarity based on weights
        ItemRarity selectedRarity = SelectRandomRarity();
        
        // Then, filter items by that rarity
        List<ItemData> itemsOfRarity = new List<ItemData>();
        foreach (ItemData item in availableItems)
        {
            if (item != null && item.rarity == selectedRarity)
            {
                itemsOfRarity.Add(item);
            }
        }
        
        // If no items of selected rarity, fall back to any item
        if (itemsOfRarity.Count == 0)
        {
            Debug.LogWarning($"[ItemDropper] No items of rarity {selectedRarity}, selecting random");
            return availableItems[Random.Range(0, availableItems.Length)];
        }
        
        // Select random item from filtered list
        return itemsOfRarity[Random.Range(0, itemsOfRarity.Count)];
    }
    
    private ItemRarity SelectRandomRarity()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var weight in rarityWeights.Values)
        {
            totalWeight += weight;
        }
        
        // Random roll
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        // Find selected rarity
        foreach (var kvp in rarityWeights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
            {
                return kvp.Key;
            }
        }
        
        // Fallback (shouldn't happen)
        return ItemRarity.Common;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DROP ITEM
    // ═══════════════════════════════════════════════════════════════
    
    public void DropItem(ItemData item, Vector3 position)
{
    if (item == null)
    {
        Debug.LogWarning("[ItemDropper] Attempted to drop null item!");
        return;
    }
    
    if (PoolManager.Instance == null || !PoolManager.Instance.HasPool(ITEM_DROP_POOL))
    {
        Debug.LogError("[ItemDropper] Item drop pool not initialized!");
        
        // Fallback: instantiate directly
        if (itemDropPrefab != null)
        {
            GameObject fallbackDrop = Instantiate(itemDropPrefab, position, Quaternion.identity);
            SetupItemDrop(fallbackDrop, item);
        }
        return;
    }
    
    // Get from pool
    GameObject pooledDrop = PoolManager.Instance.Spawn(ITEM_DROP_POOL, position, Quaternion.identity);
    
    if (pooledDrop != null)
    {
        SetupItemDrop(pooledDrop, item);
        activeDrops.Add(pooledDrop);
        
        Debug.Log($"[ItemDropper] Dropped {item.itemName} at {position}");
    }
}
    
    private void SetupItemDrop(GameObject dropObj, ItemData item)
    {
        ItemDrop itemDrop = dropObj.GetComponent<ItemDrop>();
        
        if (itemDrop != null)
        {
            itemDrop.Initialize(item, dropFallSpeed, dropLifetime);
        }
        else
        {
            Debug.LogError("[ItemDropper] Drop object missing ItemDrop component!");
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DROP MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    public void RemoveDrop(GameObject dropObj)
    {
        if (dropObj == null) return;
        
        activeDrops.Remove(dropObj);
        
        if (PoolManager.Instance != null && PoolManager.Instance.HasPool(ITEM_DROP_POOL))
        {
            PoolManager.Instance.Despawn(ITEM_DROP_POOL, dropObj);
        }
        else
        {
            Destroy(dropObj);
        }
    }
    
    public void ClearAllDrops()
    {
        // Despawn all active drops
        foreach (GameObject drop in activeDrops)
        {
            if (drop != null)
            {
                if (PoolManager.Instance != null && PoolManager.Instance.HasPool(ITEM_DROP_POOL))
                {
                    PoolManager.Instance.Despawn(ITEM_DROP_POOL, drop);
                }
                else
                {
                    Destroy(drop);
                }
            }
        }
        
        activeDrops.Clear();
        
        Debug.Log("[ItemDropper] All drops cleared");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════
    
    public void SetDropChance(float chance)
    {
        dropChance = Mathf.Clamp01(chance);
    }
    
    public float GetDropChance()
    {
        return dropChance;
    }
    
    public void SetDropFallSpeed(float speed)
    {
        dropFallSpeed = Mathf.Max(0, speed);
    }
    
    public int GetActiveDropCount()
    {
        // Remove null entries
        activeDrops.RemoveAll(drop => drop == null);
        return activeDrops.Count;
    }
    
    public void AddAvailableItem(ItemData item)
    {
        if (item == null) return;
        
        List<ItemData> items = new List<ItemData>(availableItems);
        if (!items.Contains(item))
        {
            items.Add(item);
            availableItems = items.ToArray();
            Debug.Log($"[ItemDropper] Added item: {item.itemName}");
        }
    }
    
    public void RemoveAvailableItem(ItemData item)
    {
        if (item == null) return;
        
        List<ItemData> items = new List<ItemData>(availableItems);
        if (items.Remove(item))
        {
            availableItems = items.ToArray();
            Debug.Log($"[ItemDropper] Removed item: {item.itemName}");
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(10, 220, 300, 150));
        GUILayout.Label("=== Item Dropper ===");
        GUILayout.Label($"Drop Chance: {dropChance * 100f:F0}%");
        GUILayout.Label($"Active Drops: {GetActiveDropCount()}");
        GUILayout.Label($"Available Items: {availableItems?.Length ?? 0}");
        
        if (PoolManager.Instance != null && PoolManager.Instance.HasPool(ITEM_DROP_POOL))
        {
            GUILayout.Label(PoolManager.Instance.GetPoolStats(ITEM_DROP_POOL));
        }
        
        GUILayout.EndArea();
    }
}