using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeliverySlotUI : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI labelText;
    public Image outlineImage;
    public Transform itemIconRoot;
    public Image itemIconPrefab;

    public ItemSO TargetItem { get; private set; }
    public int RequiredQuantity { get; private set; }
    public int DeliveredCount { get; private set; }

    Color baseOutlineColor;
    Coroutine flashRoutine;
    Image currentIcon;

    void Awake()
    {
        if (outlineImage != null)
            baseOutlineColor = outlineImage.color;

        RefreshLabel();
        UpdateVisualIcon();
    }

    public void Configure(ItemSO item, int quantity)
    {
        TargetItem = item;
        RequiredQuantity = Mathf.Max(1, quantity);
        DeliveredCount = 0;
        RefreshLabel();
        ResetVisual();
        UpdateVisualIcon();
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
            drag.ReturnRemainder(payload);
            Flash(false);
            return;
        }

        int remaining = Mathf.Max(0, RequiredQuantity - DeliveredCount);
        if (remaining <= 0)
        {
            drag.ReturnRemainder(payload);
            Flash(true);
            return;
        }

        int taken = Mathf.Min(remaining, payload.count);
        DeliveredCount += taken;
        payload.count -= taken;

        RefreshLabel();
        UpdateVisualIcon();
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
        UpdateVisualIcon();
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

    void ClearIcon()
    {
        if (currentIcon != null)
        {
            Destroy(currentIcon.gameObject);
            currentIcon = null;
        }
    }

    void UpdateVisualIcon()
    {
        if (!itemIconRoot || !itemIconPrefab || TargetItem == null || DeliveredCount <= 0)
        {
            ClearIcon();
            return;
        }

        if (currentIcon == null)
        {
            currentIcon = Instantiate(itemIconPrefab, itemIconRoot);
            currentIcon.preserveAspect = true;
        }

        currentIcon.sprite = TargetItem.icon;

        float t = Mathf.Clamp01(DeliveredCount / (float)RequiredQuantity);
        Color c = currentIcon.color;
        c.a = Mathf.Lerp(0.3f, 1f, t);
        currentIcon.color = c;
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
