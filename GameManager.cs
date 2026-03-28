using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Complete game controller with state machine, event system, and visual level editor support
/// Place in: Assets/Scripts/Core/GameManager.cs
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                
                if (_instance == null)
                {
                    Debug.LogError("[GameManager] No GameManager found in scene!");
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LEVEL EDITOR SUPPORT
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Level Editor")]
    public LevelData currentLevelData;
    public GameObject brickPrefab;
    public List<BrickShape> availableShapes = new List<BrickShape>();
    public List<GameObject> activeBricks = new List<GameObject>();
    
    [Header("Level Bounds")]
    public Vector2 boundsCenter = Vector2.zero;
    public Vector2 boundsSize = new Vector2(18f, 10f);
    
    [Header("Brick Spacing")]
    public float brickGapX = 0.1f;
    public float brickGapY = 0.1f;
    
    [Header("Default Brick Settings")]
    public Vector2 defaultBrickSize = new Vector2(1f, 0.5f);
    
    // ═══════════════════════════════════════════════════════════════
    // GAME REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Game Prefabs")]
    [SerializeField] private GameObject ballPrefab;
    
    [Header("Game References")]
    [SerializeField] private Paddle paddle;
    [SerializeField] private Transform spawnPoint;
    
    // Cached references
    private Ball _currentBall;
    private List<Ball> _activeBalls = new List<Ball>();
    private List<Brick> _activeBrickComponents = new List<Brick>();
    
    // ═══════════════════════════════════════════════════════════════
    // GAME STATE
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Game State")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int playerLives = Constants.Game.STARTING_LIVES;
    [SerializeField] private int playerScore = 0;
    
    public int CurrentLevel => currentLevel;
    public int PlayerLives => playerLives;
    public int PlayerScore => playerScore;
    
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════
    
    public Paddle Paddle => paddle;
    public Ball CurrentBall => _currentBall;
    public int ActiveBallCount => _activeBalls.Count;
    public int RemainingBricks => _activeBrickComponents.Count;
    
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
        
        InitializeReferences();
    }
    
    private void OnEnable()
    {
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void Start()
    {
        GameStateManager.Instance.ChangeState(GameState.WaitingToStart);
        
        // Load level from LevelData if available
        if (currentLevelData != null && Application.isPlaying)
        {
            LoadLevelFromData();
        }
        
        InitializeLevel();
        
        // Auto-load save if exists
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            SaveSystem.Instance.LoadGame();
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            UnsubscribeFromEvents();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void InitializeReferences()
    {
        // Find paddle if not assigned
        if (paddle == null)
        {
            paddle = FindFirstObjectByType<Paddle>();
            
            if (paddle == null)
            {
                Debug.LogWarning("[GameManager] No Paddle found in scene!");
            }
        }
        
        // Create spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject spawnGO = new GameObject("BallSpawnPoint");
            spawnPoint = spawnGO.transform;
            spawnPoint.SetParent(transform);
            
            // Position above paddle
            if (paddle != null)
            {
                Vector3 pos = paddle.transform.position;
                pos.y += 1f;
                spawnPoint.position = pos;
            }
            else
            {
                spawnPoint.position = new Vector3(0, -3, 0);
            }
        }
        
        // Validate ball prefab
        if (ballPrefab == null)
        {
            Debug.LogWarning("[GameManager] Ball prefab not assigned!");
        }
        else if (ballPrefab.GetComponent<Ball>() == null)
        {
            Debug.LogError("[GameManager] Ball prefab missing Ball component!");
        }
        
        // Initialize activeBricks list if null
        if (activeBricks == null)
        {
            activeBricks = new List<GameObject>();
        }
    }
    
    private void InitializeLevel()
    {
        RefreshBrickList();
        
        Debug.Log($"[GameManager] Level {currentLevel} initialized with {_activeBrickComponents.Count} bricks");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LEVEL EDITOR METHODS
    // ═══════════════════════════════════════════════════════════════
    
    public Vector2 GetStepSize(BrickShape shape)
    {
        Vector2 brickSize = GetBrickWorldSize(shape);
        return new Vector2(brickSize.x + brickGapX, brickSize.y + brickGapY);
    }
    
    public Vector2 GetBrickWorldSize(BrickShape shape)
    {
        if (shape != null)
        {
            return shape.size;
        }
        return defaultBrickSize;
    }
    
    public bool IsInBounds(Vector2 position)
    {
        float halfWidth = boundsSize.x / 2f;
        float halfHeight = boundsSize.y / 2f;
        
        return position.x >= boundsCenter.x - halfWidth &&
               position.x <= boundsCenter.x + halfWidth &&
               position.y >= boundsCenter.y - halfHeight &&
               position.y <= boundsCenter.y + halfHeight;
    }
    
    public GameObject CreateBrick(BrickData brickData, bool isPreview = false)
    {
        if (brickPrefab == null)
        {
            Debug.LogError("[GameManager] Brick prefab not assigned!");
            return null;
        }
        
        GameObject brickObj = Instantiate(brickPrefab, brickData.position, Quaternion.identity);
        
        if (isPreview)
        {
            brickObj.hideFlags = HideFlags.DontSave;
        }
        
        // Apply scale
        if (brickData.brickScale != Vector3.zero)
        {
            brickObj.transform.localScale = brickData.brickScale;
        }
        
        // Set up sprite renderer
        SpriteRenderer sr = brickObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = brickData.color;
            
            // Apply shape sprite if available
            if (brickData.shape != null && brickData.shape.sprite != null)
            {
                sr.sprite = brickData.shape.sprite;
            }
        }
        
        // Set up brick component
        Brick brickComponent = brickObj.GetComponent<Brick>();
        if (brickComponent == null)
        {
            brickComponent = brickObj.AddComponent<Brick>();
        }
        
        // Set brick properties using reflection or public setters
        // Note: You may need to add public setters to Brick.cs for these
        
        return brickObj;
    }
    
    private void LoadLevelFromData()
    {
        if (currentLevelData == null || currentLevelData.bricks == null)
        {
            Debug.LogWarning("[GameManager] No level data to load!");
            return;
        }
        
        // Clear existing bricks
        ClearAllBricks();
        
        // Spawn bricks from data
        foreach (BrickData brickData in currentLevelData.bricks)
        {
            GameObject brick = CreateBrick(brickData, false);
            if (brick != null)
            {
                activeBricks.Add(brick);
            }
        }
        
        Debug.Log($"[GameManager] Loaded {currentLevelData.bricks.Count} bricks from level data");
    }
    
    private void ClearAllBricks()
    {
        // Destroy all active bricks
        foreach (GameObject brick in activeBricks)
        {
            if (brick != null)
            {
                Destroy(brick);
            }
        }
        activeBricks.Clear();
        
        // Also find and destroy any bricks in scene
        Brick[] allBricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        foreach (Brick brick in allBricks)
        {
            if (brick != null)
            {
                Destroy(brick.gameObject);
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT SUBSCRIPTION
    // ═══════════════════════════════════════════════════════════════
    
    private void SubscribeToEvents()
    {
        GameEvents.OnBallLost += HandleBallLost;
        GameEvents.OnBrickDestroyed += HandleBrickDestroyed;
        GameEvents.OnAllBricksDestroyed += HandleAllBricksDestroyed;
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnGameOver += HandleGameOver;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnBallLost -= HandleBallLost;
        GameEvents.OnBrickDestroyed -= HandleBrickDestroyed;
        GameEvents.OnAllBricksDestroyed -= HandleAllBricksDestroyed;
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnGameOver -= HandleGameOver;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandleBallLost(Ball ball)
    {
        if (ball == null) return;
        
        // Remove from active balls
        _activeBalls.Remove(ball);
        
        if (ball == _currentBall)
        {
            _currentBall = null;
        }
        
        // Destroy the ball
        Destroy(ball.gameObject);
        
        // Check if any balls remain
        if (_activeBalls.Count == 0)
        {
            LoseLife();
        }
    }
    
    private void HandleBrickDestroyed(Brick brick, int scoreValue)
    {
        if (brick == null) return;
        
        // Add score
        AddScore(scoreValue);
        
        // Remove from active bricks
        _activeBrickComponents.Remove(brick);
        
        // Also remove from activeBricks GameObject list
        if (brick.gameObject != null)
        {
            activeBricks.Remove(brick.gameObject);
        }
        
        // Check level completion
        CheckLevelComplete();
    }
    
    private void HandleAllBricksDestroyed()
    {
        CompleteLevel();
    }
    
    private void HandleGameStarted()
    {
        if (_currentBall == null)
        {
            SpawnBall();
        }
    }
    
    private void HandleGameOver(bool isWin)
    {
        if (isWin)
        {
            Debug.Log($"[GameManager] Victory! Final Score: {playerScore}");
        }
        else
        {
            Debug.Log($"[GameManager] Game Over! Final Score: {playerScore}");
        }
        
        StartCoroutine(GameOverRoutine(isWin));
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BALL MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    public Ball SpawnBall()
    {
        return SpawnBall(spawnPoint != null ? spawnPoint.position : Vector3.zero);
    }
    
    public Ball SpawnBall(Vector3 position)
    {
        if (ballPrefab == null)
        {
            Debug.LogError("[GameManager] Cannot spawn ball - prefab is null!");
            return null;
        }
        
        GameObject ballGO = Instantiate(ballPrefab, position, Quaternion.identity);
        Ball ball = ballGO.GetComponent<Ball>();
        
        if (ball == null)
        {
            Debug.LogError("[GameManager] Ball prefab missing Ball component!");
            Destroy(ballGO);
            return null;
        }
        
        // Track the ball
        _activeBalls.Add(ball);
        
        if (_currentBall == null)
        {
            _currentBall = ball;
        }
        
        // Fire event
        GameEvents.BallSpawned(ball);
        
        Debug.Log($"[GameManager] Ball spawned at {position}");
        
        return ball;
    }
    
    public void LaunchBall()
    {
        if (_currentBall == null)
        {
            Debug.LogWarning("[GameManager] No ball to launch!");
            return;
        }
        
        if (!GameStateManager.Instance.CanPlay)
        {
            Debug.LogWarning("[GameManager] Cannot launch ball - game not in playable state");
            return;
        }
        
        // Launch the ball
        _currentBall.Launch();
        
        // Change state to playing
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }
    
    public void SpawnMultiBalls(int count)
    {
        if (_currentBall == null)
        {
            Debug.LogWarning("[GameManager] No current ball to spawn from!");
            return;
        }
        
        Vector3 currentPos = _currentBall.transform.position;
        Vector2 currentVel = _currentBall.GetComponent<Rigidbody2D>().linearVelocity;
        
        for (int i = 0; i < count; i++)
        {
            Ball newBall = SpawnBall(currentPos);
            
            if (newBall != null)
            {
                // Launch at angle spread
                float angle = Constants.PowerUp.MULTI_BALL_ANGLE_SPREAD * (i + 1);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * currentVel.normalized;
                newBall.Launch(direction);
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BRICK MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private void RefreshBrickList()
    {
        _activeBrickComponents.Clear();
        
        Brick[] allBricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        
        foreach (Brick brick in allBricks)
        {
            // Only count destructible bricks
            if (brick != null && !brick.IsIndestructible)
            {
                _activeBrickComponents.Add(brick);
            }
        }
    }
    
    private void CheckLevelComplete()
    {
        // Remove any null references
        _activeBrickComponents.RemoveAll(brick => brick == null);
        
        // Check if all destructible bricks are gone
        if (_activeBrickComponents.Count == 0)
        {
            GameEvents.AllBricksDestroyed();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SCORE & LIVES
    // ═══════════════════════════════════════════════════════════════
    
    public void AddScore(int points)
    {
        int oldScore = playerScore;
        playerScore += points;
        
        GameEvents.ScoreChanged(oldScore, playerScore);
        
        Debug.Log($"[GameManager] Score: {oldScore} -> {playerScore} (+{points})");
    }
    
    public void AddLife()
    {
        if (playerLives >= Constants.Game.MAX_LIVES)
        {
            // Convert to score instead
            AddScore(1000);
            return;
        }
        
        int oldLives = playerLives;
        playerLives++;
        
        GameEvents.LivesChanged(oldLives, playerLives);
    }
    
    private void LoseLife()
    {
        int oldLives = playerLives;
        playerLives--;
        
        GameEvents.LivesChanged(oldLives, playerLives);
        
        Debug.Log($"[GameManager] Lives: {oldLives} -> {playerLives}");
        
        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(RespawnRoutine());
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LEVEL CONTROL
    // ═══════════════════════════════════════════════════════════════
    
    public void StartGame()
    {
        if (GameStateManager.Instance.CurrentState == GameState.WaitingToStart)
        {
            LaunchBall();
        }
        else if (GameStateManager.Instance.CurrentState == GameState.Paused)
        {
            GameStateManager.Instance.ChangeState(GameState.Playing);
        }
    }
    
    public void PauseGame()
    {
        if (GameStateManager.Instance.IsPlaying)
        {
            GameStateManager.Instance.ChangeState(GameState.Paused);
        }
    }
    
    public void ResumeGame()
    {
        if (GameStateManager.Instance.IsPaused)
        {
            GameStateManager.Instance.ChangeState(GameState.Playing);
        }
    }
    
    private void CompleteLevel()
    {
        Debug.Log($"[GameManager] Level {currentLevel} complete!");
        
        GameStateManager.Instance.ChangeState(GameState.LevelComplete);
        GameEvents.LevelCompleted(currentLevel);
        
        // Save progress
        SaveProgress();
        
        StartCoroutine(LevelCompleteRoutine());
    }
    
    private void GameOver()
    {
        // Update high score
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveHighScore(playerScore);
        }
        
        GameStateManager.Instance.ChangeState(GameState.GameOver);
    }
    
    public void RestartGame()
    {
        // Reset state
        playerScore = 0;
        playerLives = Constants.Game.STARTING_LIVES;
        currentLevel = Constants.Game.STARTING_LEVEL;
        
        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        GameEvents.LevelLoading(levelIndex);
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SAVE/LOAD INTEGRATION
    // ═══════════════════════════════════════════════════════════════
    
    public void SaveProgress()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            Debug.Log("[GameManager] Progress saved");
        }
    }
    
    public void LoadProgress()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            bool loaded = SaveSystem.Instance.LoadGame();
            
            if (loaded)
            {
                Debug.Log("[GameManager] Progress loaded");
            }
        }
    }
    
    public void SetGameState(int level, int score, int lives)
    {
        currentLevel = level;
        playerScore = score;
        playerLives = lives;
        
        Debug.Log($"[GameManager] Game state set: Level {level}, Score {score}, Lives {lives}");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // COROUTINES
    // ═══════════════════════════════════════════════════════════════
    
    private IEnumerator RespawnRoutine()
    {
        GameStateManager.Instance.ChangeState(GameState.WaitingToStart);
        
        yield return new WaitForSeconds(Constants.Game.RESPAWN_DELAY);
        
        if (paddle != null)
        {
            paddle.ResetPosition();
        }
        
        SpawnBall();
        
        GameEvents.ShowMessage("Press SPACE to launch", 2f);
    }
    
    private IEnumerator LevelCompleteRoutine()
    {
        yield return new WaitForSeconds(Constants.Game.LEVEL_TRANSITION_DELAY);
        
        currentLevel++;
        LoadLevel(currentLevel);
    }
    
    private IEnumerator GameOverRoutine(bool isWin)
    {
        yield return new WaitForSeconds(Constants.Game.GAME_OVER_DELAY);
        
        Debug.Log("[GameManager] Restarting game...");
        RestartGame();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"State: {GameStateManager.Instance.CurrentState}");
        GUILayout.Label($"Score: {playerScore}");
        GUILayout.Label($"Lives: {playerLives}");
        GUILayout.Label($"Level: {currentLevel}");
        GUILayout.Label($"Active Balls: {_activeBalls.Count}");
        GUILayout.Label($"Remaining Bricks: {_activeBrickComponents.Count}");
        GUILayout.EndArea();
    }
}