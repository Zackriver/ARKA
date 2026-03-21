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

    [Header("Mobile Settings")]
    public bool autoConfigureCanvas = true;
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    [Range(0f, 1f)]
    public float screenMatchMode = 0.5f; // 0 = width, 1 = height, 0.5 = balanced
    
    [Header("Font Sizes")]
    public int scoreFontSize = 48;
    public int livesFontSize = 48;
    public int gameOverFontSize = 64;

    void Awake()
    {
        if (autoConfigureCanvas)
        {
            ConfigureCanvasForMobile();
        }
        
        SetupFontSizes();
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
            
            // Set to Scale With Screen Size
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
        if (!manager.gameOver && !manager.wonGame)
        {
            scoreText.text = "<b>SCORE</b>\n" + manager.score;
            livesText.text = "<b>LIVES</b>: " + manager.lives;
        }
        else
        {
            scoreText.text = "";
            livesText.text = "";
        }
    }

    public void SetGameOver()
    {
        gameOverScreen.SetActive(true);
        gameOverScoreText.text = "<b>YOU ACHIEVED A SCORE OF</b>\n" + manager.score;
    }

    public void SetWin()
    {
        winScreen.SetActive(true);
    }

    public void TryAgainButton()
    {
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);
        manager.StartGame();
    }

    public void MenuButton()
    {
        SceneManager.LoadScene(0);
    }
}