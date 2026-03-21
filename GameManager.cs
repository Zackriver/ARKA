using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int score;
    public int lives = 10;
    public bool gameOver;
    public bool wonGame;

    [Header("Brick Settings")]
    [Tooltip("Horizontal gap between bricks (columns)")]
    public float brickGapX = 0.1f;
    [Tooltip("Vertical gap between bricks (rows)")]
    public float brickGapY = 0.1f;

    [Header("Brick Frame")]
    public bool showBrickFrame = true;
    [Range(0.02f, 0.3f)]
    public float frameThickness = 0.1f;
    public Color frameColor = new Color(0.3f, 0.3f, 0.35f, 1f);
    [Tooltip("Add metallic highlight effect")]
    public bool metallicEffect = true;
    [Range(0f, 1f)]
    public float highlightStrength = 0.4f;
    [Range(0f, 1f)]
    public float shadowStrength = 0.5f;

    [Header("Brick Shapes")]
    public List<BrickShape> availableShapes = new List<BrickShape>();

    [Header("Level Playlist")]
    public List<LevelData> allLevels = new List<LevelData>();
    public int currentLevelIndex = 0;
    public LevelData currentLevelData;

    [Header("References")]
    public GameObject paddle;
    public GameObject ball;
    public GameUI gameUI;
    public GameObject brickPrefab;
    public List<GameObject> activeBricks = new List<GameObject>();

    [Header("Placement Bounds")]
    public Vector2 boundsCenter = new Vector2(0, 2f);
    public Vector2 boundsSize = new Vector2(14f, 6f);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip toughSound;
    public AudioClip breakSound;

    // Cached white sprite for frame elements
    private Sprite _whiteSprite;
    private Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                Texture2D tex = new Texture2D(8, 8);
                Color[] colors = new Color[64];
                for (int i = 0; i < 64; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            }
            return _whiteSprite;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        toughSound = ProceduralSoundGenerator.PaddleHit();
        breakSound = ProceduralSoundGenerator.BrickDestroy();
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            PlaySound(ProceduralSoundGenerator.GameStart());
            StartGame();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public GameObject CreateBrick(BrickData data, bool isPreview)
    {
        if (brickPrefab == null) return null;

        GameObject newBrick = Instantiate(brickPrefab, (Vector3)data.position, Quaternion.identity);
        newBrick.transform.localScale = Vector3.one;

        SpriteRenderer sr = newBrick.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = newBrick.AddComponent<SpriteRenderer>();
        }

        if (data.shape != null && data.shape.sprite != null)
        {
            sr.sprite = data.shape.sprite;
        }

        Color finalColor = data.color;
        finalColor.a = 1f;
        sr.color = finalColor;
        sr.sortingOrder = 10;

        if (showBrickFrame)
        {
            AddFrame(newBrick, sr);
        }

        if (isPreview)
        {
            newBrick.hideFlags = HideFlags.DontSave;
        }
        else
        {
            Brick bScript = newBrick.GetComponent<Brick>();
            if (bScript == null)
            {
                bScript = newBrick.AddComponent<Brick>();
            }
            bScript.Init(data.health, finalColor, this);
        }

        return newBrick;
    }

    private void AddFrame(GameObject brick, SpriteRenderer brickSR)
    {
        if (brickSR == null || brickSR.sprite == null) return;

        Vector2 brickSize = brickSR.sprite.bounds.size;
        float halfW = brickSize.x / 2f;
        float halfH = brickSize.y / 2f;
        float t = frameThickness;

        // Create frame parent
        GameObject frameParent = new GameObject("Frame");
        frameParent.transform.SetParent(brick.transform, false);
        frameParent.transform.localPosition = Vector3.zero;

        // === OUTER FRAME (4 sides) ===
        
        // Top frame bar
        CreateFrameBar(frameParent, "TopFrame",
            new Vector3(0, halfH + t / 2f, 0.01f),
            new Vector3(brickSize.x + t * 2f, t, 1f),
            frameColor, 5);

        // Bottom frame bar
        CreateFrameBar(frameParent, "BottomFrame",
            new Vector3(0, -halfH - t / 2f, 0.01f),
            new Vector3(brickSize.x + t * 2f, t, 1f),
            frameColor, 5);

        // Left frame bar
        CreateFrameBar(frameParent, "LeftFrame",
            new Vector3(-halfW - t / 2f, 0, 0.01f),
            new Vector3(t, brickSize.y, 1f),
            frameColor, 5);

        // Right frame bar
        CreateFrameBar(frameParent, "RightFrame",
            new Vector3(halfW + t / 2f, 0, 0.01f),
            new Vector3(t, brickSize.y, 1f),
            frameColor, 5);

        // === METALLIC EFFECT ===
        if (metallicEffect)
        {
            // Top highlight (light reflection)
            Color highlightColor = new Color(1f, 1f, 1f, highlightStrength);
            CreateFrameBar(frameParent, "TopHighlight",
                new Vector3(0, halfH + t * 0.75f, 0.005f),
                new Vector3(brickSize.x + t, t * 0.4f, 1f),
                highlightColor, 6);

            // Left highlight
            CreateFrameBar(frameParent, "LeftHighlight",
                new Vector3(-halfW - t * 0.75f, 0, 0.005f),
                new Vector3(t * 0.4f, brickSize.y - t * 0.5f, 1f),
                highlightColor, 6);

            // Bottom shadow
            Color shadowColor = new Color(0f, 0f, 0f, shadowStrength);
            CreateFrameBar(frameParent, "BottomShadow",
                new Vector3(0, -halfH - t * 0.75f, 0.005f),
                new Vector3(brickSize.x + t, t * 0.4f, 1f),
                shadowColor, 6);

            // Right shadow
            CreateFrameBar(frameParent, "RightShadow",
                new Vector3(halfW + t * 0.75f, 0, 0.005f),
                new Vector3(t * 0.4f, brickSize.y - t * 0.5f, 1f),
                shadowColor, 6);

            // Inner bevel highlight (top-left inner edge)
            Color innerHighlight = new Color(1f, 1f, 1f, highlightStrength * 0.5f);
            CreateFrameBar(frameParent, "InnerTopHighlight",
                new Vector3(0, halfH - t * 0.15f, -0.001f),
                new Vector3(brickSize.x - t * 0.3f, t * 0.2f, 1f),
                innerHighlight, 11);

            CreateFrameBar(frameParent, "InnerLeftHighlight",
                new Vector3(-halfW + t * 0.15f, 0, -0.001f),
                new Vector3(t * 0.2f, brickSize.y - t * 0.6f, 1f),
                innerHighlight, 11);

            // Inner bevel shadow (bottom-right inner edge)
            Color innerShadow = new Color(0f, 0f, 0f, shadowStrength * 0.3f);
            CreateFrameBar(frameParent, "InnerBottomShadow",
                new Vector3(0, -halfH + t * 0.15f, -0.001f),
                new Vector3(brickSize.x - t * 0.3f, t * 0.2f, 1f),
                innerShadow, 11);

            CreateFrameBar(frameParent, "InnerRightShadow",
                new Vector3(halfW - t * 0.15f, 0, -0.001f),
                new Vector3(t * 0.2f, brickSize.y - t * 0.6f, 1f),
                innerShadow, 11);
        }
    }

    private void CreateFrameBar(GameObject parent, string name, Vector3 localPos, Vector3 scale, Color color, int sortOrder)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent.transform, false);
        bar.transform.localPosition = localPos;
        bar.transform.localScale = scale;

        SpriteRenderer sr = bar.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;
    }

    public void StartGame()
    {
        score = 0; 
        lives = 10; 
        gameOver = false; 
        wonGame = false;
        
        if (paddle != null) paddle.SetActive(true);
        if (ball != null) ball.SetActive(true);
        
        currentLevelIndex = 0;
        LoadCurrentLevelFromList();
    }

    public void LoadCurrentLevelFromList()
    {
        if (allLevels.Count > 0 && currentLevelIndex < allLevels.Count)
        {
            currentLevelData = allLevels[currentLevelIndex];
            LoadLevel(currentLevelData);
            ResetBallAndPaddle();
        }
        else if (allLevels.Count > 0) 
        {
            WinGame();
        }
    }

    public void LoadLevel(LevelData data)
    {
        foreach (GameObject b in activeBricks) 
        {
            if (b != null) Destroy(b);
        }
        activeBricks.Clear();
        
        if (data == null) return;
        
        foreach (BrickData brick in data.bricks)
        {
            GameObject newBrick = CreateBrick(brick, false);
            if (newBrick != null) activeBricks.Add(newBrick);
        }
    }

    public void RemoveBrick(GameObject brick)
    {
        activeBricks.Remove(brick);
        score += 10;
        
        if (ball != null && ball.TryGetComponent(out Ball ballScript))
            ballScript.IncreaseSpeed(0.05f);

        if (activeBricks.Count <= 0)
        {
            currentLevelIndex++;
            if (currentLevelIndex < allLevels.Count) 
            {
                LoadCurrentLevelFromList();
            }
            else 
            {
                WinGame();
            }
        }
    }

    public void LiveLost()
    {
        lives--;
        PlaySound(ProceduralSoundGenerator.BallLost());
        
        if (lives < 1)
        {
            gameOver = true;
            PlaySound(ProceduralSoundGenerator.GameOver());
            if (paddle != null) paddle.SetActive(false);
            if (ball != null) ball.SetActive(false);
            if (gameUI != null) gameUI.SetGameOver();
        }
        else 
        {
            ResetBallAndPaddle();
        }
    }

    private void ResetBallAndPaddle()
    {
        if (paddle != null)
        {
            Paddle paddleScript = paddle.GetComponent<Paddle>();
            if (paddleScript != null)
            {
                paddleScript.ResetPaddle();
            }
            else
            {
                paddle.transform.position = new Vector3(0, paddle.transform.position.y, 0);
            }
        }

        if (ball != null)
        {
            ball.SetActive(true);
            
            Ball ballScript = ball.GetComponent<Ball>();
            if (ballScript != null)
            {
                ballScript.ResetBall();
            }
        }
    }

    public void WinGame()
    {
        wonGame = true;
        PlaySound(ProceduralSoundGenerator.LevelComplete());
        if (paddle != null) paddle.SetActive(false);
        if (ball != null) ball.SetActive(false);
        if (gameUI != null) gameUI.SetWin();
    }

    public Vector2 GetBrickWorldSize(BrickShape shape)
    {
        if (shape != null && shape.sprite != null) return shape.sprite.bounds.size;
        if (brickPrefab != null)
        {
            SpriteRenderer sr = brickPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite.bounds.size;
        }
        return new Vector2(0.9f, 0.4f);
    }

    public Vector2 GetStepSize(BrickShape shape)
    {
        Vector2 size = GetBrickWorldSize(shape);
        return new Vector2(size.x + brickGapX, size.y + brickGapY);
    }

    public bool IsInBounds(Vector2 pos)
    {
        float halfW = boundsSize.x / 2f;
        float halfH = boundsSize.y / 2f;
        return pos.x >= boundsCenter.x - halfW && pos.x <= boundsCenter.x + halfW &&
               pos.y >= boundsCenter.y - halfH && pos.y <= boundsCenter.y + halfH;
    }

    public void RefreshPreview()
    {
        if (Application.isPlaying) return;
        for (int i = activeBricks.Count - 1; i >= 0; i--)
            if (activeBricks[i] != null) DestroyImmediate(activeBricks[i]);
        activeBricks.Clear();
        if (currentLevelData == null) return;
        foreach (BrickData brick in currentLevelData.bricks)
        {
            GameObject newBrick = CreateBrick(brick, true);
            if (newBrick != null) activeBricks.Add(newBrick);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube((Vector3)boundsCenter, (Vector3)boundsSize);
    }
}