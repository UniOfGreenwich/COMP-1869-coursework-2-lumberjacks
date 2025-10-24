using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StorageShelfSlot : MonoBehaviour, IItemSource, IDropHandler
{
    [Header("UI (auto-found if children are named Icon/Name/Count)")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;

    private StorageManager storage;
    private IRebuildRequester owner;
    private ItemSO item;

    void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        if (!icon) icon = transform.Find("Icon") ? transform.Find("Icon").GetComponent<Image>() : null;
        if (!nameText) nameText = transform.Find("Name") ? transform.Find("Name").GetComponent<TextMeshProUGUI>() : null;
        if (!countText) countText = transform.Find("Count") ? transform.Find("Count").GetComponent<TextMeshProUGUI>() : null;
    }

    public void Bind(ItemSO i, IRebuildRequester gridOwner)
    {
        owner = gridOwner;
        item = i;

        if (item != null)
        {
            if (icon) { icon.enabled = true; icon.sprite = item.icon; if (!icon.GetComponent<DraggableItemUI>()) icon.gameObject.AddComponent<DraggableItemUI>(); }
            if (nameText) { nameText.gameObject.SetActive(true); nameText.text = item.displayName; }
            RefreshCount();
            enabled = true;
        }
        else ClearVisuals();
    }

    public void ClearVisuals()
    {
        item = null;
        if (icon) { icon.enabled = false; icon.sprite = null; }
        if (nameText) { nameText.text = ""; nameText.gameObject.SetActive(false); }
        if (countText) countText.text = "";
        enabled = false;
    }

    private void RefreshCount()
    {
        if (!countText || item == null || storage == null) return;
        countText.text = storage.GetCount(item).ToString();
    }

    // Drag OUT of shelf
    public NoOfItems TakeAll()
    {
        if (item == null || storage == null) return default;
        int have = storage.GetCount(item);
        if (have <= 0) return default;

        int taken = storage.Take(item, have);
        RefreshCount();
        owner?.RequestRebuildSoon();
        return new NoOfItems { item = item, count = taken };
    }

    // If drop fails / leftovers
    public void PutBack(NoOfItems stack)
    {
        if (stack.IsEmpty || storage == null) return;
        storage.Put(stack.item, stack.count);
        owner?.RequestRebuildSoon();
    }

    // Drop ONTO shelf → return to storage (any item type)
    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (!drag) return;

        var payload = drag.TakePayload();
        if (!payload.IsEmpty)
        {
            storage.Put(payload.item, payload.count);
            payload.Clear();
            owner?.RequestRebuildSoon();
        }
        drag.ReturnRemainder(payload);
    }
}

public interface IRebuildRequester { void RequestRebuildSoon(); }
