using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages main menu and scene transitions
/// Place in: Assets/Scripts/UI/MenuManager.cs
/// </summary>
public class MenuManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static MenuManager _instance;
    
    public static MenuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MenuManager>();
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Start()
    {
        ShowMainMenu();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // MENU NAVIGATION
    // ═══════════════════════════════════════════════════════════════
    
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    public void ShowOptions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    public void ShowCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BUTTON CALLBACKS
    // ═══════════════════════════════════════════════════════════════
    
    public void OnPlayButton()
    {
        LoadGameScene();
    }
    
    public void OnOptionsButton()
    {
        ShowOptions();
    }
    
    public void OnCreditsButton()
    {
        ShowCredits();
    }
    
    public void OnBackButton()
    {
        ShowMainMenu();
    }
    
    public void OnQuitButton()
    {
        QuitGame();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SCENE LOADING
    // ═══════════════════════════════════════════════════════════════
    
    private void LoadGameScene()
    {
        Debug.Log("[MenuManager] Loading game scene...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load game scene
        SceneManager.LoadScene(Constants.Scenes.GAME);
    }
    
    public void LoadMainMenu()
    {
        Debug.Log("[MenuManager] Loading main menu...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene(Constants.Scenes.MAIN_MENU);
    }
    
    private void QuitGame()
    {
        Debug.Log("[MenuManager] Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}