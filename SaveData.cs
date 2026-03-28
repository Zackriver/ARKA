using UnityEngine;

/// <summary>
/// Data structure for save files
/// Place in: Assets/Scripts/Data/SaveData.cs
/// </summary>
[System.Serializable]
public class SaveData
{
    // ═══════════════════════════════════════════════════════════════
    // GAME PROGRESS
    // ═══════════════════════════════════════════════════════════════
    
    public int currentLevel = 1;
    public int playerScore = 0;
    public int playerLives = 3;
    public int highScore = 0;
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY
    // ═══════════════════════════════════════════════════════════════
    
    public PlayerInventory.InventorySaveData inventoryData;
    
    // ═══════════════════════════════════════════════════════════════
    // METADATA
    // ═══════════════════════════════════════════════════════════════
    
    public string saveTimestamp;
    public string gameVersion = "1.0.0";
    
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════
    
    public SaveData()
    {
        saveTimestamp = System.DateTime.Now.ToString();
    }
}