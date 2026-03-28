using UnityEngine;

/// <summary>
/// Ball controller with fixed physics and proper collision handling
/// Place in: Assets/Scripts/Gameplay/Ball.cs
/// 
/// REQUIRED COMPONENTS:
/// - Rigidbody2D (gravity scale = 0)
/// - CircleCollider2D
/// 
/// INSPECTOR SETUP:
/// 1. Set layer to "Ball"
/// 2. Assign paddle reference (optional - will auto-find)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    [SerializeField] private Paddle paddle;
    
    private Rigidbody2D rb;
    private CircleCollider2D col;
    
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    private bool isLaunched = false;
    private float currentSpeed = Constants.Ball.DEFAULT_SPEED;
    
    public bool IsLaunched => isLaunched;
    public float CurrentSpeed => currentSpeed;
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        
        // Configure rigidbody
        rb.gravityScale = Constants.Physics.GRAVITY_SCALE;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // Find paddle if not assigned
        if (paddle == null)
        {
            paddle = FindFirstObjectByType<Paddle>();
        }
    }
    
    private void Update()
    {
        if (!isLaunched && paddle != null)
        {
            // Stick to paddle
            FollowPaddle();
            
            // Check for launch input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (isLaunched)
        {
            EnforceMinimumYVelocity();
            EnforceSpeed();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;
        
        HandleCollision(collision);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Death zone detection
        if (other.CompareTag(Constants.Tags.DEATH_ZONE))
        {
            OnBallLost();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LAUNCH CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    private void FollowPaddle()
    {
        if (paddle == null) return;
        
        Vector3 paddlePos = paddle.transform.position;
        Vector3 newPos = paddlePos;
        newPos.y += 0.5f; // Offset above paddle
        transform.position = newPos;
    }
    
    public void Launch()
    {
        Launch(Vector2.up);
    }
    
    public void Launch(Vector2 direction)
    {
        if (isLaunched)
        {
            Debug.LogWarning("[Ball] Already launched!");
            return;
        }
        
        isLaunched = true;
        
        // Apply velocity
        Vector2 velocity = direction.normalized * currentSpeed;
        rb.linearVelocity = velocity;
        
        Debug.Log($"[Ball] Launched with velocity {rb.linearVelocity}");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PHYSICS ENFORCEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private void EnforceMinimumYVelocity()
    {
        Vector2 vel = rb.linearVelocity;
        
        // Prevent horizontal-only movement
        if (Mathf.Abs(vel.y) < Constants.Ball.MIN_Y_VELOCITY)
        {
            float sign = vel.y >= 0 ? 1f : -1f;
            vel.y = Constants.Ball.MIN_Y_VELOCITY * sign;
            rb.linearVelocity = vel;
        }
    }
    
    private void EnforceSpeed()
    {
        // Maintain constant speed
        float currentMagnitude = rb.linearVelocity.magnitude;
        
        if (Mathf.Abs(currentMagnitude - currentSpeed) > 0.1f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }
    
    public void IncreaseSpeed()
    {
        currentSpeed += Constants.Ball.SPEED_INCREMENT;
        currentSpeed = Mathf.Clamp(currentSpeed, Constants.Ball.MIN_SPEED, Constants.Ball.MAX_SPEED);
        
        if (isLaunched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }
    
    public void DecreaseSpeed()
    {
        currentSpeed -= Constants.Ball.SPEED_INCREMENT;
        currentSpeed = Mathf.Clamp(currentSpeed, Constants.Ball.MIN_SPEED, Constants.Ball.MAX_SPEED);
        
        if (isLaunched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }
    
    public void ResetSpeed()
    {
        currentSpeed = Constants.Ball.DEFAULT_SPEED;
        
        if (isLaunched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // COLLISION HANDLING
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleCollision(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        
        // Paddle collision
        if (other.CompareTag(Constants.Tags.PLAYER))
        {
            Paddle hitPaddle = other.GetComponent<Paddle>();
            if (hitPaddle != null)
            {
                ReflectFromPaddle(hitPaddle, collision);
                GameEvents.BallHitPaddle(this, hitPaddle);
            }
        }
        // Brick collision
        else if (other.CompareTag(Constants.Tags.BRICK))
        {
            Brick brick = other.GetComponent<Brick>();
            if (brick != null)
            {
                ReflectBall(collision);
                GameEvents.BallHitBrick(this, brick);
            }
        }
        // Wall collision
        else if (other.CompareTag(Constants.Tags.WALL))
        {
            ReflectBall(collision);
            GameEvents.BallHitWall(this);
        }
        // Default reflection
        else
        {
            ReflectBall(collision);
        }
    }
    
    private void ReflectFromPaddle(Paddle paddle, Collision2D collision)
    {
        // Get hit position relative to paddle center
        float paddleWidth = paddle.GetWidth();
        float hitPoint = transform.position.x - paddle.transform.position.x;
        float normalizedHitPoint = hitPoint / (paddleWidth / 2f); // -1 to 1
        
        // Calculate reflection angle based on hit position
        float bounceAngle = normalizedHitPoint * 60f; // Max 60 degrees
        
        // Create new direction
        Vector2 newDirection = Quaternion.Euler(0, 0, bounceAngle) * Vector2.up;
        
        // Apply velocity
        rb.linearVelocity = newDirection * currentSpeed;
        
        Debug.Log($"[Ball] Paddle hit at {normalizedHitPoint:F2}, angle {bounceAngle:F1}°");
    }
    
    private void ReflectBall(Collision2D collision)
    {
        // Get contact normal
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        
        // Reflect velocity
        Vector2 currentVel = rb.linearVelocity;
        Vector2 reflectedVel = Vector2.Reflect(currentVel, normal);
        
        // Add randomness THEN normalize (FIX for physics issue)
        Vector2 randomOffset = new Vector2(
            Random.Range(-Constants.Ball.RANDOM_REFLECTION_RANGE, Constants.Ball.RANDOM_REFLECTION_RANGE),
            Random.Range(-Constants.Ball.RANDOM_REFLECTION_RANGE, Constants.Ball.RANDOM_REFLECTION_RANGE)
        );
        
        Vector2 finalDirection = (reflectedVel + randomOffset).normalized; // FIX: normalize AFTER adding random
        
        // Apply velocity
        rb.linearVelocity = finalDirection * currentSpeed;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BALL LOST
    // ═══════════════════════════════════════════════════════════════
    
    private void OnBallLost()
    {
        Debug.Log("[Ball] Ball lost!");
        GameEvents.BallLost(this);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PUBLIC UTILITIES
    // ═══════════════════════════════════════════════════════════════
    
    public void SetSpeed(float speed)
    {
        currentSpeed = Mathf.Clamp(speed, Constants.Ball.MIN_SPEED, Constants.Ball.MAX_SPEED);
        
        if (isLaunched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }
    
    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }
}