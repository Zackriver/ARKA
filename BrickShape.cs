using UnityEngine;

[CreateAssetMenu(fileName = "NewBrickShape", menuName = "Breakout/Brick Shape")]
public class BrickShape : ScriptableObject
{
    public string shapeName = "Square";
    public Sprite sprite;
    public Vector3 defaultScale = new Vector3(0.8f, 0.4f, 1f);

    [Tooltip("Leave empty to use default brick prefab with swapped sprite. " +
             "Assign a custom prefab for shapes that need different colliders.")]
    public GameObject customPrefab;
}