using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorageShelfGridAnchored : MonoBehaviour, IRebuildRequester
{
    [Header("Which page is this?")]
    public ItemCategory category;            // Utility or FinishedProduct

    [Header("Plumbing")]
    public StorageManager storageManager;
    public GameObject shelfSlotPrefab;

    [Header("Anchors (place over the shelf art)")]
    public List<RectTransform> anchors = new(); // 8 anchors, left→right
    public bool stretchToAnchor = true;

    private readonly List<StorageShelfSlot> slots = new();
    private Coroutine rebuildCo;

    void OnEnable()
    {
        if (!storageManager) storageManager = FindFirstObjectByType<StorageManager>();
        BuildOrReuseSlots();
        Rebuild();
    }

    public void RequestRebuildSoon()
    {
        if (rebuildCo != null) return;
        rebuildCo = StartCoroutine(RebuildNextFrame());
    }
    private IEnumerator RebuildNextFrame() { yield return null; rebuildCo = null; Rebuild(); }

    private void BuildOrReuseSlots()
    {
        slots.Clear();

        for (int i = 0; i < anchors.Count; i++)
        {
            var anchor = anchors[i];
            if (!anchor) { Debug.LogWarning($"[StorageShelfGrid] Missing anchor {i}"); continue; }

            var slot = anchor.GetComponentInChildren<StorageShelfSlot>(true);
            if (!slot)
            {
                var go = Object.Instantiate(shelfSlotPrefab, anchor);
                go.name = $"ShelfSlot_{i + 1}";
                slot = go.GetComponent<StorageShelfSlot>();

                // fit to anchor rect
                var rt = go.GetComponent<RectTransform>();
                if (stretchToAnchor)
                {
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                }
                else
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                }
            }
            slots.Add(slot);
        }
    }

    public void Rebuild()
    {
        if (slots.Count == 0 || storageManager == null) return;

        // items of this category with count > 0
        var items = storageManager
            .AllItems()
            .Where(kv => kv.Key && kv.Value > 0 && kv.Key.category == category)
            .OrderBy(kv => kv.Key.displayName) // change if you want custom order
            .Select(kv => kv.Key)
            .ToList();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count) slots[i].Bind(items[i], this);
            else slots[i].ClearVisuals();
        }
    }
}
