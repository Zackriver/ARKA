using UnityEngine;

/// <summary>
/// Player-controlled paddle with bounds checking and smooth movement
/// Place in: Assets/Scripts/Gameplay/Paddle.cs
/// 
/// REQUIRED COMPONENTS:
/// - BoxCollider2D
/// - SpriteRenderer (optional)
/// 
/// INSPECTOR SETUP:
/// 1. Set layer to "Paddle"
/// 2. Set tag to "Player"
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Paddle : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = Constants.Paddle.DEFAULT_SPEED;
    
    [Header("Boundaries")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private bool autoCalculateBounds = true;
    
    // ═══════════════════════════════════════════════════════════════
    // CACHED REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    private BoxCollider2D col;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    private Vector3 initialPosition;
    private float currentWidth = Constants.Paddle.DEFAULT_WIDTH;
    
    public float GetWidth() => currentWidth;
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        initialPosition = transform.position;
        
        if (autoCalculateBounds)
        {
            CalculateBounds();
        }
        
        UpdateWidth();
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // MOVEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleMovement()
    {
        if (!GameStateManager.Instance.CanPlay)
        {
            return;
        }
        
        float moveInput = Input.GetAxisRaw("Horizontal");
        
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            MovePaddle(moveInput);
        }
    }
    
    private void MovePaddle(float direction)
    {
        Vector3 pos = transform.position;
        pos.x += direction * moveSpeed * Time.deltaTime;
        
        // Clamp to boundaries
        float halfWidth = currentWidth / 2f;
        pos.x = Mathf.Clamp(pos.x, minX + halfWidth, maxX - halfWidth);
        
        transform.position = pos;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SIZE CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    public void IncreaseWidth()
    {
        SetWidth(currentWidth + Constants.Paddle.WIDTH_CHANGE_AMOUNT);
    }
    
    public void DecreaseWidth()
    {
        SetWidth(currentWidth - Constants.Paddle.WIDTH_CHANGE_AMOUNT);
    }
    
    public void ResetWidth()
    {
        SetWidth(Constants.Paddle.DEFAULT_WIDTH);
    }
    
    public void SetWidth(float width)
    {
        currentWidth = Mathf.Clamp(width, Constants.Paddle.MIN_WIDTH, Constants.Paddle.MAX_WIDTH);
        UpdateWidth();
    }
    
    private void UpdateWidth()
    {
        // Update collider
        if (col != null)
        {
            Vector2 size = col.size;
            size.x = currentWidth;
            col.size = size;
        }
        
        // Update sprite
        if (spriteRenderer != null)
        {
            Vector3 scale = transform.localScale;
            scale.x = currentWidth / Constants.Paddle.DEFAULT_WIDTH;
            transform.localScale = scale;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SPEED CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    public void SetSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0, speed);
    }
    
    public void ResetSpeed()
    {
        moveSpeed = Constants.Paddle.DEFAULT_SPEED;
    }
    
    public float GetSpeed() => moveSpeed;
    
    // ═══════════════════════════════════════════════════════════════
    // POSITION CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    public void ResetPosition()
    {
        transform.position = initialPosition;
    }
    
    public void SetPosition(Vector3 position)
    {
        // Clamp to boundaries
        float halfWidth = currentWidth / 2f;
        position.x = Mathf.Clamp(position.x, minX + halfWidth, maxX - halfWidth);
        transform.position = position;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BOUNDARY CALCULATION
    // ═══════════════════════════════════════════════════════════════
    
    private void CalculateBounds()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;
            
            minX = -camWidth + Constants.Paddle.BOUNDARY_PADDING;
            maxX = camWidth - Constants.Paddle.BOUNDARY_PADDING;
            
            Debug.Log($"[Paddle] Calculated bounds: {minX:F2} to {maxX:F2}");
        }
        else
        {
            Debug.LogWarning("[Paddle] No camera found for bounds calculation!");
        }
    }
    
    public void SetBounds(float min, float max)
    {
        minX = min;
        maxX = max;
        autoCalculateBounds = false;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw boundaries
        Gizmos.color = Color.yellow;
        float y = transform.position.y;
        Gizmos.DrawLine(new Vector3(minX, y - 1, 0), new Vector3(minX, y + 1, 0));
        Gizmos.DrawLine(new Vector3(maxX, y - 1, 0), new Vector3(maxX, y + 1, 0));
    }
}