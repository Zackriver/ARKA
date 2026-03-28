using UnityEngine;

/// <summary>
/// Brick that can be destroyed by the ball
/// Place in: Assets/Scripts/Gameplay/Brick.cs
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
    [Header("Brick Settings")]
    [SerializeField] private int resistance = Constants.Brick.DEFAULT_RESISTANCE;
    [SerializeField] private int scoreValue = Constants.Brick.DEFAULT_SCORE_VALUE;
    [SerializeField] private bool isIndestructible = false;
    
    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;
    [SerializeField] private Color indestructibleColor = Color.gray;
    
    private SpriteRenderer spriteRenderer;
    private int currentResistance;
    
    // Public properties
    public bool IsIndestructible => isIndestructible || resistance == Constants.Brick.INDESTRUCTIBLE_RESISTANCE;
    public int CurrentResistance => currentResistance;
    public int ScoreValue => scoreValue;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Initialize resistance
        if (isIndestructible)
        {
            currentResistance = Constants.Brick.INDESTRUCTIBLE_RESISTANCE;
        }
        else
        {
            currentResistance = resistance;
        }
        
        UpdateVisuals();
    }
    
    public void Hit()
    {
        if (IsIndestructible)
        {
            // Indestructible brick - just play sound
            GameEvents.PlaySound("BrickHit", transform.position);
            return;
        }
        
        currentResistance--;
        
        if (currentResistance <= 0)
        {
            DestroyBrick();
        }
        else
        {
            // Damaged but not destroyed
            UpdateVisuals();
            GameEvents.BrickHit(this, currentResistance);
            GameEvents.PlaySound("BrickHit", transform.position);
        }
    }
    
    private void DestroyBrick()
    {
        // Fire event BEFORE destroying
        GameEvents.BrickDestroyed(this, scoreValue);
        GameEvents.PlaySound("BrickBreak", transform.position);
        
        // Chance to drop power-up
        if (Random.value <= Constants.Brick.DROP_CHANCE)
        {
            // This will be handled by ItemDropper if it exists
            GameEvents.ItemDropped(null, transform.position);
        }
        
        Destroy(gameObject);
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        
        if (IsIndestructible)
        {
            spriteRenderer.color = indestructibleColor;
        }
        else
        {
            // Interpolate color based on damage
            float damagePercent = 1f - ((float)currentResistance / resistance);
            spriteRenderer.color = Color.Lerp(normalColor, damagedColor, damagePercent);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.Tags.BALL))
        {
            Hit();
        }
    }
}