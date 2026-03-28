using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual menu UI components
/// Place in: Assets/Scripts/UI/MenuUI.cs
/// </summary>
public class MenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Settings")]
    [SerializeField] private string gameTitle = "ARKA";
    [SerializeField] private string gameVersion = "v1.0.0";
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Start()
    {
        InitializeUI();
        SetupButtons();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void InitializeUI()
    {
        if (titleText != null)
        {
            titleText.text = gameTitle;
        }
        
        if (versionText != null)
        {
            versionText.text = gameVersion;
        }
    }
    
    private void SetupButtons()
    {
        if (playButton != null && MenuManager.Instance != null)
        {
            playButton.onClick.AddListener(MenuManager.Instance.OnPlayButton);
        }
        
        if (optionsButton != null && MenuManager.Instance != null)
        {
            optionsButton.onClick.AddListener(MenuManager.Instance.OnOptionsButton);
        }
        
        if (creditsButton != null && MenuManager.Instance != null)
        {
            creditsButton.onClick.AddListener(MenuManager.Instance.OnCreditsButton);
        }
        
        if (quitButton != null && MenuManager.Instance != null)
        {
            quitButton.onClick.AddListener(MenuManager.Instance.OnQuitButton);
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════
    
    public void SetTitle(string title)
    {
        gameTitle = title;
        if (titleText != null)
        {
            titleText.text = title;
        }
    }
    
    public void SetVersion(string version)
    {
        gameVersion = version;
        if (versionText != null)
        {
            versionText.text = version;
        }
    }
}