using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ActivePowerUp
{
    public PowerUpType type;
    public PowerUpData data;
    public float remainingDuration;
    public int level;
    
    public ActivePowerUp(PowerUpData powerUpData, int powerUpLevel)
    {
        type = powerUpData.powerUpType;
        data = powerUpData;
        level = powerUpLevel;
        remainingDuration = powerUpData.GetDuration(powerUpLevel);
    }
}

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }
    
    [Header("Active Power-Ups")]
    public List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();
    
    [Header("References")]
    public Paddle paddle;
    public Ball ball;
    public GameManager gameManager;
    
    // Events
    public event Action<PowerUpType> OnPowerUpActivated;
    public event Action<PowerUpType> OnPowerUpDeactivated;
    public event Action<PowerUpType, float> OnPowerUpDurationChanged;
    public event Action<DebuffData> OnDebuffApplied;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        UpdateActivePowerUps();
        UpdateRechargeTimers();
    }
    
    private void UpdateActivePowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            activePowerUps[i].remainingDuration -= Time.deltaTime;
            OnPowerUpDurationChanged?.Invoke(activePowerUps[i].type, activePowerUps[i].remainingDuration);
            
            if (activePowerUps[i].remainingDuration <= 0f)
            {
                DeactivatePowerUp(activePowerUps[i]);
            }
        }
    }
    
    private void UpdateRechargeTimers()
    {
        PlayerInventory.Instance?.UpdateRechargeTimers(Time.deltaTime);
    }
    
    public bool HasActivePowerUp()
    {
        return activePowerUps.Count > 0;
    }
    
    public bool HasActivePowerUp(PowerUpType type)
    {
        foreach (var active in activePowerUps)
        {
            if (active.type == type) return true;
        }
        return false;
    }
    
    public bool TryActivatePowerUp(PowerUpData powerUpData)
    {
        if (powerUpData == null) return false;
        
        // Check if unlocked and ready
        if (!PlayerInventory.Instance.IsPowerUpUnlocked(powerUpData.powerUpType))
        {
            return false;
        }
        
        if (!PlayerInventory.Instance.IsPowerUpReady(powerUpData.powerUpType))
        {
            return false;
        }
        
        // Check if can use multiple power-ups
        if (activePowerUps.Count > 0 && !PlayerInventory.Instance.CanUseMultiplePowerUps())
        {
            // Deactivate current power-up first
            DeactivatePowerUp(activePowerUps[0]);
        }
        
        // Check if already active (same type)
        if (HasActivePowerUp(powerUpData.powerUpType))
        {
            return false;
        }
        
        // Activate
        int level = PlayerInventory.Instance.GetPowerUpLevel(powerUpData.powerUpType);
        ActivePowerUp newActive = new ActivePowerUp(powerUpData, level);
        activePowerUps.Add(newActive);
        
        ApplyPowerUpEffect(newActive);
        OnPowerUpActivated?.Invoke(powerUpData.powerUpType);
        
        return true;
    }
    
    private void DeactivatePowerUp(ActivePowerUp powerUp)
    {
        RemovePowerUpEffect(powerUp);
        activePowerUps.Remove(powerUp);
        
        // Start recharge
        PlayerInventory.Instance?.StartRecharge(powerUp.type, powerUp.data.rechargeTime);
        
        OnPowerUpDeactivated?.Invoke(powerUp.type);
    }
    
    public void ApplyDebuff(DebuffData debuff)
    {
        if (debuff == null || activePowerUps.Count == 0) return;
        
        OnDebuffApplied?.Invoke(debuff);
        
        switch (debuff.debuffType)
        {
            case DebuffType.RustFragment:
                // Cut Paddle power-up time by 50%
                ReduceDurationByCategory(PowerUpCategory.PaddleDefense, debuff.durationReduction);
                break;
                
            case DebuffType.VoidShard:
                // Cut Ball power-up time by 50%
                ReduceDurationByCategory(PowerUpCategory.BallOffensive, debuff.durationReduction);
                break;
                
            case DebuffType.GlitchChip:
                // Cut Weaponry power-up time by 50%
                ReduceDurationByCategory(PowerUpCategory.Weaponry, debuff.durationReduction);
                break;
                
            case DebuffType.TimeDrain:
                // Cut ALL by 25%
                ReduceAllDurations(debuff.durationReduction);
                break;
                
            case DebuffType.EnergyLeak:
                // Instantly end current power-up
                if (activePowerUps.Count > 0)
                {
                    DeactivatePowerUp(activePowerUps[0]);
                }
                break;
        }
    }
    
    private void ReduceDurationByCategory(PowerUpCategory category, float reduction)
    {
        foreach (var active in activePowerUps)
        {
            if (active.data.category == category)
            {
                active.remainingDuration *= (1f - reduction);
            }
        }
    }
    
    private void ReduceAllDurations(float reduction)
    {
        foreach (var active in activePowerUps)
        {
            active.remainingDuration *= (1f - reduction);
        }
    }
    
    // ==================== POWER-UP EFFECTS ====================
    
    private void ApplyPowerUpEffect(ActivePowerUp powerUp)
    {
        switch (powerUp.type)
        {
            // Paddle & Defense
            case PowerUpType.MagnetPaddle:
                ApplyMagnetPaddle(true);
                break;
            case PowerUpType.WidePaddle:
                ApplyWidePaddle(true, powerUp.level);
                break;
            case PowerUpType.NanoShield:
                ApplyNanoShield(true);
                break;
            case PowerUpType.AgilityBoost:
                ApplyAgilityBoost(true, powerUp.level);
                break;
            case PowerUpType.TwinPaddles:
                ApplyTwinPaddles(true);
                break;
                
            // Ball & Offensive
            case PowerUpType.MultiBall:
                ApplyMultiBall();
                break;
            case PowerUpType.Fireball:
                ApplyFireball(true);
                break;
            case PowerUpType.AcidBall:
                ApplyAcidBall(true);
                break;
            case PowerUpType.HeavyBall:
                ApplyHeavyBall(true);
                break;
            case PowerUpType.GuidedMissileBall:
                ApplyGuidedMissile(true);
                break;
                
            // Weaponry
            case PowerUpType.LaserCannons:
                ApplyLaserCannons(true);
                break;
            case PowerUpType.ExplosiveShrapnel:
                ApplyExplosiveShrapnel(true);
                break;
            case PowerUpType.EMPulse:
                ApplyEMPulse();
                break;
                
            // Utility
            case PowerUpType.TimeDilation:
                ApplyTimeDilation(true);
                break;
            case PowerUpType.GravityWell:
                ApplyGravityWell(true);
                break;
            case PowerUpType.ScoreMultiplier:
                ApplyScoreMultiplier(true);
                break;
            case PowerUpType.BallRadar:
                ApplyBallRadar(true);
                break;
        }
    }
    
    private void RemovePowerUpEffect(ActivePowerUp powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.MagnetPaddle:
                ApplyMagnetPaddle(false);
                break;
            case PowerUpType.WidePaddle:
                ApplyWidePaddle(false, powerUp.level);
                break;
            case PowerUpType.NanoShield:
                ApplyNanoShield(false);
                break;
            case PowerUpType.AgilityBoost:
                ApplyAgilityBoost(false, powerUp.level);
                break;
            case PowerUpType.TwinPaddles:
                ApplyTwinPaddles(false);
                break;
            case PowerUpType.Fireball:
                ApplyFireball(false);
                break;
            case PowerUpType.AcidBall:
                ApplyAcidBall(false);
                break;
            case PowerUpType.HeavyBall:
                ApplyHeavyBall(false);
                break;
            case PowerUpType.GuidedMissileBall:
                ApplyGuidedMissile(false);
                break;
            case PowerUpType.LaserCannons:
                ApplyLaserCannons(false);
                break;
            case PowerUpType.ExplosiveShrapnel:
                ApplyExplosiveShrapnel(false);
                break;
            case PowerUpType.TimeDilation:
                ApplyTimeDilation(false);
                break;
            case PowerUpType.GravityWell:
                ApplyGravityWell(false);
                break;
            case PowerUpType.ScoreMultiplier:
                ApplyScoreMultiplier(false);
                break;
            case PowerUpType.BallRadar:
                ApplyBallRadar(false);
                break;
        }
    }
    
    // ==================== INDIVIDUAL EFFECTS (Implement these) ====================
    
    private void ApplyMagnetPaddle(bool active)
    {
        // TODO: Implement magnet paddle - ball sticks to paddle
        if (paddle != null)
        {
            // paddle.SetMagnetMode(active);
        }
    }
    
    private void ApplyWidePaddle(bool active, int level)
    {
        if (paddle != null)
        {
            float scale = active ? 1f + (0.5f * level) : 1f; // 1.5x at level 1, 2x at level 2, etc.
            paddle.transform.localScale = new Vector3(scale, 1f, 1f);
        }
    }
    
    private void ApplyNanoShield(bool active)
    {
        // TODO: Create temporary floor barrier
    }
    
    private void ApplyAgilityBoost(bool active, int level)
    {
        if (paddle != null)
        {
            Paddle paddleScript = paddle.GetComponent<Paddle>();
            if (paddleScript != null)
            {
                paddleScript.speed = active ? 15f + (5f * level) : 15f;
            }
        }
    }
    
    private void ApplyTwinPaddles(bool active)
    {
        // TODO: Spawn second paddle
    }
    
    private void ApplyMultiBall()
    {
        // TODO: Split ball into multiple
    }
    
    private void ApplyFireball(bool active)
    {
        // TODO: Ball passes through bricks
        if (ball != null)
        {
            // ball.SetFireballMode(active);
        }
    }
    
    private void ApplyAcidBall(bool active)
    {
        // TODO: Ball dissolves neighboring bricks
    }
    
    private void ApplyHeavyBall(bool active)
    {
        // TODO: Ball destroys hard bricks in one hit
    }
    
    private void ApplyGuidedMissile(bool active)
    {
        // TODO: Ball curves toward nearest brick
    }
    
    private void ApplyLaserCannons(bool active)
    {
        // TODO: Paddle shoots lasers
    }
    
    private void ApplyExplosiveShrapnel(bool active)
    {
        // TODO: Bricks explode and damage neighbors
    }
    
    private void ApplyEMPulse()
    {
        // TODO: One-time blast around paddle
    }
    
    private void ApplyTimeDilation(bool active)
    {
        Time.timeScale = active ? 0.5f : 1f;
    }
    
    private void ApplyGravityWell(bool active)
    {
        // TODO: Items pulled toward paddle
    }
    
    private void ApplyScoreMultiplier(bool active)
    {
        // TODO: Double score
    }
    
    private void ApplyBallRadar(bool active)
    {
        // TODO: Show ball trajectory
    }
    
    // ==================== PUBLIC HELPERS ====================
    
    public float GetRemainingDuration(PowerUpType type)
    {
        foreach (var active in activePowerUps)
        {
            if (active.type == type)
            {
                return active.remainingDuration;
            }
        }
        return 0f;
    }
    
    public void DeactivateAllPowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            DeactivatePowerUp(activePowerUps[i]);
        }
    }
}