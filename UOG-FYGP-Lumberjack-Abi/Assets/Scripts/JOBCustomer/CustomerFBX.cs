using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class CustomerFBX : MonoBehaviour
{
    public TextMeshPro worldText;
    public CustomerCardUI orderPopup;

    JobManager jobManager;
    JobOrder job;

    public void Setup(JobManager manager, JobOrder order)
    {
        jobManager = manager;
        job = order;

        if (worldText != null)
        {
            worldText.text = BuildLabel(order);
        }

        if (orderPopup == null)
        {
            orderPopup = FindFirstObjectByType<CustomerCardUI>();
        }
    }

    string BuildLabel(JobOrder order)
    {
        if (order == null)
        {
            return "Order";
        }

        int total = order.TotalQuantity;
        return "Order\n" + total.ToString() + " items";
    }

    void OnMouseDown()
    {
        if (jobManager == null || job == null) return;

        if (orderPopup == null)
        {
            orderPopup = FindFirstObjectByType<CustomerCardUI>();
        }

        if (orderPopup != null)
        {
            orderPopup.Show(jobManager, job, this);
        }
    }
}
