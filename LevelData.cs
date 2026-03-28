using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Level configuration data with brick list
/// Place in: Assets/Scripts/Data/LevelData.cs
/// </summary>
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game Data/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string levelName = "New Level";
    
    [Header("Bricks")]
    public List<BrickData> bricks = new List<BrickData>();
    
    [Header("Brick Settings")]
    public BrickShape[] availableShapes;
    
    [Header("Difficulty")]
    [Range(0, 10)]
    public int difficulty = 1;
    
    // ═══════════════════════════════════════════════════════════════
    // BRICK MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    public void AddBrick(Vector2 position, int health, Color color, Vector3 scale, BrickShape shape = null)
    {
        BrickData newBrick = new BrickData(position, health, color, scale, shape);
        bricks.Add(newBrick);
    }
    
    public void RemoveBrick(BrickData brick)
    {
        bricks.Remove(brick);
    }
    
    public void ClearBricks()
    {
        bricks.Clear();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════
    
    public bool IsValid()
    {
        if (bricks == null)
        {
            Debug.LogError($"[LevelData] {name}: Bricks list is null!");
            return false;
        }
        
        return true;
    }
    
    public int GetBrickCount()
    {
        return bricks != null ? bricks.Count : 0;
    }
}