using System;
using UnityEngine;

/// <summary>
/// Centralized event system for decoupling game components
/// Place in: Assets/Scripts/Core/GameEvents.cs
/// 
/// Usage example:
///     // Subscribe
///     GameEvents.OnBrickDestroyed += HandleBrickDestroyed;
///     
///     // Unsubscribe (important in OnDestroy!)
///     GameEvents.OnBrickDestroyed -= HandleBrickDestroyed;
///     
///     // Trigger
///     GameEvents.BrickDestroyed(brick, points);
/// </summary>
public static class GameEvents
{
    // ═══════════════════════════════════════════════════════════════
    // GAME STATE EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when game state changes</summary>
    public static event Action<GameState, GameState> OnGameStateChanged;
    
    /// <summary>Fired when game starts</summary>
    public static event Action OnGameStarted;
    
    /// <summary>Fired when game pauses</summary>
    public static event Action OnGamePaused;
    
    /// <summary>Fired when game resumes</summary>
    public static event Action OnGameResumed;
    
    /// <summary>Fired when game is over</summary>
    public static event Action<bool> OnGameOver; // bool = isWin
    
    /// <summary>Fired when level starts loading</summary>
    public static event Action<int> OnLevelLoading;
    
    /// <summary>Fired when level is loaded and ready</summary>
    public static event Action<int> OnLevelLoaded;
    
    /// <summary>Fired when level is completed</summary>
    public static event Action<int> OnLevelCompleted;
    
    // Trigger methods
    public static void GameStateChanged(GameState oldState, GameState newState)
    {
        OnGameStateChanged?.Invoke(oldState, newState);
    }
    
    public static void GameStarted()
    {
        OnGameStarted?.Invoke();
    }
    
    public static void GamePaused()
    {
        OnGamePaused?.Invoke();
    }
    
    public static void GameResumed()
    {
        OnGameResumed?.Invoke();
    }
    
    public static void GameOver(bool isWin)
    {
        OnGameOver?.Invoke(isWin);
    }
    
    public static void LevelLoading(int levelIndex)
    {
        OnLevelLoading?.Invoke(levelIndex);
    }
    
    public static void LevelLoaded(int levelIndex)
    {
        OnLevelLoaded?.Invoke(levelIndex);
    }
    
