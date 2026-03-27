using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Image iconImage;
    public ItemData itemData;
    [HideInInspector] public ItemSlotUI sourceSlot;
    [HideInInspector] public bool wasDroppedSuccessfully = false;

    private void Awake() {
        // Blocks raycasts so we can "see" the slots underneath the ghost
        if (GetComponent<CanvasGroup>() != null) 
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // Requirement #2: If drop failed, make icon reappear in mother slot
        if (!wasDroppedSuccessfully) {
            sourceSlot.UpdateSlotDisplay();
        }
        Destroy(gameObject);
    }
}