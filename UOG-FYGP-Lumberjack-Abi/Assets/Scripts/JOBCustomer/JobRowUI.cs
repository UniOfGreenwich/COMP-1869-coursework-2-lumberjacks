using UnityEngine;
using TMPro;

public class JobRowUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI starText;
    public TextMeshProUGUI statusText;

    JobOrder job;

    public void Bind(JobOrder order)
    {
        job = order;
        if (job == null) return;

        if (titleText)
        {
            titleText.text = BuildTitle(job);
        }
    }

    string BuildTitle(JobOrder order)
    {
        if (order == null) return "";

        string customer = order.customer.ToString();
        string combo = BuildComboShort(order);
        int total = order.TotalQuantity;

        return customer + " - " + combo + " (" + total.ToString() + " items)";
    }

    string BuildComboShort(JobOrder order)
    {
        if (order.lines == null || order.lines.Count == 0)
        {
            return "No items";
        }

        if (order.lines.Count == 1)
        {
            var line = order.lines[0];
            string name = line.product ? line.product.displayName : "Item";
            return name + " x" + line.quantity.ToString();
        }

        if (order.lines.Count == 2)
        {
            var l0 = order.lines[0];
            var l1 = order.lines[1];

            string n0 = l0.product ? l0.product.displayName : "Item";
            string n1 = l1.product ? l1.product.displayName : "Item";

            return n0 + " x" + l0.quantity.ToString() +
                   " + " +
                   n1 + " x" + l1.quantity.ToString();
        }

        var first = order.lines[0];
        var second = order.lines[1];

        string fn = first.product ? first.product.displayName : "Item";
        string sn = second.product ? second.product.displayName : "Item";

        int more = order.lines.Count - 2;

        return fn + " x" + first.quantity.ToString() +
               " + " +
               sn + " x" + second.quantity.ToString() +
               " +" + more.ToString() + " more";
    }

    void Update()
    {
        if (job == null) return;

        if (timerText)
        {
            if (!job.isAccepted || job.isCompleted || job.isFailed)
            {
                timerText.text = "--:--";
            }
            else
            {
                float t = job.RemainingSeconds;
                int seconds = Mathf.CeilToInt(t);
                int m = seconds / 60;
                int s = seconds % 60;
                timerText.text = m.ToString("00") + ":" + s.ToString("00");
            }
        }

        if (starText)
        {
            starText.text = job.StarValue.ToString("0.0") + " / 3";
        }

        if (statusText)
        {
            string status = "Active";
            if (job.isCompleted) status = "Completed";
            else if (job.isFailed) status = "Failed";
            statusText.text = status;
        }
    }
}
