using UnityEngine;
using TMPro;
public class StockMarket : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private RealWorldData realWorldData;
    [SerializeField] private Inventory inventory;
    [SerializeField] private bool usingSimulatedData = true;

    // Optional simulated trade data (time, price)
    [SerializeField] private float[,] tradeData;

    [Header("Sell Panel")]
    [SerializeField] private TMP_Text amountToSellUI;
    [SerializeField] private TMP_Text totalPriceSellUI;
    [SerializeField] private int amountToSell = 0;
    private int maxSell;

    [Header("Buy Panel")]
    [SerializeField] private TMP_Text amountToBuyUI;
    [SerializeField] private TMP_Text totalPriceBuyUI;
    [SerializeField] private int amountToBuy = 0;
    private int maxBuy;

    [Header("HUD")]
    [SerializeField] private TMP_Text moneyUI;
    [SerializeField] private TMP_Text lumberUI;

    private void Start()
    {
        // Initialize UI
        amountToBuyUI.text = amountToBuy.ToString();
        amountToSellUI.text = amountToSell.ToString();

        // Max sell = current lumber
        maxSell = inventory != null ? inventory.lumber : 0;

        // Max buy = money / price
        if (usingSimulatedData && tradeData != null && tradeData.GetLength(0) > 0)
        {
            float lastPrice = tradeData[tradeData.GetLength(0) - 1, 1];
            maxBuy = Mathf.FloorToInt(inventory.money / lastPrice);
        }
        else if (realWorldData != null)
        {
            maxBuy = Mathf.FloorToInt(inventory.money / realWorldData.costLumber);
        }
        else
        {
            maxBuy = 0;
        }

        UpdateHUD();
    }

    #region Sell Panel
    public void AddAmountSell(int amount)
    {
        amountToSell = Mathf.Clamp(amountToSell + amount, 0, maxSell);
        amountToSellUI.text = amountToSell.ToString();
        UpdateTotalPriceSell();
    }

    public void SubtractAmountSell(int amount)
    {
        amountToSell = Mathf.Max(amountToSell - amount, 0);
        amountToSellUI.text = amountToSell.ToString();
        UpdateTotalPriceSell();
    }

    private void UpdateTotalPriceSell()
    {
        if (realWorldData != null)
            totalPriceSellUI.text = (amountToSell * realWorldData.costLumber).ToString();
    }

    public void ExecuteSell()
    {
        if (inventory != null && amountToSell > 0 && inventory.lumber >= amountToSell)
        {
            inventory.money += amountToSell * realWorldData.costLumber;
            inventory.lumber -= amountToSell;
            amountToSell = 0;
            UpdateHUD();
        }
    }
    #endregion

    #region Buy Panel
    public void AddAmountBuy(int amount)
    {
        amountToBuy = Mathf.Clamp(amountToBuy + amount, 0, maxBuy);
        amountToBuyUI.text = amountToBuy.ToString();
        UpdateTotalPriceBuy();
    }

    public void SubtractAmountBuy(int amount)
    {
        amountToBuy = Mathf.Max(amountToBuy - amount, 0);
        amountToBuyUI.text = amountToBuy.ToString();
        UpdateTotalPriceBuy();
    }

    private void UpdateTotalPriceBuy()
    {
        if (realWorldData != null)
            totalPriceBuyUI.text = (amountToBuy * realWorldData.costLumber).ToString();
    }

    public void ExecuteBuy()
    {
        if (inventory != null && amountToBuy > 0)
        {
            int totalCost = Mathf.RoundToInt(amountToBuy * realWorldData.costLumber);
            if (inventory.money >= totalCost)
            {
                inventory.money -= totalCost;
                inventory.lumber += amountToBuy;
                amountToBuy = 0;
                UpdateHUD();
            }
        }
    }
    #endregion

    private void UpdateHUD()
    {
        if (moneyUI != null) moneyUI.text = inventory.money.ToString();
        if (lumberUI != null) lumberUI.text = inventory.lumber.ToString();
        if (amountToBuyUI != null) amountToBuyUI.text = amountToBuy.ToString();
        if (amountToSellUI != null) amountToSellUI.text = amountToSell.ToString();
    }
}
