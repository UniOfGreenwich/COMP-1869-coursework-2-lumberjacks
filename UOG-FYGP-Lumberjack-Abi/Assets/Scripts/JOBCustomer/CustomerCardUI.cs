using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomerCardUI : MonoBehaviour
{
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI jobText;
    public TextMeshProUGUI rewardText;
    public Button acceptButton;

    JobOrder job;
    JobManager manager;

    public void Bind(JobOrder jobOrder, JobManager owner, Sprite portrait)
    {
        job = jobOrder;
        manager = owner;

        if (portraitImage) portraitImage.sprite = portrait;
        if (nameText) nameText.text = GetDisplayName(job.customer);
        if (jobText)
        {
            jobText.text = BuildJobSummary(job);
        }
        if (rewardText && manager != null)
        {
            int gold = manager.EstimateGold(job);
            rewardText.text = gold.ToString() + " gold";
        }
        if (acceptButton)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAcceptClicked);
        }
    }

    string GetDisplayName(CustomerKind kind)
    {
        switch (kind)
        {
            case CustomerKind.Charlie: return "Charlie";
            case CustomerKind.Gabby: return "Gabby";
            case CustomerKind.Sponge: return "Sponge";
            case CustomerKind.Brandon: return "Brandon";
            default: return kind.ToString();
        }
    }

    string BuildJobSummary(JobOrder order)
    {
        if (order == null || order.lines == null || order.lines.Count == 0)
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

    void OnAcceptClicked()
    {
        if (manager == null || job == null) return;
        manager.AcceptJob(job);
    }
}
