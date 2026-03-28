using UnityEngine;

/// <summary>
/// Data structure for individual brick in level
/// Place in: Assets/Scripts/Data/BrickData.cs
/// </summary>
[System.Serializable]
public class BrickData
{
    public Vector2 position;
    public int health = 1;
    public Color color = Color.white;
    public Vector3 brickScale = Vector3.one;
    public BrickShape shape;
    
    public BrickData()
    {
        position = Vector2.zero;
        health = 1;
        color = Color.white;
        brickScale = Vector3.one;
        shape = null;
    }
    
    public BrickData(Vector2 pos, int hp, Color col, Vector3 scale, BrickShape brickShape = null)
    {
        position = pos;
        health = hp;
        color = col;
        brickScale = scale;
        shape = brickShape;
    }
}