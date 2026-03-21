using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ItemDrop : MonoBehaviour
{
    [Header("Item Info")]
    public ItemData itemData;
    public bool isDebuff = false;
    public DebuffData debuffData;
    
    [Header("Movement")]
    public float fallSpeed = 3f;
    public float wobbleAmount = 0.5f;
    public float wobbleSpeed = 3f;
    
    [Header("Visual")]
    public SpriteRenderer iconRenderer;
    public SpriteRenderer glowRenderer;
    public ParticleSystem collectParticle;
    
    private Rigidbody2D rb;
    private float wobbleOffset;
    private float startX;
    private bool collected = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Auto-find renderers if not assigned
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (glowRenderer == null)
        {
            Transform glowTransform = transform.Find("Glow");
            if (glowTransform != null)
            {
                glowRenderer = glowTransform.GetComponent<SpriteRenderer>();
            }
        }
    }
    
    private void Start()
    {
        startX = transform.position.x;
        SetupVisuals();
    }
    
    private void Update()
    {
        if (collected) return;
        
        // Fall down
        float newY = transform.position.y - (fallSpeed * Time.deltaTime);
        
        // Wobble side to side
        float wobble = Mathf.Sin((Time.time + wobbleOffset) * wobbleSpeed) * wobbleAmount;
        float newX = startX + wobble;
        
        transform.position = new Vector3(newX, newY, transform.position.z);
        
        // Destroy if falls below screen
        if (transform.position.y < -15f)
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupVisuals()
    {
        if (isDebuff && debuffData != null)
        {
            SetupDebuffVisuals();
        }
        else if (itemData != null)
        {
            SetupItemVisuals();
        }
        else
        {
            // Fallback - white circle
            SetupFallbackVisuals();
        }
    }
    
    private void SetupItemVisuals()
    {
        if (iconRenderer != null)
        {
            // Use assigned icon or generate one
            if (itemData.icon != null)
            {
                iconRenderer.sprite = itemData.icon;
            }
            else
            {
                iconRenderer.sprite = SpriteGenerator.CreateCircleSprite(32, Color.white);
            }
            
            iconRenderer.color = itemData.itemColor;
        }
        
        if (glowRenderer != null)
        {
            Color rarityColor = itemData.GetRarityColor();
            
            // Generate glow sprite if needed
            if (glowRenderer.sprite == null)
            {
                glowRenderer.sprite = SpriteGenerator.CreateGlowSprite(64, Color.white);
            }
            
            glowRenderer.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.5f);
        }
    }
    
    private void SetupDebuffVisuals()
    {
        if (iconRenderer != null)
        {
            if (debuffData.icon != null)
            {
                iconRenderer.sprite = debuffData.icon;
            }
            else
            {
                // Diamond shape for debuffs
                iconRenderer.sprite = SpriteGenerator.CreateDiamondSprite(32, Color.white);
            }
            
            iconRenderer.color = debuffData.debuffColor;
        }
        
        if (glowRenderer != null)
        {
            if (glowRenderer.sprite == null)
            {
                glowRenderer.sprite = SpriteGenerator.CreateGlowSprite(64, Color.white);
            }
            
            Color debuffGlow = debuffData.debuffColor;
            glowRenderer.color = new Color(debuffGlow.r, debuffGlow.g, debuffGlow.b, 0.6f);
        }
    }
    
    private void SetupFallbackVisuals()
    {
        if (iconRenderer != null)
        {
            iconRenderer.sprite = SpriteGenerator.CreateCircleSprite(32, Color.white);
            iconRenderer.color = Color.white;
        }
        
        if (glowRenderer != null)
        {
            glowRenderer.sprite = SpriteGenerator.CreateGlowSprite(64, Color.white);
            glowRenderer.color = new Color(1f, 1f, 1f, 0.3f);
        }
    }
    
    public void Initialize(ItemData item)
    {
        itemData = item;
        isDebuff = false;
        debuffData = null;
        SetupVisuals();
    }
    
    public void InitializeAsDebuff(DebuffData debuff)
    {
        debuffData = debuff;
        isDebuff = true;
        itemData = null;
        SetupVisuals();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (other.CompareTag("Paddle"))
        {
            Collect();
        }
        else if (other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
        }
    }
    
    private void Collect()
    {
        collected = true;
        
        if (isDebuff && debuffData != null)
        {
            PowerUpManager.Instance?.ApplyDebuff(debuffData);
            PlayCollectSound(false);
        }
        else if (itemData != null)
        {
            PlayerInventory.Instance?.AddItem(itemData.itemType, 1);
            PlayCollectSound(true);
        }
        
        // Play particle effect
        if (collectParticle != null)
        {
            collectParticle.transform.SetParent(null);
            collectParticle.Play();
            Destroy(collectParticle.gameObject, 2f);
        }
        
        // Quick scale down animation before destroy
        StartCoroutine(CollectAnimation());
    }
    
    private System.Collections.IEnumerator CollectAnimation()
    {
        float duration = 0.1f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    private void PlayCollectSound(bool isItem)
    {
        // Play sound through GameManager or AudioManager
        if (GameManager.Instance != null)
        {
            if (isItem)
            {
                // Item collect sound
                GameManager.Instance.PlaySound(GameManager.Instance.toughSound);
            }
            else
            {
                // Debuff sound (maybe different?)
                GameManager.Instance.PlaySound(GameManager.Instance.breakSound);
            }
        }
    }
}