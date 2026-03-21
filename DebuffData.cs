using UnityEngine;

public enum DebuffType
{
    RustFragment,    // Cuts Paddle power-up time by 50%
    VoidShard,       // Cuts Ball power-up time by 50%
    GlitchChip,      // Cuts Weaponry power-up time by 50%
    TimeDrain,       // Cuts ALL active power-ups by 25%
    EnergyLeak       // Instantly ends current power-up
}

[CreateAssetMenu(fileName = "NewDebuff", menuName = "Breakout/Debuff Data")]
public class DebuffData : ScriptableObject
{
    public string debuffName;
    public DebuffType debuffType;
    public Sprite icon;
    public Color debuffColor = Color.red;
    
    [TextArea(2, 4)]
    public string description;
    
    [Range(0f, 1f)]
    [Tooltip("How much to reduce duration (0.5 = 50% reduction)")]
    public float durationReduction = 0.5f;
    
    [Tooltip("Only for EnergyLeak - instantly ends power-up")]
    public bool instantEnd = false;
    
    [Tooltip("Drop chance when power-up is active (5% = 0.05)")]
    public float dropChance = 0.05f;
}