using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeliverySlotUI : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI labelText;
    public Image outlineImage;

    public ItemSO TargetItem { get; private set; }
    public int RequiredQuantity { get; private set; }
    public int DeliveredCount { get; private set; }

    Color baseOutlineColor;
    Coroutine flashRoutine;

    void Awake()
    {
        if (outlineImage != null)
            baseOutlineColor = outlineImage.color;

        RefreshLabel();
    }

    public void Configure(ItemSO item, int quantity)
    {
        TargetItem = item;
        RequiredQuantity = Mathf.Max(1, quantity);
        DeliveredCount = 0;
        RefreshLabel();
        ResetVisual();
    }

    public void RefreshLabel()
    {
        if (!labelText) return;

        string name = TargetItem ? TargetItem.displayName : "Item";
        labelText.text = name + " " + DeliveredCount + "/" + RequiredQuantity;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (drag == null) return;

        NoOfItems payload = drag.TakePayload();
        if (payload.IsEmpty)
        {
            drag.ReturnRemainder(payload);
            return;
        }

        ItemSO item = payload.item;

        if (item != TargetItem)
        {
            Debug.Log("[DeliverySlotUI] Rejected item " +
                      (item ? item.displayName : "null") +
                      " expected " +
                      (TargetItem ? TargetItem.displayName : "none"));

            drag.ReturnRemainder(payload);
            Flash(false);
            return;
        }

        int remaining = Mathf.Max(0, RequiredQuantity - DeliveredCount);
        if (remaining <= 0)
        {
            Debug.Log("[DeliverySlotUI] Slot already full for " + TargetItem.displayName);
            drag.ReturnRemainder(payload);
            Flash(true);
            return;
        }

        int taken = Mathf.Min(remaining, payload.count);
        DeliveredCount += taken;
        payload.count -= taken;

        Debug.Log("[DeliverySlotUI] Loaded " + taken + " x " +
                  TargetItem.displayName + " now " +
                  DeliveredCount + "/" + RequiredQuantity);

        RefreshLabel();
        drag.ReturnRemainder(payload);
        Flash(true);
    }

    public bool IsSatisfied()
    {
        return DeliveredCount >= RequiredQuantity;
    }

    public void ClearCountsOnly()
    {
        DeliveredCount = 0;
        RefreshLabel();
        ResetVisual();
    }

    public void ResetVisual()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (outlineImage != null)
            outlineImage.color = baseOutlineColor;
    }

    void Flash(bool ok)
    {
        if (outlineImage == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        Color target = ok ? Color.green : Color.red;
        flashRoutine = StartCoroutine(FlashOutline(target));
    }

    IEnumerator FlashOutline(Color target)
    {
        outlineImage.color = target;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            outlineImage.color = Color.Lerp(target, baseOutlineColor, t);
            yield return null;
        }

        outlineImage.color = baseOutlineColor;
        flashRoutine = null;
    }
}
