using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductioSlotUI : MonoBehaviour, IDropHandler
{
    public string slotId;
    public TextMeshProUGUI labelText;
    public Image outlineImage;
    public Image pieceIcon;

    public ItemSO CurrentItem { get; private set; }

    private StorageManager storage;

    void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        ClearPiece();
    }

    public void Configure(ProductionRecipeSO.SlotRequirement requirement)
    {
        slotId = requirement.slotId;
        if (labelText) labelText.text = requirement.label;
        ClearPiece();
    }

    public void ClearPiece()
    {
        CurrentItem = null;
        if (pieceIcon)
        {
            pieceIcon.enabled = false;
            pieceIcon.sprite = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (drag == null) return;

        var payload = drag.TakePayload();
        if (payload.IsEmpty)
        {
            drag.ReturnRemainder(payload);
            return;
        }

        var item = payload.item;
        int useCount = Mathf.Min(1, payload.count);
        payload.count -= useCount;
        drag.ReturnRemainder(payload);

        if (CurrentItem != null && storage != null)
        {
            storage.Put(CurrentItem, 1);
        }

        CurrentItem = item;
        if (pieceIcon)
        {
            pieceIcon.enabled = true;
            pieceIcon.sprite = item.icon;
        }
    }
}
