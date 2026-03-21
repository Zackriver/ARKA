using UnityEngine;
using System.Collections.Generic;

public enum PowerUpCategory
{
    PaddleDefense,    // 🛡️ Uses Metal Shard + Energy Core
    BallOffensive,    // 💥 Uses Plasma Orb + Power Cell
    Weaponry,         // 🔫 Uses Tech Chip + Circuit Board
    Utility           // ⏱️ Uses Pulse Crystal + Gravity Stone
}

public enum PowerUpType
{
    // Paddle & Defense
    MagnetPaddle,
    WidePaddle,
    NanoShield,
    AgilityBoost,
    TwinPaddles,
    
    // Ball & Offensive
    MultiBall,
    Fireball,
    AcidBall,
    HeavyBall,
    GuidedMissileBall,
    
    // Weaponry
    LaserCannons,
    ExplosiveShrapnel,
    EMPulse,
    
    // Utility
    TimeDilation,
    GravityWell,
    ScoreMultiplier,
    BallRadar
}

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Breakout/Power-Up Data")]
public class PowerUpData : ScriptableObject
{
    [Header("Basic Info")]
    public string powerUpName;
    public PowerUpType powerUpType;
    public PowerUpCategory category;
    public Sprite icon;
    
    [TextArea(2, 4)]
    public string description;
    
    [Header("Recipe (4 Items Required)")]
    public ItemType item1;
    public ItemType item2;
    public ItemType item3;
    public ItemType item4;
    
    [Header("Timing")]
    [Tooltip("Base duration at level 0-1 (in seconds)")]
    public float baseDuration = 5f;
    
    [Tooltip("Recharge time after power-up ends (in seconds)")]
    public float rechargeTime = 55f;
    
    [Header("Level 50 Unlock")]
    [Tooltip("Can player stack multiple power-ups after level 50?")]
    public bool allowStacking = true;
    
    public float GetDuration(int level)
    {
        // Level 0-1 = base duration (5 seconds)
        // Higher levels = TBD (placeholder formula)
        if (level <= 1)
        {
            return baseDuration;
        }
        
        // Placeholder: increases slightly per level
        // You can adjust this formula later
        return baseDuration + (level - 1) * 0.5f;
    }
    
    public ItemType[] GetRecipe()
    {
        return new ItemType[] { item1, item2, item3, item4 };
    }
    
    public bool MatchesRecipe(ItemType[] items)
    {
        if (items == null || items.Length != 4) return false;
        
        ItemType[] recipe = GetRecipe();
        
        // Sort both arrays to compare
        System.Array.Sort(items);
        System.Array.Sort(recipe);
        
        for (int i = 0; i < 4; i++)
        {
            if (items[i] != recipe[i]) return false;
        }
        
        return true;
    }
}