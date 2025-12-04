using System.Text;
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
        string combo = BuildCombo(order);
        int total = order.TotalQuantity;

        return customer + " - " + combo + " (" + total.ToString() + " items)";
    }

    string BuildCombo(JobOrder order)
    {
        if (order.lines == null || order.lines.Count == 0)
        {
            return "No items";
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < order.lines.Count; i++)
        {
            var line = order.lines[i];
            if (line == null) continue;

            if (sb.Length > 0)
            {
                sb.Append(" + ");
            }

            string name = line.product ? line.product.displayName : "Item";
            sb.Append(name);
            sb.Append(" x");
            sb.Append(line.quantity);
        }

        return sb.ToString();
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

            if (job.isFailed)
            {
                status = "Failed";
            }
            else if (job.isCompleted)
            {
                status = "Delivered";
            }
            else if (job.TotalQuantity > 0 && job.TotalProduced >= job.TotalQuantity)
            {
                status = "Ready";
            }

            statusText.text = status;
        }
    }
}
