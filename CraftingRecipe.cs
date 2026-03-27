using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<ItemData> requiredItems;
    public ItemData resultItem;

    public bool Matches(List<ItemData> inputs)
    {
        if (inputs.Count != requiredItems.Count) return false;

        // Check if all required items are present (simple version)
        List<ItemData> checkList = new List<ItemData>(inputs);
        foreach (var req in requiredItems)
        {
            if (checkList.Contains(req))
                checkList.Remove(req);
            else
                return false;
        }
        return true;
    }
}