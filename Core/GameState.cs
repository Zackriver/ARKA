using System;
using UnityEngine;

/// <summary>
/// Game state enumeration
/// </summary>
public enum GameState
{
    None,
    Initializing,
    MainMenu,
    Loading,
    WaitingToStart,
    Playing,
    Paused,
    LevelComplete,
    GameOver,
    Victory
}

/// <summary>
/// Manages game state transitions with validation
/// Place in: Assets/Scripts/Core/GameState.cs
/// 
/// Usage:
///     GameStateManager.Instance.ChangeState(GameState.Playing);
///     if (GameStateManager.Instance.CurrentState == GameState.Playing) { }
///     if (GameStateManager.Instance.IsPlaying) { }
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static GameStateManager _instance;
    
    public static GameStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameStateManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[GameStateManager]");
                    _instance = go.AddComponent<GameStateManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // STATE PROPERTIES
    // ═══════════════════════════════════════════════════════════════
    
    [SerializeField] 
    private GameState _currentState = GameState.None;
    
    [SerializeField]
    private GameState _previousState = GameState.None;
    
    public GameState CurrentState => _currentState;
    public GameState PreviousState => _previousState;
    
    // Convenience properties
    public bool IsPlaying => _currentState == GameState.Playing;
    public bool IsPaused => _currentState == GameState.Paused;
    public bool IsGameOver => _currentState == GameState.GameOver || _currentState == GameState.Victory;
    public bool IsInMenu => _currentState == GameState.MainMenu;
    public bool IsLoading => _currentState == GameState.Loading || _currentState == GameState.Initializing;
    public bool IsWaitingToStart => _currentState == GameState.WaitingToStart;
    public bool CanPlay => _currentState == GameState.Playing || _currentState == GameState.WaitingToStart;
    
    // ═══════════════════════════════════════════════════════════════
    // VALID STATE TRANSITIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Defines which state transitions are allowed
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<GameState, GameState[]> ValidTransitions = 
        new System.Collections.Generic.Dictionary<GameState, GameState[]>
    {
        { GameState.None, new[] { GameState.Initializing, GameState.MainMenu } },
        { GameState.Initializing, new[] { GameState.MainMenu, GameState.Loading } },
        { GameState.MainMenu, new[] { GameState.Loading, GameState.Initializing } },
        { GameState.Loading, new[] { GameState.WaitingToStart, GameState.MainMenu, GameState.Playing } },
        { GameState.WaitingToStart, new[] { GameState.Playing, GameState.Paused, GameState.MainMenu } },
        { GameState.Playing, new[] { GameState.Paused, GameState.LevelComplete, GameState.GameOver, GameState.Victory, GameState.MainMenu } },
        { GameState.Paused, new[] { GameState.Playing, GameState.MainMenu, GameState.WaitingToStart } },
        { GameState.LevelComplete, new[] { GameState.Loading, GameState.Victory, GameState.MainMenu } },
        { GameState.GameOver, new[] { GameState.MainMenu, GameState.Loading, GameState.WaitingToStart } },
        { GameState.Victory, new[] { GameState.MainMenu, GameState.Loading } }
    };
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        _currentState = GameState.None;
        _previousState = GameState.None;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // STATE CHANGE METHODS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Changes game state with validation
    /// </summary>
    /// <param name="newState">State to change to</param>
    /// <param name="force">If true, bypasses validation</param>
    /// <returns>True if state changed successfully</returns>
    public bool ChangeState(GameState newState, bool force = false)
    {
        // Same state - no change needed
        if (_currentState == newState)
        {
            Debug.LogWarning($"[GameStateManager] Already in state: {newState}");
            return false;
        }
        
        // Validate transition
        if (!force && !IsValidTransition(_currentState, newState))
        {
            Debug.LogError($"[GameStateManager] Invalid state transition: {_currentState} -> {newState}");
            return false;
        }
        
        // Perform transition
        GameState oldState = _currentState;
        _previousState = _currentState;
        _currentState = newState;
        
        Debug.Log($"[GameStateManager] State changed: {oldState} -> {newState}");
        
        // Fire event
        GameEvents.GameStateChanged(oldState, newState);
        
        // Handle state-specific events
        HandleStateChange(oldState, newState);
        
        return true;
    }
    
    /// <summary>
    /// Returns to the previous state
    /// </summary>
    public bool ReturnToPreviousState()
    {
        if (_previousState == GameState.None)
        {
            Debug.LogWarning("[GameStateManager] No previous state to return to");
            return false;
        }
        
        return ChangeState(_previousState);
    }
    
    /// <summary>
    /// Checks if a transition is valid
    /// </summary>
    public bool IsValidTransition(GameState from, GameState to)
    {
        if (!ValidTransitions.ContainsKey(from))
        {
            return false;
        }
        
        GameState[] validStates = ValidTransitions[from];
        return System.Array.Exists(validStates, state => state == to);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // STATE-SPECIFIC HANDLING
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleStateChange(GameState oldState, GameState newState)
    {
        // Handle exiting old state
        switch (oldState)
        {
            case GameState.Paused:
                Time.timeScale = 1f;
                break;
        }
        
        // Handle entering new state
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                if (oldState == GameState.WaitingToStart || oldState == GameState.Loading)
                {
                    GameEvents.GameStarted();
                }
                else if (oldState == GameState.Paused)
                {
                    GameEvents.GameResumed();
                }
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                GameEvents.GamePaused();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 1f;
                GameEvents.GameOver(false);
                break;
                
            case GameState.Victory:
                Time.timeScale = 1f;
                GameEvents.GameOver(true);
                break;
                
            case GameState.LevelComplete:
                // Level complete handling
                break;
                
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Toggles pause state
    /// </summary>
    public void TogglePause()
    {
        if (_currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
        else if (_currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }
    
    /// <summary>
    /// Resets state manager to initial state
    /// </summary>
    public void Reset()
    {
        _currentState = GameState.None;
        _previousState = GameState.None;
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Gets a string representation of the current state for debugging
    /// </summary>
    public string GetStateInfo()
    {
        return $"Current: {_currentState}, Previous: {_previousState}";
    }
}