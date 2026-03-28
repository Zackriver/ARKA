using UnityEngine;

/// <summary>
/// UI manager for crafting interface
/// Place in: Assets/Scripts/UI/CraftingManager.cs
/// </summary>
[DefaultExecutionOrder(-20)]
public class CraftingManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static CraftingManager _instance;
    
    public static CraftingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CraftingManager>();
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    [Header("UI References")]
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private CraftingSlotUI[] craftingSlotUIs;
    [SerializeField] private ItemSlotUI[] inventorySlotUIs;
    [SerializeField] private UnityEngine.UI.Button craftButton;
    [SerializeField] private UnityEngine.UI.Button cancelButton;
    
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    private bool isOpen = false;
    
    public bool IsOpen => isOpen;
    
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
        
        ValidateReferences();
        InitializeButtons();
    }
    
    private void OnEnable()
    {
        GameEvents.OnCraftingSlotChanged += HandleCraftingSlotChanged;
        GameEvents.OnCraftingSuccess += HandleCraftingSuccess;
        GameEvents.OnCraftingFailed += HandleCraftingFailed;
        GameEvents.OnInventoryChanged += HandleInventoryChanged;
    }
    
    private void OnDisable()
    {
        GameEvents.OnCraftingSlotChanged -= HandleCraftingSlotChanged;
        GameEvents.OnCraftingSuccess -= HandleCraftingSuccess;
        GameEvents.OnCraftingFailed -= HandleCraftingFailed;
        GameEvents.OnInventoryChanged -= HandleInventoryChanged;
    }
    
    private void Start()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Toggle crafting UI with 'C' key
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCraftingUI();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void ValidateReferences()
    {
        if (craftingPanel == null)
        {
            Debug.LogError("[CraftingManager] Crafting panel not assigned!");
        }
        
        if (craftingSlotUIs == null || craftingSlotUIs.Length == 0)
        {
            Debug.LogWarning("[CraftingManager] No crafting slot UIs assigned!");
        }
        
        if (craftButton == null)
        {
            Debug.LogWarning("[CraftingManager] Craft button not assigned!");
        }
        
        if (cancelButton == null)
        {
            Debug.LogWarning("[CraftingManager] Cancel button not assigned!");
        }
    }
    
    private void InitializeButtons()
    {
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UI CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    public void OpenCraftingUI()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(true);
            isOpen = true;
            
            RefreshUI();
            
            Debug.Log("[CraftingManager] Crafting UI opened");
        }
    }
    
    public void CloseCraftingUI()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
            isOpen = false;
            
            Debug.Log("[CraftingManager] Crafting UI closed");
        }
    }
    
    public void ToggleCraftingUI()
    {
        if (isOpen)
        {
            CloseCraftingUI();
        }
        else
        {
            OpenCraftingUI();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UI REFRESH
    // ═══════════════════════════════════════════════════════════════
    
    private void RefreshUI()
    {
        RefreshCraftingSlots();
        RefreshInventorySlots();
        UpdateCraftButtonState();
    }
    
    private void RefreshCraftingSlots()
    {
        if (craftingSlotUIs == null || CraftingSystem.Instance == null) return;
        
        for (int i = 0; i < craftingSlotUIs.Length; i++)
        {
            if (craftingSlotUIs[i] != null)
            {
                craftingSlotUIs[i].RefreshSlot();
            }
        }
    }
    
    private void RefreshInventorySlots()
    {
        if (inventorySlotUIs == null || PlayerInventory.Instance == null) return;
        
        var allItems = PlayerInventory.Instance.GetAllItems();
        int slotIndex = 0;
        
        // Fill slots with inventory items
        foreach (var kvp in allItems)
        {
            if (slotIndex >= inventorySlotUIs.Length) break;
            
            if (inventorySlotUIs[slotIndex] != null)
            {
                inventorySlotUIs[slotIndex].SetItem(kvp.Key, kvp.Value, null);
            }
            
            slotIndex++;
        }
        
        // Clear remaining slots
        for (int i = slotIndex; i < inventorySlotUIs.Length; i++)
        {
            if (inventorySlotUIs[i] != null)
            {
                inventorySlotUIs[i].ClearSlot();
            }
        }
    }
    
    private void UpdateCraftButtonState()
    {
        if (craftButton == null || CraftingSystem.Instance == null) return;
        
        bool canCraft = CraftingSystem.Instance.CanCraft();
        craftButton.interactable = canCraft;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleCraftingSlotChanged(int slotIndex, ItemData item)
    {
        RefreshCraftingSlots();
        UpdateCraftButtonState();
    }
    
    private void HandleCraftingSuccess(CraftingRecipe recipe, ItemData result)
    {
        RefreshUI();
    }
    
    private void HandleCraftingFailed(CraftingRecipe recipe, string reason)
    {
        RefreshUI();
    }
    
    private void HandleInventoryChanged()
    {
        RefreshInventorySlots();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BUTTON CALLBACKS
    // ═══════════════════════════════════════════════════════════════
    
    private void OnCraftButtonClicked()
    {
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.AttemptCraft();
        }
    }
    
    private void OnCancelButtonClicked()
    {
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.CancelCrafting();
        }
        
        CloseCraftingUI();
    }
}