    public static void LevelCompleted(int levelIndex)
    {
        OnLevelCompleted?.Invoke(levelIndex);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BALL EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when ball spawns</summary>
    public static event Action<Ball> OnBallSpawned;
    
    /// <summary>Fired when ball is lost (falls below paddle)</summary>
    public static event Action<Ball> OnBallLost;
    
    /// <summary>Fired when ball hits paddle</summary>
    public static event Action<Ball, Paddle> OnBallHitPaddle;
    
    /// <summary>Fired when ball hits brick</summary>
    public static event Action<Ball, Brick> OnBallHitBrick;
    
    /// <summary>Fired when ball hits wall</summary>
    public static event Action<Ball> OnBallHitWall;
    
    // Trigger methods
    public static void BallSpawned(Ball ball)
    {
        OnBallSpawned?.Invoke(ball);
    }
    
    public static void BallLost(Ball ball)
    {
        OnBallLost?.Invoke(ball);
    }
    
    public static void BallHitPaddle(Ball ball, Paddle paddle)
    {
        OnBallHitPaddle?.Invoke(ball, paddle);
    }
    
    public static void BallHitBrick(Ball ball, Brick brick)
    {
        OnBallHitBrick?.Invoke(ball, brick);
    }
    
    public static void BallHitWall(Ball ball)
    {
        OnBallHitWall?.Invoke(ball);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BRICK EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when brick is hit but not destroyed</summary>
    public static event Action<Brick, int> OnBrickHit; // Brick, remaining resistance
    
    /// <summary>Fired when brick is destroyed</summary>
    public static event Action<Brick, int> OnBrickDestroyed; // Brick, score value
    
    /// <summary>Fired when all bricks are destroyed</summary>
    public static event Action OnAllBricksDestroyed;
    
    // Trigger methods
    public static void BrickHit(Brick brick, int remainingResistance)
    {
        OnBrickHit?.Invoke(brick, remainingResistance);
    }
    
    public static void BrickDestroyed(Brick brick, int scoreValue)
    {
        OnBrickDestroyed?.Invoke(brick, scoreValue);
    }
    
    public static void AllBricksDestroyed()
    {
        OnAllBricksDestroyed?.Invoke();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SCORE & LIVES EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when score changes</summary>
    public static event Action<int, int> OnScoreChanged; // oldScore, newScore
    
    /// <summary>Fired when lives change</summary>
    public static event Action<int, int> OnLivesChanged; // oldLives, newLives
    
    /// <summary>Fired when combo is achieved</summary>
    public static event Action<int> OnComboAchieved; // combo count
    
    // Trigger methods
    public static void ScoreChanged(int oldScore, int newScore)
    {
        OnScoreChanged?.Invoke(oldScore, newScore);
    }
    
    public static void LivesChanged(int oldLives, int newLives)
    {
        OnLivesChanged?.Invoke(oldLives, newLives);
    }
    
    public static void ComboAchieved(int comboCount)
    {
        OnComboAchieved?.Invoke(comboCount);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POWER-UP EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when power-up spawns</summary>
    public static event Action<PowerUpData, Vector3> OnPowerUpSpawned;
    
    /// <summary>Fired when power-up is collected</summary>
    public static event Action<PowerUpData> OnPowerUpCollected;
    
    /// <summary>Fired when power-up activates</summary>
    public static event Action<PowerUpData> OnPowerUpActivated;
    
    /// <summary>Fired when power-up expires</summary>
    public static event Action<PowerUpData> OnPowerUpExpired;
    
    // Trigger methods
    public static void PowerUpSpawned(PowerUpData powerUp, Vector3 position)
    {
        OnPowerUpSpawned?.Invoke(powerUp, position);
    }
    
    public static void PowerUpCollected(PowerUpData powerUp)
    {
        OnPowerUpCollected?.Invoke(powerUp);
    }
    
    public static void PowerUpActivated(PowerUpData powerUp)
    {
        OnPowerUpActivated?.Invoke(powerUp);
    }
    
    public static void PowerUpExpired(PowerUpData powerUp)
    {
        OnPowerUpExpired?.Invoke(powerUp);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUFF EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when debuff activates</summary>
    public static event Action<DebuffData> OnDebuffActivated;
    
    /// <summary>Fired when debuff expires</summary>
    public static event Action<DebuffData> OnDebuffExpired;
    
    // Trigger methods
    public static void DebuffActivated(DebuffData debuff)
    {
        OnDebuffActivated?.Invoke(debuff);
    }
    
    public static void DebuffExpired(DebuffData debuff)
    {
        OnDebuffExpired?.Invoke(debuff);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when item is added to inventory</summary>
    public static event Action<ItemData, int> OnItemAdded; // item, amount
    
    /// <summary>Fired when item is removed from inventory</summary>
    public static event Action<ItemData, int> OnItemRemoved; // item, amount
    
    /// <summary>Fired when inventory changes (any change)</summary>
    public static event Action OnInventoryChanged;
    
    /// <summary>Fired when item is dropped in world</summary>
    public static event Action<ItemData, Vector3> OnItemDropped;
    
    /// <summary>Fired when item is picked up</summary>
    public static event Action<ItemData> OnItemPickedUp;
    
    // Trigger methods
    public static void ItemAdded(ItemData item, int amount)
    {
        OnItemAdded?.Invoke(item, amount);
        OnInventoryChanged?.Invoke();
    }
    
    public static void ItemRemoved(ItemData item, int amount)
    {
        OnItemRemoved?.Invoke(item, amount);
        OnInventoryChanged?.Invoke();
    }
    
    public static void InventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
    
    public static void ItemDropped(ItemData item, Vector3 position)
    {
        OnItemDropped?.Invoke(item, position);
    }
    
    public static void ItemPickedUp(ItemData item)
    {
        OnItemPickedUp?.Invoke(item);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CRAFTING EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired when crafting starts</summary>
    public static event Action<CraftingRecipe> OnCraftingStarted;
    
    /// <summary>Fired when crafting succeeds</summary>
    public static event Action<CraftingRecipe, ItemData> OnCraftingSuccess;
    
    /// <summary>Fired when crafting fails</summary>
    public static event Action<CraftingRecipe, string> OnCraftingFailed; // recipe, reason
    
    /// <summary>Fired when crafting slot changes</summary>
    public static event Action<int, ItemData> OnCraftingSlotChanged; // slotIndex, item
    
    // Trigger methods
    public static void CraftingStarted(CraftingRecipe recipe)
    {
        OnCraftingStarted?.Invoke(recipe);
    }
    
    public static void CraftingSuccess(CraftingRecipe recipe, ItemData result)
    {
        OnCraftingSuccess?.Invoke(recipe, result);
    }
    
    public static void CraftingFailed(CraftingRecipe recipe, string reason)
    {
        OnCraftingFailed?.Invoke(recipe, reason);
    }
    
    public static void CraftingSlotChanged(int slotIndex, ItemData item)
    {
        OnCraftingSlotChanged?.Invoke(slotIndex, item);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // AUDIO EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired to request a sound play</summary>
    public static event Action<string, Vector3> OnPlaySound; // soundId, position
    
    /// <summary>Fired to request music change</summary>
    public static event Action<string> OnPlayMusic; // musicId
    
    /// <summary>Fired to stop all sounds</summary>
    public static event Action OnStopAllSounds;
    
    // Trigger methods
    public static void PlaySound(string soundId, Vector3 position = default)
    {
        OnPlaySound?.Invoke(soundId, position);
    }
    
    public static void PlayMusic(string musicId)
    {
        OnPlayMusic?.Invoke(musicId);
    }
    
    public static void StopAllSounds()
    {
        OnStopAllSounds?.Invoke();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UI EVENTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Fired to show a message/notification</summary>
    public static event Action<string, float> OnShowMessage; // message, duration
    
    /// <summary>Fired to update UI element</summary>
    public static event Action<string, object> OnUIUpdate; // elementId, value
    
    // Trigger methods
    public static void ShowMessage(string message, float duration = 2f)
    {
        OnShowMessage?.Invoke(message, duration);
    }
    
    public static void UIUpdate(string elementId, object value)
    {
        OnUIUpdate?.Invoke(elementId, value);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHOD - Clear all events (use carefully!)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Clears all event subscriptions. 
    /// Use only when completely resetting the game or in testing.
    /// </summary>
    public static void ClearAllEvents()
    {
        OnGameStateChanged = null;
        OnGameStarted = null;
        OnGamePaused = null;
        OnGameResumed = null;
        OnGameOver = null;
        OnLevelLoading = null;
        OnLevelLoaded = null;
        OnLevelCompleted = null;
        
        OnBallSpawned = null;
        OnBallLost = null;
        OnBallHitPaddle = null;
        OnBallHitBrick = null;
        OnBallHitWall = null;
        
        OnBrickHit = null;
        OnBrickDestroyed = null;
        OnAllBricksDestroyed = null;
        
        OnScoreChanged = null;
        OnLivesChanged = null;
        OnComboAchieved = null;
        
        OnPowerUpSpawned = null;
        OnPowerUpCollected = null;
        OnPowerUpActivated = null;
        OnPowerUpExpired = null;
        
        OnDebuffActivated = null;
        OnDebuffExpired = null;
        
        OnItemAdded = null;
        OnItemRemoved = null;
        OnInventoryChanged = null;
        OnItemDropped = null;
        OnItemPickedUp = null;
        
        OnCraftingStarted = null;
        OnCraftingSuccess = null;
        OnCraftingFailed = null;
        OnCraftingSlotChanged = null;
        
        OnPlaySound = null;
        OnPlayMusic = null;
        OnStopAllSounds = null;
        
        OnShowMessage = null;
        OnUIUpdate = null;
        
        Debug.Log("[GameEvents] All events cleared");
    }
}