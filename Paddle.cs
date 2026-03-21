using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Paddle : MonoBehaviour
{
    public float speed = 15f;
    public float minX = -8f;
    public float maxX = 8f;
    public float startY = -93f;
    public Sprite paddleSprite;

    private Vector3 targetPosition;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (spriteRenderer != null && spriteRenderer.sprite == null && paddleSprite != null)
            spriteRenderer.sprite = paddleSprite;
    }

    void Start()
    {
        mainCamera = Camera.main;
        transform.position = new Vector3(0, startY, transform.position.z);
        targetPosition = transform.position;
    }

    void Update()
    {
        float horizontalInput = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) horizontalInput = -1f;
            else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) horizontalInput = 1f;
        }

        if (horizontalInput == 0f)
        {
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.isPressed)
            {
                Vector2 screenPos = ts.primaryTouch.position.ReadValue();
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
                targetPosition.x = worldPos.x;
            }
        }
        else
        {
            targetPosition.x += horizontalInput * speed * Time.deltaTime;
        }

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        transform.position = new Vector3(targetPosition.x, startY, transform.position.z);
    }

    public void ResetPaddle()
    {
        targetPosition = new Vector3(0, startY, transform.position.z);
        transform.position = targetPosition;
    }
}