using UnityEngine;
using System.Collections;

/// <summary>
/// Individual item drop that falls from destroyed bricks
/// Place in: Assets/Scripts/Gameplay/ItemDrop.cs
/// 
/// REQUIRED COMPONENTS:
/// - Rigidbody2D (kinematic, gravity = 0)
/// - Collider2D (trigger = true)
/// - SpriteRenderer
/// 
/// INSPECTOR SETUP:
/// 1. Set layer to "Item"
/// 2. Set tag to "Item"
/// 3. Configure collider as trigger
/// 
/// PREFAB SETUP:
/// - Attach this script
/// - Add Circle/Box Collider 2D (isTrigger = true)
/// - Add Rigidbody2D (isKinematic = true, gravityScale = 0)
/// - Add SpriteRenderer
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemDrop : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Visual Effects")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 90f;
    
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowIntensity = 1.5f;
    
    [Header("Warning Flash")]
    [SerializeField] private bool flashBeforeDespawn = true;
    [SerializeField] private float flashStartTime = 2f; // Start flashing 2s before despawn
    [SerializeField] private float flashSpeed = 5f;
    
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    private ItemData itemData;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    
    private float fallSpeed;
    private float lifetime;
    private float spawnTime;
    private Vector3 originalScale;
    private Color originalColor;
    
    private bool isInitialized = false;
    private bool isCollected = false;
    
    // Visual effect state
    private float pulseTime;
    
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════
    
    public ItemData ItemData => itemData;
    public bool IsInitialized => isInitialized;
    public float RemainingLifetime => lifetime - (Time.time - spawnTime);
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        // Configure Rigidbody2D
        rb.gravityScale = Constants.Physics.GRAVITY_SCALE;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // Configure Collider
        col.isTrigger = true;
        
        // Store original scale
        originalScale = transform.localScale;
    }
    
    private void OnEnable()
    {
        spawnTime = Time.time;
        pulseTime = 0f;
        isCollected = false;
        
        // Reset scale
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
    }
    
    private void OnDisable()
    {
        // Reset state
        isInitialized = false;
        itemData = null;
    }
    
    private void Update()
    {
        if (!isInitialized || isCollected) return;
        
        // Movement
        UpdateMovement();
        
        // Visual effects
        if (enablePulseEffect)
        {
            UpdatePulseEffect();
        }
        
        if (enableRotation)
        {
            UpdateRotation();
        }
        
        // Check lifetime
        float remainingTime = RemainingLifetime;
        
        if (flashBeforeDespawn && remainingTime <= flashStartTime)
        {
            UpdateFlashEffect(remainingTime);
        }
        
        if (remainingTime <= 0f)
        {
            Despawn();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || isCollected) return;
        
        // Check if collected by paddle
        if (other.CompareTag(Constants.Tags.PLAYER))
        {
            Collect();
        }
        // Check if fell below screen (death zone)
        else if (other.CompareTag(Constants.Tags.DEATH_ZONE))
        {
            Despawn();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Initialize the item drop with data
    /// </summary>
    public void Initialize(ItemData data, float speed, float lifetime)
    {
        if (data == null)
        {
            Debug.LogError("[ItemDrop] Cannot initialize with null ItemData!");
            Despawn();
            return;
        }
        
        this.itemData = data;
        this.fallSpeed = speed;
        this.lifetime = lifetime;
        this.spawnTime = Time.time;
        this.isInitialized = true;
        this.isCollected = false;
        
        UpdateVisuals();
        
        Debug.Log($"[ItemDrop] Initialized: {itemData.itemName} at {transform.position}");
    }
    
    /// <summary>
    /// Update visual appearance based on item data
    /// </summary>
    private void UpdateVisuals()
    {
        if (itemData == null || spriteRenderer == null) return;
        
        // Set sprite from item data
        if (itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
        else
        {
            Debug.LogWarning($"[ItemDrop] Item {itemData.itemName} has no icon!");
        }
        
        // Set color based on rarity
        Color rarityColor = GetRarityColor(itemData.rarity);
        
        if (enableGlow)
        {
            rarityColor *= glowIntensity;
        }
        
        spriteRenderer.color = rarityColor;
        originalColor = rarityColor;
        
        // Set sorting layer
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 10; // Above bricks but below UI
        
        // Set name for debugging
        gameObject.name = $"ItemDrop_{itemData.itemName}_{GetInstanceID()}";
    }
    
    /// <summary>
    /// Get color based on item rarity
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f); // Light gray
            
            case ItemRarity.Uncommon:
                return new Color(0.3f, 1f, 0.3f); // Green
            
            case ItemRarity.Rare:
                return new Color(0.3f, 0.5f, 1f); // Blue
            
            case ItemRarity.Legendary:
                return new Color(1f, 0.6f, 0f); // Orange/Gold
            
            default:
                return Color.white;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // MOVEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private void UpdateMovement()
    {
        // Fall down at constant speed
        Vector3 velocity = Vector3.down * fallSpeed;
        
        // Apply movement
        transform.position += velocity * Time.deltaTime;
    }
    
    /// <summary>
    /// Set fall speed
    /// </summary>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = Mathf.Max(0, speed);
    }
    
    /// <summary>
    /// Stop falling (useful for special effects)
    /// </summary>
    public void StopFalling()
    {
        fallSpeed = 0f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // VISUAL EFFECTS
    // ═══════════════════════════════════════════════════════════════
    
    private void UpdatePulseEffect()
    {
        pulseTime += Time.deltaTime * pulseSpeed;
        
        float pulse = 1f + Mathf.Sin(pulseTime) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
    
    private void UpdateRotation()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
    
    private void UpdateFlashEffect(float remainingTime)
    {
        // Flash faster as time runs out
        float flashRate = flashSpeed * (1f + (flashStartTime - remainingTime));
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashRate));
        
        Color flashColor = originalColor;
        flashColor.a = Mathf.Lerp(0.3f, 1f, alpha);
        spriteRenderer.color = flashColor;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // COLLECTION
    // ═══════════════════════════════════════════════════════════════
    
    private void Collect()
    {
      if (!isInitialized || isCollected || itemData == null) return;
    
      isCollected = true;
    
     Debug.Log($"[ItemDrop] Attempting to collect: {itemData.itemName}");
    
    // Try to add to player inventory
    if (PlayerInventory.Instance != null)
    {
        // Use itemType from ItemData
        bool added = PlayerInventory.Instance.AddItem(itemData.itemType, 1);
        
        if (added)
        {
            // Successfully added
            OnCollectSuccess();
        }
        else
        {
            // Inventory full
            OnCollectFailed("Inventory full!");
            isCollected = false; // Allow collection again
        }
    }
    else
    {
        Debug.LogError("[ItemDrop] PlayerInventory.Instance is null!");
        OnCollectFailed("No inventory system found");
    }
}
    
    private void OnCollectSuccess()
    {
        // Fire events
        GameEvents.ItemPickedUp(itemData);
        GameEvents.PlaySound("ItemPickup", transform.position);
        
        // Show message
        string rarityText = GetRarityText(itemData.rarity);
        GameEvents.ShowMessage($"<color={GetRarityHexColor(itemData.rarity)}>{rarityText}</color> {itemData.itemName}", 2f);
        
        Debug.Log($"[ItemDrop] Collected: {itemData.itemName}");
        
        // Visual feedback
        StartCoroutine(CollectAnimationRoutine());
    }
    
    private void OnCollectFailed(string reason)
    {
        GameEvents.ShowMessage(reason, 1.5f);
        GameEvents.PlaySound("ItemCollectFail", transform.position);
        
        Debug.LogWarning($"[ItemDrop] Collection failed: {reason}");
    }
    
    private IEnumerator CollectAnimationRoutine()
    {
        // Disable collider
        col.enabled = false;
        
        // Animate upward and shrink
        float animTime = 0.3f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * 2f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < animTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animTime;
            
            // Move up
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // Shrink
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            // Fade out
            Color color = spriteRenderer.color;
            color.a = 1f - t;
            spriteRenderer.color = color;
            
            yield return null;
        }
        
        // Despawn
        Despawn();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DESPAWN
    // ═══════════════════════════════════════════════════════════════
    
    private void Despawn()
    {
        if (!isInitialized) return;
        
        Debug.Log($"[ItemDrop] Despawning: {itemData?.itemName ?? "Unknown"}");
        
        isInitialized = false;
        isCollected = false;
        itemData = null;
        
        // Re-enable collider for next use
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Reset visual state
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        
        // Return to pool via ItemDropper
        if (ItemDropper.Instance != null)
        {
            ItemDropper.Instance.RemoveDrop(gameObject);
        }
        else
        {
            // Fallback: just deactivate
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Force despawn (called externally)
    /// </summary>
    public void ForceDespawn()
    {
        StopAllCoroutines();
        Despawn();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // RARITY TEXT HELPERS
    // ═══════════════════════════════════════════════════════════════
    
    private string GetRarityText(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "[COMMON]";
            case ItemRarity.Uncommon:
                return "[UNCOMMON]";
            case ItemRarity.Rare:
                return "[RARE]";
            case ItemRarity.Legendary:
                return "[LEGENDARY]";
            default:
                return "";
        }
    }
    
    private string GetRarityHexColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "#CCCCCC"; // Light gray
            case ItemRarity.Uncommon:
                return "#4CFF4C"; // Green
            case ItemRarity.Rare:
                return "#4C7FFF"; // Blue
            case ItemRarity.Legendary:
                return "#FF9933"; // Orange
            default:
                return "#FFFFFF";
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PUBLIC UTILITIES
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Extend lifetime by given amount
    /// </summary>
    public void ExtendLifetime(float additionalTime)
    {
        lifetime += additionalTime;
        Debug.Log($"[ItemDrop] Lifetime extended by {additionalTime}s");
    }
    
    /// <summary>
    /// Attract to position (useful for magnet power-up)
    /// </summary>
    public void AttractTo(Vector3 targetPosition, float attractSpeed)
    {
        StartCoroutine(AttractRoutine(targetPosition, attractSpeed));
    }
    
    private IEnumerator AttractRoutine(Vector3 targetPosition, float attractSpeed)
    {
        while (isInitialized && !isCollected)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * attractSpeed * Time.deltaTime;
            
            // Check if close enough
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                Collect();
                yield break;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Enable/disable visual effects
    /// </summary>
    public void SetVisualEffects(bool enablePulse, bool enableRot, bool enableGlowEffect)
    {
        enablePulseEffect = enablePulse;
        enableRotation = enableRot;
        enableGlow = enableGlowEffect;
        
        if (!enablePulse)
        {
            transform.localScale = originalScale;
        }
        
        if (!enableRot)
        {
            transform.rotation = Quaternion.identity;
        }
        
        if (enableGlow != enableGlowEffect)
        {
            UpdateVisuals();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnDrawGizmos()
    {
        if (!isInitialized) return;
        
        // Draw lifetime indicator
        Gizmos.color = Color.yellow;
        float lifetimePercent = RemainingLifetime / lifetime;
        Gizmos.DrawWireSphere(transform.position, 0.5f * lifetimePercent);
        
        // Draw collection radius
        if (col != null)
        {
            Gizmos.color = Color.green;
            if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position, circleCol.radius);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!isInitialized) return;
        
        // Draw info
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1f,
            $"{itemData?.itemName ?? "Unknown"}\nLifetime: {RemainingLifetime:F1}s"
        );
        #endif
    }
}