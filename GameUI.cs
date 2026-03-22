using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour 
{
    public GameManager manager;
    public Text scoreText;
    public Text livesText;
    public GameObject gameOverScreen;
    public Text gameOverScoreText;
    public GameObject winScreen;
    public GameObject pauseScreen;

    [Header("Mobile Settings")]
    public bool autoConfigureCanvas = true;
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    [Range(0f, 1f)]
    public float screenMatchMode = 0.5f;
    
    [Header("Font Sizes")]
    public int scoreFontSize = 48;
    public int livesFontSize = 48;
    public int gameOverFontSize = 64;

    private bool isPaused = false;

    void Awake()
    {
        if (autoConfigureCanvas)
        {
            ConfigureCanvasForMobile();
        }
        
        SetupFontSizes();
    }

    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<GameManager>();
        }
        
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }

    void ConfigureCanvasForMobile()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }
        
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = screenMatchMode;
            scaler.referencePixelsPerUnit = 100;
        }
    }

    void SetupFontSizes()
    {
        if (scoreText != null)
        {
            scoreText.fontSize = scoreFontSize;
            scoreText.resizeTextForBestFit = true;
            scoreText.resizeTextMinSize = 20;
            scoreText.resizeTextMaxSize = scoreFontSize;
        }
        
        if (livesText != null)
        {
            livesText.fontSize = livesFontSize;
            livesText.resizeTextForBestFit = true;
            livesText.resizeTextMinSize = 20;
            livesText.resizeTextMaxSize = livesFontSize;
        }
        
        if (gameOverScoreText != null)
        {
            gameOverScoreText.fontSize = gameOverFontSize;
            gameOverScoreText.resizeTextForBestFit = true;
            gameOverScoreText.resizeTextMinSize = 24;
            gameOverScoreText.resizeTextMaxSize = gameOverFontSize;
        }
    }

    void Update()
    {
        if (manager == null) return;
        
        if (!manager.gameOver && !manager.wonGame)
        {
            if (scoreText != null)
            {
                scoreText.text = "<b>SCORE</b>\n" + manager.score;
            }
            
            if (livesText != null)
            {
                livesText.text = "<b>LIVES</b>: " + manager.lives;
            }
        }
        else
        {
            if (scoreText != null) scoreText.text = "";
            if (livesText != null) livesText.text = "";
        }
    }

    // ==================== PAUSE SYSTEM ====================
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(true);
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }
    
    // ==================== BUTTONS ====================
    
    public void EndGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    public void PauseButton()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // ==================== GAME STATE ====================

    public void SetGameOver()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
        
        if (gameOverScoreText != null && manager != null)
        {
            gameOverScoreText.text = "<b>YOU ACHIEVED A SCORE OF</b>\n" + manager.score;
        }
    }

    public void SetWin()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }

    public void TryAgainButton()
    {
        Time.timeScale = 1f;
        
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
        
        if (manager != null)
        {
            manager.StartGame();
        }
    }

    public void MenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}