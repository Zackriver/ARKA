using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CraftingManager : MonoBehaviour
{
    public ItemSlotUI[] craftingSlots; 
    public ItemSlotUI resultSlot;      
    public ItemSlotUI[] materialSlots; 
    public TextMeshProUGUI descriptionBox; 
    public List<CraftingRecipe> recipes;

    public void ShowDescription(ItemData data) {
        if (descriptionBox == null) return;
        descriptionBox.richText = true;
        if (data != null) {
            // Fixed Rich Text tags
            descriptionBox.text = $"<color=#00FFFF><size=140%>{data.itemName}</size></color>\n\n{data.description}";
        } else {
            descriptionBox.text = "Awaiting item scan...";
        }
    }

    public void CheckRecipe() {
        List<ItemData> currentInputs = new List<ItemData>();
        foreach (var slot in craftingSlots) {
            if (slot.itemData != null) currentInputs.Add(slot.itemData);
        }

        foreach (var recipe in recipes) {
            if (recipe.Matches(currentInputs)) {
                resultSlot.SetItem(recipe.resultItem, 1);
                return;
            }
        }
        resultSlot.ClearSlot();
    }

    public void OnCraftButtonPressed() {
        if (resultSlot.itemData == null) return;
        
        ItemData resultItem = resultSlot.itemData;
        bool added = false;

        // Try to add to an existing stack in the Mother Slots
        foreach (var slot in materialSlots) {
            if (slot.itemData == resultItem) {
                slot.SetItem(resultItem, slot.quantity + 1);
                added = true;
                break;
            }
        }

        // If no existing stack, find the first empty Mother Slot
        if (!added) {
            foreach (var slot in materialSlots) {
                if (slot.itemData == null) {
                    slot.SetItem(resultItem, 1);
                    added = true;
                    break;
                }
            }
        }

        if (added) {
            // Consume ingredients and clear result
            foreach (var slot in craftingSlots) slot.RemoveItem(1);
            resultSlot.ClearSlot();
        }
    }
}