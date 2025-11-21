using UnityEngine;

public class Customer : MonoBehaviour
{
    public Inventory inventory;
    public JobRewardSO jobReward;
    public CustomerTypeSO customerType;

    void Awake()
    {
        if (!inventory)
        {
            GameObject gm = GameObject.FindWithTag("GameController");
            if (gm)
                inventory = gm.GetComponent<Inventory>();
        }
    }

    public void CompleteOrder()
    {
        if (!inventory) return;
        if (!jobReward) return;
        if (!customerType) return;

        float finalMoney = jobReward.moneyReward * customerType.moneyMultiplier;
        int finalXp = Mathf.RoundToInt(jobReward.xpReward * customerType.xpMultiplier);

        inventory.AddMoney(finalMoney);
        inventory.AddXp(finalXp);
    }
}
