using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public Inventory inventory;
    public JobRewardSO jobReward;
    public CustomerTypeSO customerType;

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public TextMeshProUGUI timerText;
    public float timeLimitSeconds = 180f;

    float deadlineTime;
    bool timerRunning;

    void Awake()
    {
        if (!inventory)
        {
            GameObject gm = GameObject.FindWithTag("GameController");
            if (gm)
                inventory = gm.GetComponent<Inventory>();
        }
    }

    public void Init(JobRewardSO job, CustomerTypeSO type, Inventory inv)
    {
        jobReward = job;
        customerType = type;
        inventory = inv ? inv : inventory;

        if (customerType != null)
        {
            if (nameText) nameText.text = customerType.displayName;
            if (portraitImage) portraitImage.sprite = customerType.portrait;
        }

        if (jobReward != null && jobReward.estimatedMinutes > 0)
            timeLimitSeconds = jobReward.estimatedMinutes * 60f;

        deadlineTime = Time.time + timeLimitSeconds;
        timerRunning = true;
    }

    void Update()
    {
        if (!timerRunning) return;

        float remaining = Mathf.Max(0f, deadlineTime - Time.time);

        if (timerText)
            timerText.text = Mathf.CeilToInt(remaining).ToString();

        if (remaining <= 0f)
        {
            timerRunning = false;
            Destroy(gameObject);
        }
    }

    public void CompleteOrder()
    {
        if (!timerRunning) return;
        if (!inventory) return;
        if (!jobReward) return;
        if (!customerType) return;

        float finalMoney = jobReward.moneyReward * customerType.moneyMultiplier;
        int finalXp = Mathf.RoundToInt(jobReward.xpReward * customerType.xpMultiplier);

        inventory.AddMoney(finalMoney);
        inventory.AddXp(finalXp);

        timerRunning = false;
        Destroy(gameObject);
    }
}
