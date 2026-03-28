using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles game UI updates
/// Place in: Assets/Scripts/UI/GameUI.cs
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI messageText;
    
    private void OnEnable()
    {
        // Subscribe to events
        GameEvents.OnScoreChanged += HandleScoreChanged;
        GameEvents.OnLivesChanged += HandleLivesChanged;
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnShowMessage += HandleShowMessage;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameEvents.OnScoreChanged -= HandleScoreChanged;
        GameEvents.OnLivesChanged -= HandleLivesChanged;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnShowMessage -= HandleShowMessage;
    }
    
    private void Start()
    {
        // Initialize UI
        UpdateScore(0);
        UpdateLives(Constants.Game.STARTING_LIVES);
        UpdateLevel(Constants.Game.STARTING_LEVEL);
        
        // Hide panels
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleScoreChanged(int oldScore, int newScore)
    {
        UpdateScore(newScore);
    }
    
    private void HandleLivesChanged(int oldLives, int newLives)
    {
        UpdateLives(newLives);
    }
    
    private void HandleLevelLoaded(int levelIndex)
    {
        UpdateLevel(levelIndex);
    }
    
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        // Update UI based on state
        if (pausePanel != null)
        {
            pausePanel.SetActive(newState == GameState.Paused);
        }
    }
    
    private void HandleGameOver(bool isWin)
    {
        if (isWin)
        {
            ShowVictoryScreen();
        }
        else
        {
            ShowGameOverScreen();
        }
    }
    
    private void HandleShowMessage(string message, float duration)
    {
        ShowMessage(message, duration);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UPDATE METHODS
    // ═══════════════════════════════════════════════════════════════
    
    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    private void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }
    
    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverScoreText != null && GameManager.Instance != null)
            {
                gameOverScoreText.text = $"Final Score: {GameManager.Instance.PlayerScore}";
            }
        }
    }
    
    private void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            if (gameOverScoreText != null && GameManager.Instance != null)
            {
                gameOverScoreText.text = $"Victory! Score: {GameManager.Instance.PlayerScore}";
            }
        }
    }
    
    private void ShowMessage(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), duration);
        }
    }
    
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BUTTON HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    public void OnResumeButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    public void OnPauseButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }
    
    public void OnMainMenuButton()
    {
        // TODO: Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.Scenes.MAIN_MENU);
    }
}