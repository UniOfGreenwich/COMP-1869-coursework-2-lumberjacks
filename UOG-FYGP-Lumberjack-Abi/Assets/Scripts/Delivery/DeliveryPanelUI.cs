using UnityEngine;
using UnityEngine.UI;

public class DeliveryPanelUI : MonoBehaviour
{
    [Header("Refs")]
    public JobManager jobManager;
    public RectTransform jobListRoot;
    public DeliveryJobRowUI jobRowPrefab;
    public Button closeButton;
    public StorageManager storage;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (!storage)
            storage = FindFirstObjectByType<StorageManager>();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RebuildJobs();
        PlayerController.IsInputLocked = true;
    }

    public void Close()
    {
        RefundAllRows();
        gameObject.SetActive(false);
        PlayerController.IsInputLocked = false;
    }

    void RefundAllRows()
    {
        if (!jobListRoot || !storage) return;

        for (int i = 0; i < jobListRoot.childCount; i++)
        {
            var row = jobListRoot.GetChild(i).GetComponent<DeliveryJobRowUI>();
            if (row != null)
            {
                row.RefundItems(storage);
            }
        }
    }

    public void RebuildJobs()
    {
        if (!jobManager || !jobListRoot || !jobRowPrefab) return;

        RefundAllRows();

        for (int i = jobListRoot.childCount - 1; i >= 0; i--)
            Destroy(jobListRoot.GetChild(i).gameObject);

        var jobs = jobManager.ActiveJobs;
        int built = 0;

        for (int i = 0; i < jobs.Count; i++)
        {
            JobOrder job = jobs[i];
            if (job == null) continue;
            if (!job.isAccepted) continue;
            if (job.isFailed) continue;
            if (job.isCompleted) continue;
            if (job.TotalQuantity <= 0) continue;
            if (job.TotalProduced < job.TotalQuantity) continue;

            var rowObj = Instantiate(jobRowPrefab.gameObject, jobListRoot);
            var row = rowObj.GetComponent<DeliveryJobRowUI>();
            row.Bind(this, job);
            built++;
        }

        Debug.Log("[DeliveryPanelUI] Rebuilt delivery list, rows=" + built);
    }

    public bool TryDeliverJob(JobOrder job)
    {
        if (!jobManager)
        {
            Debug.LogWarning("[DeliveryPanelUI] No JobManager, cannot deliver.");
            return false;
        }

        bool ok = jobManager.TryClaimRewards(job);
        if (ok)
        {
            Debug.Log("[DeliveryPanelUI] Delivery confirmed for job " + job.id);
            RebuildJobs();
        }
        else
        {
            Debug.Log("[DeliveryPanelUI] Delivery blocked for job " + job.id);
        }

        return ok;
    }
}
