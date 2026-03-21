using UnityEngine;

public enum ItemRarity
{
    Common,      // 15% drop
    Uncommon,    // 8% drop
    Rare,        // 4% drop
    Epic,        // 1.5% drop
    Legendary    // 0.5% drop
}

public enum ItemType
{
    // Common
    MetalShard,
    PlasmaOrb,
    TechChip,
    
    // Uncommon
    EnergyCore,
    PowerCell,
    CircuitBoard,
    
    // Rare
    MagnetPiece,
    ShieldFragment,
    SpeedGem,
    
    // Epic
    FireEssence,
    AcidVial,
    GravityStone,
    
    // Legendary
    LaserLens,
    ExplosivePowder,
    PulseCrystal
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Breakout/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public ItemRarity rarity;
    public Sprite icon;
    public Color itemColor = Color.white;
    
    [TextArea(2, 4)]
    public string description;
    
    public float GetDropChance()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 0.15f;
            case ItemRarity.Uncommon: return 0.08f;
            case ItemRarity.Rare: return 0.04f;
            case ItemRarity.Epic: return 0.015f;
            case ItemRarity.Legendary: return 0.005f;
            default: return 0f;
        }
    }
    
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f); // Gray
            case ItemRarity.Uncommon: return new Color(0.2f, 0.9f, 0.2f); // Green
            case ItemRarity.Rare: return new Color(0.2f, 0.5f, 1f); // Blue
            case ItemRarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
            case ItemRarity.Legendary: return new Color(1f, 0.85f, 0f); // Gold
            default: return Color.white;
        }
    }
}