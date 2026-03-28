using UnityEngine;

/// <summary>
/// Defines custom brick shapes for level designer
/// Place in: Assets/Scripts/Data/BrickShape.cs
/// </summary>
[CreateAssetMenu(fileName = "NewBrickShape", menuName = "Game Data/Brick Shape")]
public class BrickShape : ScriptableObject
{
    [Header("Shape Info")]
    public string shapeName = "Custom Brick";
    
    [Header("Visual")]
    public Sprite sprite;
    public Vector2 size = new Vector2(1f, 0.5f);
    
    [Header("Physics")]
    public bool useCustomCollider = false;
    public Vector2 colliderSize = new Vector2(1f, 0.5f);
    public Vector2 colliderOffset = Vector2.zero;
}