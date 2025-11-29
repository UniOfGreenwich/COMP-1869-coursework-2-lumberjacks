using UnityEngine;

public class CustomerSpawnUI : MonoBehaviour
{
    public JobManager jobManager;
    public RectTransform container;
    public GameObject cardPrefab;
    public Sprite charlieSprite;
    public Sprite gabbySprite;
    public Sprite spongeSprite;
    public Sprite brandonSprite;

    public void Refresh()
    {
        if (!jobManager || !container || !cardPrefab) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        var jobs = jobManager.AvailableJobs;
        for (int i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            var go = Instantiate(cardPrefab, container);
            var card = go.GetComponent<CustomerCardUI>();
            if (!card) card = go.AddComponent<CustomerCardUI>();
            card.Bind(job, jobManager, GetSprite(job.customer));
        }
    }

    Sprite GetSprite(CustomerKind kind)
    {
        switch (kind)
        {
            case CustomerKind.Charlie: return charlieSprite;
            case CustomerKind.Gabby: return gabbySprite;
            case CustomerKind.Sponge: return spongeSprite;
            case CustomerKind.Brandon: return brandonSprite;
            default: return null;
        }
    }
}
