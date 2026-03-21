using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BrickData
{
    public Vector2 position;
    public int health;
    public Color color;
    public Vector3 brickScale = new Vector3(0.8f, 0.4f, 1f);
    public BrickShape shape; // Can be null for default square
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "Breakout/Level Data")]
public class LevelData : ScriptableObject
{
    public List<BrickData> bricks = new List<BrickData>();

    public void AddBrick(Vector2 pos, int hp, Color col, Vector3 scale, BrickShape shape = null)
    {
        bricks.Add(new BrickData
        {
            position = pos,
            health = hp,
            color = col,
            brickScale = scale,
            shape = shape
        });
    }
}