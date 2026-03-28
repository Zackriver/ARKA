using UnityEngine;

/// <summary>
/// Centralized game constants - eliminates all magic numbers
/// Place in: Assets/Scripts/Core/Constants.cs
/// </summary>
public static class Constants
{
    // ═══════════════════════════════════════════════════════════════
    // BALL SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Ball
    {
        public const float DEFAULT_SPEED = 8f;
        public const float MIN_SPEED = 5f;
        public const float MAX_SPEED = 15f;
        public const float MIN_Y_VELOCITY = 0.5f;
        public const float RANDOM_REFLECTION_RANGE = 0.1f;
        public const float PADDLE_INFLUENCE = 0.5f;
        public const float SPEED_INCREMENT = 0.5f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PADDLE SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Paddle
    {
        public const float DEFAULT_SPEED = 10f;
        public const float DEFAULT_WIDTH = 2f;
        public const float MIN_WIDTH = 1f;
        public const float MAX_WIDTH = 4f;
        public const float WIDTH_CHANGE_AMOUNT = 0.5f;
        public const float BOUNDARY_PADDING = 0.5f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BRICK SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Brick
    {
        public const int INDESTRUCTIBLE_RESISTANCE = -1;
        public const int DEFAULT_RESISTANCE = 1;
        public const int DEFAULT_SCORE_VALUE = 10;
        public const float DROP_CHANCE = 0.3f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POWER-UP SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class PowerUp
    {
        public const float DEFAULT_DURATION = 10f;
        public const float MIN_DURATION = 1f;
        public const float MAX_DURATION = 60f;
        public const float DROP_FALL_SPEED = 3f;
        public const float MULTI_BALL_ANGLE_SPREAD = 15f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // GAME SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Game
    {
        public const int STARTING_LIVES = 3;
        public const int MAX_LIVES = 5;
        public const int STARTING_LEVEL = 1;
        public const float LEVEL_TRANSITION_DELAY = 2f;
        public const float RESPAWN_DELAY = 1f;
        public const float GAME_OVER_DELAY = 3f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Inventory
    {
        public const int MAX_STACK_SIZE = 99;
        public const int DEFAULT_SLOTS = 20;
        public const int CRAFTING_SLOTS = 4;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // AUDIO SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Audio
    {
        public const int SAMPLE_RATE = 44100;
        public const float DEFAULT_VOLUME = 1f;
        public const float MIN_FREQUENCY = 20f;
        public const float MAX_FREQUENCY = 20000f;
        
        // Sound frequencies
        public const float BALL_HIT_FREQUENCY = 440f;
        public const float BRICK_BREAK_FREQUENCY = 880f;
        public const float POWER_UP_FREQUENCY = 660f;
        public const float GAME_OVER_FREQUENCY = 220f;
        
        // Sound durations
        public const float SHORT_SOUND_DURATION = 0.1f;
        public const float MEDIUM_SOUND_DURATION = 0.3f;
        public const float LONG_SOUND_DURATION = 0.5f;
        
        // Pool settings
        public const int AUDIO_POOL_SIZE = 10;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // VISUAL SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Visual
    {
        public const int DEFAULT_TEXTURE_SIZE = 64;
        public const int MIN_TEXTURE_SIZE = 16;
        public const int MAX_TEXTURE_SIZE = 256;
        public const float PIXEL_PER_UNIT = 100f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PHYSICS SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    public static class Physics
    {
        public const float GRAVITY_SCALE = 0f;
        public const float BOUNCE_THRESHOLD = 0.1f;
        public const float COLLISION_OFFSET = 0.01f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LAYER NAMES (for LayerMask)
    // ═══════════════════════════════════════════════════════════════
    
    public static class Layers
    {
        public const string BALL = "Ball";
        public const string PADDLE = "Paddle";
        public const string BRICK = "Brick";
        public const string WALL = "Wall";
        public const string POWER_UP = "PowerUp";
        public const string ITEM = "Item";
    }
    
    // ═══════════════════════════════════════════════════════════════
    // TAG NAMES
    // ═══════════════════════════════════════════════════════════════
    
    public static class Tags
    {
        public const string PLAYER = "Player";
        public const string BALL = "Ball";
        public const string BRICK = "Brick";
        public const string WALL = "Wall";
        public const string DEATH_ZONE = "DeathZone";
        public const string POWER_UP = "PowerUp";
        public const string ITEM = "Item";
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SCENE NAMES
    // ═══════════════════════════════════════════════════════════════
    
    public static class Scenes
    {
        public const string MAIN_MENU = "MainMenu";
        public const string GAME = "Game";
        public const string LEVEL_SELECT = "LevelSelect";
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SAVE DATA KEYS
    // ═══════════════════════════════════════════════════════════════
    
    public static class SaveKeys
    {
        public const string HIGH_SCORE = "HighScore";
        public const string CURRENT_LEVEL = "CurrentLevel";
        public const string UNLOCKED_LEVELS = "UnlockedLevels";
        public const string INVENTORY_DATA = "InventoryData";
        public const string SETTINGS_DATA = "SettingsData";
        public const string PLAYER_LIVES = "PlayerLives";
        public const string PLAYER_SCORE = "PlayerScore";
    }
}