using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryJobRowUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Transform slotsRoot;
    public DeliverySlotUI slotPrefab;
    public Button deliverButton;
    public TextMeshProUGUI statusText;

    JobOrder job;
    DeliveryPanelUI ownerPanel;
    readonly List<DeliverySlotUI> slots = new List<DeliverySlotUI>();

    public void Bind(DeliveryPanelUI owner, JobOrder order)
    {
        ownerPanel = owner;
        job = order;

        if (titleText)
            titleText.text = BuildTitle(order);

        BuildSlots();

        if (deliverButton != null)
        {
            deliverButton.onClick.RemoveAllListeners();
            deliverButton.onClick.AddListener(OnDeliverClicked);
        }

        if (statusText)
            statusText.text = "Load items then press Deliver.";
    }

    string BuildTitle(JobOrder order)
    {
        if (order == null) return "";

        string customer = order.customer.ToString();
        string combo = BuildCombo(order);
        return customer + " delivery - " + combo;
    }

    string BuildCombo(JobOrder order)
    {
        if (order.lines == null || order.lines.Count == 0)
            return "No items";

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < order.lines.Count; i++)
        {
            var line = order.lines[i];
            if (line == null) continue;

            if (sb.Length > 0)
                sb.Append(" + ");

            string name = line.product ? line.product.displayName : "Item";
            sb.Append(name);
            sb.Append(" x");
            sb.Append(line.quantity);
        }

        return sb.ToString();
    }

    void BuildSlots()
    {
        if (!slotsRoot || slotPrefab == null || job == null) return;

        for (int i = slotsRoot.childCount - 1; i >= 0; i--)
            Destroy(slotsRoot.GetChild(i).gameObject);

        slots.Clear();

        for (int i = 0; i < job.lines.Count; i++)
        {
            JobLine line = job.lines[i];
            if (line == null || line.product == null) continue;

            DeliverySlotUI slot = Instantiate(slotPrefab, slotsRoot);
            slot.Configure(line.product, line.quantity);
            slots.Add(slot);
        }
    }

    bool AreAllSlotsSatisfied()
    {
        if (slots.Count == 0) return false;

        bool allOk = true;
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsSatisfied())
                allOk = false;
        }
        return allOk;
    }

    void OnDeliverClicked()
    {
        if (job == null || ownerPanel == null) return;

        if (!AreAllSlotsSatisfied())
        {
            if (statusText)
                statusText.text = "Not enough items loaded.";

            Debug.Log("[DeliveryJobRowUI] Deliver blocked, not all slots full for job " + job.id);
            return;
        }

        bool ok = ownerPanel.TryDeliverJob(job);
        if (ok)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].ClearCountsOnly();
            }

            if (statusText)
                statusText.text = "Delivered.";
        }
        else
        {
            if (statusText)
                statusText.text = "Delivery failed, maybe time is up.";

            Debug.Log("[DeliveryJobRowUI] TryDeliverJob returned false for job " + job.id);
        }
    }

    public void RefundItems(StorageManager storage)
    {
        if (storage == null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.TargetItem != null && slot.DeliveredCount > 0)
            {
                storage.Put(slot.TargetItem, slot.DeliveredCount);
            }

            slot.ClearCountsOnly();
        }
    }
}
