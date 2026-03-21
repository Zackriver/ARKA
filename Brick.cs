using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
    public int health = 1;
    public GameManager manager;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        
        if (col.isTrigger)
        {
            col.isTrigger = false;
        }
    }

    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<GameManager>();
        }
    }

    public void Init(int hp, Color color, GameManager mgr)
    {
        health = hp;
        manager = mgr;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<GameManager>();
        }

        if (collision.gameObject.CompareTag("Ball"))
        {
            health--;

            if (manager != null)
            {
                if (health <= 0)
                {
                    manager.PlaySound(manager.breakSound);
                    manager.RemoveBrick(this.gameObject);
                    
                    // === NEW: Try to drop items ===
                    if (ItemDropper.Instance != null)
                    {
                        ItemDropper.Instance.OnBrickDestroyed(transform.position);
                    
                    }
                       Destroy(this.gameObject);
                    
                }
                else
                {
                    manager.PlaySound(manager.toughSound);
                    DarkenOnHit();
                }
            }
            else
            {
                if (health <= 0)
                {
                    // === NEW: Try to drop item ===
                    if (ItemDropper.Instance != null)
                    {
                        ItemDropper.Instance.OnBrickDestroyed(transform.position);
                    }
                    
                    Destroy(this.gameObject);
                }
            }
        }
    }

    void DarkenOnHit()
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.r *= 0.8f; 
            c.g *= 0.8f; 
            c.b *= 0.8f;
            spriteRenderer.color = c;
        }
    }
}