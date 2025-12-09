using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryPanelUI : MonoBehaviour
{
    public JobManager jobManager;
    public RectTransform jobListRoot;
    public GameObject jobRowPrefab;
    public DeliverySlotUI slotPrefab;
    public Button closeButton;
    public TextMeshProUGUI globalStatusText;

    readonly List<DeliveryJobRowUI> rows = new List<DeliveryJobRowUI>();

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
        PlayerController.IsInputLocked = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        PlayerController.IsInputLocked = false;
    }

    public void Refresh()
    {
        if (!jobManager || !jobListRoot || !jobRowPrefab || !slotPrefab) return;

        for (int i = jobListRoot.childCount - 1; i >= 0; i--)
            Destroy(jobListRoot.GetChild(i).gameObject);

        rows.Clear();

        var jobs = jobManager.ActiveJobs;
        for (int i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            if (job == null) continue;
            if (!job.isAccepted || job.isCompleted || job.isFailed) continue;
            if (!job.isReadyForDelivery) continue;

            GameObject rowGO = Instantiate(jobRowPrefab, jobListRoot);
            DeliveryJobRowUI row = rowGO.GetComponent<DeliveryJobRowUI>();
            if (row == null) row = rowGO.AddComponent<DeliveryJobRowUI>();

            row.Bind(this, job, slotPrefab);
            rows.Add(row);
        }

        if (globalStatusText != null)
        {
            if (rows.Count == 0)
                globalStatusText.text = "No finished jobs ready for delivery.";
            else
                globalStatusText.text = "";
        }
    }

    public void TryDeliver(JobOrder job, DeliveryJobRowUI row)
    {
        if (jobManager == null || job == null || row == null) return;

        DeliverySlotUI[] slots = row.GetSlots();
        bool allOk = true;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (!slots[i].IsSatisfied())
                allOk = false;
        }

        if (!allOk)
        {
            string msg = "Not enough items loaded.";
            row.SetStatus(msg);
            if (globalStatusText != null) globalStatusText.text = msg;
            return;
        }

        row.SetStatus("Delivered.");
        if (globalStatusText != null) globalStatusText.text = "";

        jobManager.DeliverJob(job);

        Refresh();
    }
}
