using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages power-up collection, activation, and effects with crafting integration
/// Place in: Assets/Scripts/Systems/PowerUpManager.cs
/// 
/// Works with the recipe-based PowerUpData system
/// </summary>
[DefaultExecutionOrder(-50)]
public class PowerUpManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static PowerUpManager _instance;
    
    public static PowerUpManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PowerUpManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[PowerUpManager]");
                    _instance = go.AddComponent<PowerUpManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Player Level")]
    [SerializeField] private int playerLevel = 0;
    
    public int PlayerLevel
    {
        get => playerLevel;
        set => playerLevel = value;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ACTIVE POWER-UPS TRACKING
    // ═══════════════════════════════════════════════════════════════
    
    // Active power-up instances
    private class ActivePowerUp
    {
        public PowerUpData data;
        public Coroutine coroutine;
        public float remainingTime;
        public float rechargeEndTime;
        public bool isRecharging;
        
        public ActivePowerUp(PowerUpData data, Coroutine coroutine, float duration)
        {
            this.data = data;
            this.coroutine = coroutine;
            this.remainingTime = duration;
            this.rechargeEndTime = 0f;
            this.isRecharging = false;
        }
    }
    
    private Dictionary<PowerUpType, ActivePowerUp> activePowerUps = new Dictionary<PowerUpType, ActivePowerUp>();
    
    // Track original values for restoration
    private Dictionary<PowerUpType, object> originalValues = new Dictionary<PowerUpType, object>();
    
    // Max simultaneous power-ups (unlocked at level 50)
    private int MaxSimultaneousPowerUps => playerLevel >= 50 ? 3 : 1;
    
    // ═══════════════════════════════════════════════════════════════
    // CACHED REFERENCES
    // ═══════════════════════════════════════════════════════════════
    
    private Paddle cachedPaddle;
    private Ball cachedBall;
    
    private Paddle Paddle
    {
        get
        {
            if (cachedPaddle == null && GameManager.Instance != null)
            {
                cachedPaddle = GameManager.Instance.Paddle;
            }
            return cachedPaddle;
        }
    }
    
    private Ball CurrentBall
    {
        get
        {
            if (cachedBall == null && GameManager.Instance != null)
            {
                cachedBall = GameManager.Instance.CurrentBall;
            }
            return cachedBall;
        }
    }
    
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
    }
    
    private void OnEnable()
    {
        // Subscribe to events
        GameEvents.OnPowerUpCollected += HandlePowerUpCollected;
        GameEvents.OnBallSpawned += HandleBallSpawned;
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameEvents.OnPowerUpCollected -= HandlePowerUpCollected;
        GameEvents.OnBallSpawned -= HandleBallSpawned;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
    }
    
    private void Update()
    {
        UpdateRechargeTimes();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            // Clean up all active power-ups
            ClearAllPowerUps();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandlePowerUpCollected(PowerUpData powerUp)
    {
        if (powerUp != null)
        {
            ActivatePowerUp(powerUp);
        }
    }
    
    private void HandleBallSpawned(Ball ball)
    {
        cachedBall = ball;
    }
    
    private void HandleLevelLoaded(int levelIndex)
    {
        // Clear all power-ups when new level loads
        ClearAllPowerUps();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // RECHARGE SYSTEM
    // ═══════════════════════════════════════════════════════════════
    
    private void UpdateRechargeTimes()
    {
        List<PowerUpType> toRemove = new List<PowerUpType>();
        
        foreach (var kvp in activePowerUps)
        {
            if (kvp.Value.isRecharging)
            {
                if (Time.time >= kvp.Value.rechargeEndTime)
                {
                    // Recharge complete
                    toRemove.Add(kvp.Key);
                }
            }
        }
        
        // Remove recharged power-ups from tracking
        foreach (var type in toRemove)
        {
            activePowerUps.Remove(type);
            Debug.Log($"[PowerUpManager] {type} recharge complete");
        }
    }
    
    public bool IsPowerUpOnCooldown(PowerUpType type)
    {
        if (activePowerUps.TryGetValue(type, out ActivePowerUp active))
        {
            return active.isRecharging;
        }
        return false;
    }
    
    public float GetRemainingCooldown(PowerUpType type)
    {
        if (activePowerUps.TryGetValue(type, out ActivePowerUp active))
        {
            if (active.isRecharging)
            {
                return Mathf.Max(0, active.rechargeEndTime - Time.time);
            }
        }
        return 0f;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POWER-UP ACTIVATION
    // ═══════════════════════════════════════════════════════════════
    
    public void ActivatePowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
        {
            Debug.LogWarning("[PowerUpManager] Attempted to activate null power-up");
            return;
        }
        
        // Check if on cooldown
        if (IsPowerUpOnCooldown(powerUp.powerUpType))
        {
            float cooldown = GetRemainingCooldown(powerUp.powerUpType);
            GameEvents.ShowMessage($"{powerUp.powerUpName} on cooldown: {cooldown:F1}s", 2f);
            Debug.LogWarning($"[PowerUpManager] {powerUp.powerUpName} is on cooldown");
            return;
        }
        
        // Check stacking rules
        if (activePowerUps.Count >= MaxSimultaneousPowerUps)
        {
            if (playerLevel < 50)
            {
                // Below level 50: replace oldest power-up
                ReplaceOldestPowerUp(powerUp);
                return;
            }
            else if (!powerUp.allowStacking)
            {
                // Level 50+ but this power-up doesn't allow stacking
                GameEvents.ShowMessage($"{powerUp.powerUpName} cannot stack", 2f);
                Debug.LogWarning($"[PowerUpManager] {powerUp.powerUpName} does not allow stacking");
                return;
            }
        }
        
        Debug.Log($"[PowerUpManager] Activating power-up: {powerUp.powerUpName} ({powerUp.powerUpType})");
        
        // Calculate duration based on player level
        float duration = powerUp.GetDuration(playerLevel);
        
        // Check if this power-up type is already active
        if (activePowerUps.ContainsKey(powerUp.powerUpType))
        {
            // Extend duration
            ActivePowerUp existing = activePowerUps[powerUp.powerUpType];
            
            if (existing.coroutine != null)
            {
                StopCoroutine(existing.coroutine);
            }
            
            existing.remainingTime += duration;
            existing.coroutine = StartCoroutine(PowerUpCoroutine(existing));
            
            Debug.Log($"[PowerUpManager] Extended {powerUp.powerUpName} duration to {existing.remainingTime}s");
        }
        else
        {
            // New power-up activation
            Coroutine coroutine = StartCoroutine(PowerUpCoroutine(new ActivePowerUp(powerUp, null, duration)));
            ActivePowerUp newPowerUp = new ActivePowerUp(powerUp, coroutine, duration);
            newPowerUp.coroutine = coroutine;
            activePowerUps[powerUp.powerUpType] = newPowerUp;
        }
        
        // Fire event
        GameEvents.PowerUpActivated(powerUp);
        GameEvents.ShowMessage($"{powerUp.powerUpName} activated!", 2f);
    }
    
    private void ReplaceOldestPowerUp(PowerUpData newPowerUp)
    {
        // Find the power-up with least remaining time
        PowerUpType oldestType = PowerUpType.MagnetPaddle;
        float leastTime = float.MaxValue;
        
        foreach (var kvp in activePowerUps)
        {
            if (!kvp.Value.isRecharging && kvp.Value.remainingTime < leastTime)
            {
                leastTime = kvp.Value.remainingTime;
                oldestType = kvp.Key;
            }
        }
        
        // Clear the oldest
        ClearPowerUp(oldestType);
        
        // Activate new one
        ActivatePowerUp(newPowerUp);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POWER-UP COROUTINE
    // ═══════════════════════════════════════════════════════════════
    
    private IEnumerator PowerUpCoroutine(ActivePowerUp powerUp)
    {
        // Apply the power-up effect
        ApplyPowerUpEffect(powerUp.data);
        
        // Wait for duration (pause-aware)
        while (powerUp.remainingTime > 0)
        {
            if (!GameStateManager.Instance.IsPaused)
            {
                powerUp.remainingTime -= Time.deltaTime;
            }
            
            yield return null;
        }
        
        // Revert the power-up effect
        RevertPowerUpEffect(powerUp.data);
        
        // Start recharge
        powerUp.isRecharging = true;
        powerUp.rechargeEndTime = Time.time + powerUp.data.rechargeTime;
        
        // Fire event
        GameEvents.PowerUpExpired(powerUp.data);
        
        Debug.Log($"[PowerUpManager] Power-up expired: {powerUp.data.powerUpName}, recharging for {powerUp.data.rechargeTime}s");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // APPLY POWER-UP EFFECTS
    // ═══════════════════════════════════════════════════════════════
    
    private void ApplyPowerUpEffect(PowerUpData powerUp)
    {
        switch (powerUp.powerUpType)
        {
            // PADDLE & DEFENSE
            case PowerUpType.MagnetPaddle:
                ApplyMagnetPaddle();
                break;
            case PowerUpType.WidePaddle:
                ApplyWidePaddle();
                break;
            case PowerUpType.NanoShield:
                ApplyNanoShield();
                break;
            case PowerUpType.AgilityBoost:
                ApplyAgilityBoost();
                break;
            case PowerUpType.TwinPaddles:
                ApplyTwinPaddles();
                break;
                
            // BALL & OFFENSIVE
            case PowerUpType.MultiBall:
                ApplyMultiBall();
                break;
            case PowerUpType.Fireball:
                ApplyFireball();
                break;
            case PowerUpType.AcidBall:
                ApplyAcidBall();
                break;
            case PowerUpType.HeavyBall:
                ApplyHeavyBall();
                break;
            case PowerUpType.GuidedMissileBall:
                ApplyGuidedMissileBall();
                break;
                
            // WEAPONRY
            case PowerUpType.LaserCannons:
                ApplyLaserCannons();
                break;
            case PowerUpType.ExplosiveShrapnel:
                ApplyExplosiveShrapnel();
                break;
            case PowerUpType.EMPulse:
                ApplyEMPulse();
                break;
                
            // UTILITY
            case PowerUpType.TimeDilation:
                ApplyTimeDilation();
                break;
            case PowerUpType.GravityWell:
                ApplyGravityWell();
                break;
            case PowerUpType.ScoreMultiplier:
                ApplyScoreMultiplier();
                break;
            case PowerUpType.BallRadar:
                ApplyBallRadar();
                break;
                
            default:
                Debug.LogWarning($"[PowerUpManager] Unhandled power-up type: {powerUp.powerUpType}");
                break;
        }
        
        GameEvents.PlaySound("PowerUpActivate", Vector3.zero);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // REVERT POWER-UP EFFECTS
    // ═══════════════════════════════════════════════════════════════
    
    private void RevertPowerUpEffect(PowerUpData powerUp)
    {
        switch (powerUp.powerUpType)
        {
            // PADDLE & DEFENSE
            case PowerUpType.MagnetPaddle:
                RevertMagnetPaddle();
                break;
            case PowerUpType.WidePaddle:
                RevertWidePaddle();
                break;
            case PowerUpType.NanoShield:
                RevertNanoShield();
                break;
            case PowerUpType.AgilityBoost:
                RevertAgilityBoost();
                break;
            case PowerUpType.TwinPaddles:
                RevertTwinPaddles();
                break;
                
            // BALL & OFFENSIVE
            case PowerUpType.MultiBall:
                RevertMultiBall();
                break;
            case PowerUpType.Fireball:
                RevertFireball();
                break;
            case PowerUpType.AcidBall:
                RevertAcidBall();
                break;
            case PowerUpType.HeavyBall:
                RevertHeavyBall();
                break;
            case PowerUpType.GuidedMissileBall:
                RevertGuidedMissileBall();
                break;
                
            // WEAPONRY
            case PowerUpType.LaserCannons:
                RevertLaserCannons();
                break;
            case PowerUpType.ExplosiveShrapnel:
                RevertExplosiveShrapnel();
                break;
            case PowerUpType.EMPulse:
                RevertEMPulse();
                break;
                
            // UTILITY
            case PowerUpType.TimeDilation:
                RevertTimeDilation();
                break;
            case PowerUpType.GravityWell:
                RevertGravityWell();
                break;
            case PowerUpType.ScoreMultiplier:
                RevertScoreMultiplier();
                break;
            case PowerUpType.BallRadar:
                RevertBallRadar();
                break;
                
            default:
                Debug.LogWarning($"[PowerUpManager] Unhandled power-up revert: {powerUp.powerUpType}");
                break;
        }
        
        GameEvents.PlaySound("PowerUpExpire", Vector3.zero);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PADDLE & DEFENSE IMPLEMENTATIONS
    // ═══════════════════════════════════════════════════════════════
    
    private void ApplyMagnetPaddle()
    {
        // TODO: Implement magnet paddle (ball sticks to paddle)
        Debug.Log("[PowerUpManager] Magnet Paddle activated");
    }
    
    private void RevertMagnetPaddle()
    {
        Debug.Log("[PowerUpManager] Magnet Paddle deactivated");
    }
    
    private void ApplyWidePaddle()
    {
        if (Paddle == null) return;
        
        float currentWidth = Paddle.GetWidth();
        originalValues[PowerUpType.WidePaddle] = currentWidth;
        Paddle.IncreaseWidth();
        
        Debug.Log("[PowerUpManager] Wide Paddle activated");
    }
    
    private void RevertWidePaddle()
    {
        if (Paddle == null) return;
        
        if (originalValues.TryGetValue(PowerUpType.WidePaddle, out object originalWidth))
        {
            Paddle.SetWidth((float)originalWidth);
            originalValues.Remove(PowerUpType.WidePaddle);
        }
        else
        {
            Paddle.ResetWidth();
        }
        
        Debug.Log("[PowerUpManager] Wide Paddle deactivated");
    }
    
    private void ApplyNanoShield()
    {
        // TODO: Implement shield (prevent ball loss once)
        Debug.Log("[PowerUpManager] Nano Shield activated");
    }
    
    private void RevertNanoShield()
    {
        Debug.Log("[PowerUpManager] Nano Shield deactivated");
    }
    
    private void ApplyAgilityBoost()
    {
        if (Paddle == null) return;
        
        float currentSpeed = Paddle.GetSpeed();
        originalValues[PowerUpType.AgilityBoost] = currentSpeed;
        Paddle.SetSpeed(currentSpeed * 1.5f);
        
        Debug.Log("[PowerUpManager] Agility Boost activated");
    }
    
    private void RevertAgilityBoost()
    {
        if (Paddle == null) return;
        
        if (originalValues.TryGetValue(PowerUpType.AgilityBoost, out object originalSpeed))
        {
            Paddle.SetSpeed((float)originalSpeed);
            originalValues.Remove(PowerUpType.AgilityBoost);
        }
        else
        {
            Paddle.ResetSpeed();
        }
        
        Debug.Log("[PowerUpManager] Agility Boost deactivated");
    }
    
    private void ApplyTwinPaddles()
    {
        // TODO: Implement twin paddles (two paddles controlled together)
        Debug.Log("[PowerUpManager] Twin Paddles activated");
    }
    
    private void RevertTwinPaddles()
    {
        Debug.Log("[PowerUpManager] Twin Paddles deactivated");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // BALL & OFFENSIVE IMPLEMENTATIONS
    // ═══════════════════════════════════════════════════════════════
    
    private void ApplyMultiBall()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnMultiBalls(2);
        }
        
        Debug.Log("[PowerUpManager] Multi Ball activated");
    }
    
    private void RevertMultiBall()
    {
        // Multi-ball is instant, no revert needed
        Debug.Log("[PowerUpManager] Multi Ball completed");
    }
    
    private void ApplyFireball()
    {
        // TODO: Implement fireball (ball destroys multiple bricks)
        Debug.Log("[PowerUpManager] Fireball activated");
    }
    
    private void RevertFireball()
    {
        Debug.Log("[PowerUpManager] Fireball deactivated");
    }
    
    private void ApplyAcidBall()
    {
        // TODO: Implement acid ball (ball melts through bricks)
        Debug.Log("[PowerUpManager] Acid Ball activated");
    }
    
    private void RevertAcidBall()
    {
        Debug.Log("[PowerUpManager] Acid Ball deactivated");
    }
    
    private void ApplyHeavyBall()
    {
        if (CurrentBall == null) return;
        
        // Heavy ball: slower but more destructive
        float currentSpeed = CurrentBall.CurrentSpeed;
        originalValues[PowerUpType.HeavyBall] = currentSpeed;
        CurrentBall.SetSpeed(currentSpeed * 0.7f);
        
        Debug.Log("[PowerUpManager] Heavy Ball activated");
    }
    
    private void RevertHeavyBall()
    {
        if (CurrentBall == null) return;
        
        if (originalValues.TryGetValue(PowerUpType.HeavyBall, out object originalSpeed))
        {
            CurrentBall.SetSpeed((float)originalSpeed);
            originalValues.Remove(PowerUpType.HeavyBall);
        }
        else
        {
            CurrentBall.ResetSpeed();
        }
        
        Debug.Log("[PowerUpManager] Heavy Ball deactivated");
    }
    
    private void ApplyGuidedMissileBall()
    {
        // TODO: Implement guided missile (ball seeks nearest brick)
        Debug.Log("[PowerUpManager] Guided Missile Ball activated");
    }
    
    private void RevertGuidedMissileBall()
    {
        Debug.Log("[PowerUpManager] Guided Missile Ball deactivated");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // WEAPONRY IMPLEMENTATIONS
    // ═══════════════════════════════════════════════════════════════
    
    private void ApplyLaserCannons()
    {
        // TODO: Implement laser cannons (shoot lasers from paddle)
        Debug.Log("[PowerUpManager] Laser Cannons activated");
    }
    
    private void RevertLaserCannons()
    {
        Debug.Log("[PowerUpManager] Laser Cannons deactivated");
    }
    
    private void ApplyExplosiveShrapnel()
    {
        // TODO: Implement explosive shrapnel (bricks explode on destruction)
        Debug.Log("[PowerUpManager] Explosive Shrapnel activated");
    }
    
    private void RevertExplosiveShrapnel()
    {
        Debug.Log("[PowerUpManager] Explosive Shrapnel deactivated");
    }
    
    private void ApplyEMPulse()
    {
        // TODO: Implement EMP (temporarily disables special bricks)
        Debug.Log("[PowerUpManager] EM Pulse activated");
    }
    
    private void RevertEMPulse()
    {
        Debug.Log("[PowerUpManager] EM Pulse deactivated");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY IMPLEMENTATIONS
    // ═══════════════════════════════════════════════════════════════
    
    private void ApplyTimeDilation()
    {
        // Slow down time
        Time.timeScale = 0.5f;
        
        Debug.Log("[PowerUpManager] Time Dilation activated");
    }
    
    private void RevertTimeDilation()
    {
        Time.timeScale = 1f;
        
        Debug.Log("[PowerUpManager] Time Dilation deactivated");
    }
    
    private void ApplyGravityWell()
    {
        // TODO: Implement gravity well (pulls items/power-ups to paddle)
        Debug.Log("[PowerUpManager] Gravity Well activated");
    }
    
    private void RevertGravityWell()
    {
        Debug.Log("[PowerUpManager] Gravity Well deactivated");
    }
    
    private void ApplyScoreMultiplier()
    {
        // TODO: Implement score multiplier (2x score for duration)
        Debug.Log("[PowerUpManager] Score Multiplier activated");
    }
    
    private void RevertScoreMultiplier()
    {
        Debug.Log("[PowerUpManager] Score Multiplier deactivated");
    }
    
    private void ApplyBallRadar()
    {
        // TODO: Implement ball radar (shows ball trajectory)
        Debug.Log("[PowerUpManager] Ball Radar activated");
    }
    
    private void RevertBallRadar()
    {
        Debug.Log("[PowerUpManager] Ball Radar deactivated");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════
    
    public bool IsPowerUpActive(PowerUpType type)
    {
        return activePowerUps.ContainsKey(type) && !activePowerUps[type].isRecharging;
    }
    
    public float GetRemainingTime(PowerUpType type)
    {
        if (activePowerUps.TryGetValue(type, out ActivePowerUp active))
        {
            if (!active.isRecharging)
            {
                return active.remainingTime;
            }
        }
        return 0f;
    }
    
    public void ClearPowerUp(PowerUpType type)
    {
        if (activePowerUps.TryGetValue(type, out ActivePowerUp active))
        {
            if (active.coroutine != null)
            {
                StopCoroutine(active.coroutine);
            }
            
            if (!active.isRecharging)
            {
                RevertPowerUpEffect(active.data);
            }
            
            activePowerUps.Remove(type);
        }
    }
    
    public void ClearAllPowerUps()
    {
        foreach (var kvp in activePowerUps)
        {
            if (kvp.Value.coroutine != null)
            {
                StopCoroutine(kvp.Value.coroutine);
            }
            
            if (!kvp.Value.isRecharging)
            {
                RevertPowerUpEffect(kvp.Value.data);
            }
        }
        
        activePowerUps.Clear();
        originalValues.Clear();
        
        Debug.Log("[PowerUpManager] All power-ups cleared");
    }
    
    public int GetActivePowerUpCount()
    {
        int count = 0;
        foreach (var kvp in activePowerUps)
        {
            if (!kvp.Value.isRecharging)
            {
                count++;
            }
        }
        return count;
    }
    
    public List<PowerUpType> GetActivePowerUpTypes()
    {
        List<PowerUpType> types = new List<PowerUpType>();
        foreach (var kvp in activePowerUps)
        {
            if (!kvp.Value.isRecharging)
            {
                types.Add(kvp.Key);
            }
        }
        return types;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG UI
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 500));
        GUILayout.Label($"=== Power-Ups (Level {playerLevel}) ===");
        GUILayout.Label($"Max Simultaneous: {MaxSimultaneousPowerUps}");
        GUILayout.Label("");
        
        GUILayout.Label("Active:");
        bool hasActive = false;
        foreach (var kvp in activePowerUps)
        {
            if (!kvp.Value.isRecharging)
            {
                GUILayout.Label($"  {kvp.Key}: {kvp.Value.remainingTime:F1}s");
                hasActive = true;
            }
        }
        if (!hasActive) GUILayout.Label("  None");
        
        GUILayout.Label("");
        GUILayout.Label("Recharging:");
        bool hasRecharging = false;
        foreach (var kvp in activePowerUps)
        {
            if (kvp.Value.isRecharging)
            {
                float remaining = GetRemainingCooldown(kvp.Key);
                GUILayout.Label($"  {kvp.Key}: {remaining:F1}s");
                hasRecharging = true;
            }
        }
        if (!hasRecharging) GUILayout.Label("  None");
        
        GUILayout.EndArea();
    }
}