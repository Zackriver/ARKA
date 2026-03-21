using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public float startSpeed = 12f;
    public float maxSpeed = 25f;
    public GameManager manager;
    public Transform paddle;

    [Header("Bounce Settings")]
    [Range(30f, 75f)]
    [Tooltip("Minimum angle from horizontal (higher = more vertical)")]
    public float minBounceAngle = 45f;
    
    [Range(75f, 90f)]
    [Tooltip("Maximum angle from horizontal (90 = straight up)")]
    public float maxBounceAngle = 85f;

    private float currentSpeed;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool isMoving = false;
    private Vector3 offset = new Vector3(0, 0.75f, 0);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        
        if (col.isTrigger)
        {
            col.isTrigger = false;
        }
    }

    void Start()
    {
        if (manager == null) manager = FindFirstObjectByType<GameManager>();
        ResetBall();
    }

    void Update()
    {
        if (!isMoving)
        {
            if (paddle != null) transform.position = paddle.position + offset;

            if (Keyboard.current?.spaceKey.wasPressedThisFrame == true || 
                Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            {
                Launch();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            ClampVelocityAngle();
        }
    }

    private void ClampVelocityAngle()
    {
        Vector2 vel = rb.linearVelocity;
        if (vel.magnitude < 0.1f) return;

        float currentAngle = Mathf.Abs(Mathf.Atan2(vel.y, Mathf.Abs(vel.x)) * Mathf.Rad2Deg);

        if (currentAngle < minBounceAngle)
        {
            float sign = Mathf.Sign(vel.x);
            float ySign = Mathf.Sign(vel.y);
            if (ySign == 0) ySign = 1f;
            
            float correctedAngle = minBounceAngle * Mathf.Deg2Rad;
            float newX = Mathf.Cos(correctedAngle) * sign;
            float newY = Mathf.Sin(correctedAngle) * ySign;
            
            rb.linearVelocity = new Vector2(newX, newY).normalized * currentSpeed;
        }
    }

    public void Launch()
    {
        isMoving = true;
        currentSpeed = startSpeed;
        
        if (col != null) col.enabled = true;
        
        float randomAngle = Random.Range(-30f, 30f);
        float angleRad = (90f + randomAngle) * Mathf.Deg2Rad;
        
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        rb.linearVelocity = direction.normalized * currentSpeed;
    }

    public void ResetBall()
    {
        isMoving = false;
        currentSpeed = startSpeed;
        rb.linearVelocity = Vector2.zero;
        
        if (col != null) col.enabled = false;
        
        if (paddle != null)
        {
            transform.position = paddle.position + offset;
        }
    }

    public void IncreaseSpeed(float amount)
    {
        currentSpeed = Mathf.Min(currentSpeed + (startSpeed * amount), maxSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMoving) return;

        if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleBounce(collision);
            if (manager != null) manager.PlaySound(manager.toughSound);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            if (manager != null) manager.PlaySound(manager.toughSound);
        }
    }

    private void HandlePaddleBounce(Collision2D collision)
    {
        Transform paddleTransform = collision.transform;
        float paddleWidth = 1f;
        
        BoxCollider2D paddleCollider = collision.collider as BoxCollider2D;
        if (paddleCollider != null)
        {
            paddleWidth = paddleCollider.size.x * paddleTransform.localScale.x;
        }
        else
        {
            SpriteRenderer sr = collision.gameObject.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                paddleWidth = sr.sprite.bounds.size.x * paddleTransform.localScale.x;
            }
        }

        float hitPoint = collision.contacts[0].point.x;
        float paddleCenter = paddleTransform.position.x;
        float hitOffset = (hitPoint - paddleCenter) / (paddleWidth / 2f);
        hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);

        float bounceAngle = Mathf.Lerp(maxBounceAngle, minBounceAngle, Mathf.Abs(hitOffset));
        float angleRad = bounceAngle * Mathf.Deg2Rad;
        
        float xDirection = Mathf.Cos(angleRad) * Mathf.Sign(hitOffset);
        
        if (Mathf.Abs(hitOffset) < 0.1f)
        {
            xDirection = Random.Range(-0.15f, 0.15f);
        }
        
        float yDirection = Mathf.Sin(angleRad);

        Vector2 newDirection = new Vector2(xDirection, yDirection).normalized;
        rb.linearVelocity = newDirection * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            if (manager != null) manager.LiveLost();
        }
    }
}