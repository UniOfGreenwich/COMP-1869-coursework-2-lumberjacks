using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public float money { get; set; } = 5000f;
    public int xp { get; private set; }
    public int lumber { get; set; }
    public int gold { get; set; }
    public int copper { get; set; }

    [SerializeField] TMP_Text moneyUI;
    [SerializeField] TMP_Text lumberUI;
    [SerializeField] TMP_Text xpUI;

    void Awake()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (moneyUI)
            moneyUI.text = Mathf.RoundToInt(money).ToString();
        if (lumberUI)
            lumberUI.text = lumber.ToString();
        if (xpUI)
            xpUI.text = xp.ToString();
    }

    public void AddMoney(float amount)
    {
        if (amount <= 0f) return;
        money += amount;
        RefreshUI();
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f) return true;
        if (money < amount) return false;
        money -= amount;
        RefreshUI();
        return true;
    }

    public void AddXp(int amount)
    {
        if (amount <= 0) return;
        xp += amount;
        RefreshUI();
    }

    public void ChangeLumber(int delta)
    {
        lumber = Mathf.Max(0, lumber + delta);
        RefreshUI();
    }
}
