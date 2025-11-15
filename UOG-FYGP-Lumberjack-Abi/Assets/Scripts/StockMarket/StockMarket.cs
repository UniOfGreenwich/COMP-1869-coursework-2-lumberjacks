using UnityEngine;
using TMPro;
public class StockMarket : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private RealWorldData realWorldData;
    [SerializeField] private Inventory inventory;
    [SerializeField] private bool usingSimulatedData = true;

    [Header("Stock Market Panel")]
    [SerializeField] private GameObject stockMarketUIPanel;
    private bool panelOpen = false;

    [Header("Sell Panel")]
    [SerializeField] private TMP_Text amountToSellUI;
    [SerializeField] private TMP_Text totalPriceSellUI;
    [SerializeField] private int amountToSell = 0;
    private int maxSell;
    private float lumberLastPrice;

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
        maxSell = inventory.lumber;

        UpdateMaxBuy();
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
        totalPriceSellUI.text = (amountToSell * lumberLastPrice).ToString();
    }

    public void ExecuteSell()
    {
        if (inventory.lumber >= amountToSell)
        {
            inventory.money += amountToSell * lumberLastPrice;
            inventory.lumber -= amountToSell;
            amountToSell = 0;
            UpdateMaxSell();
            UpdateMaxBuy();
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
        totalPriceBuyUI.text = (amountToBuy * lumberLastPrice).ToString();
    }

    public void ExecuteBuy()
    {
        if (amountToBuy > 0)
        {
            int totalCost = Mathf.RoundToInt(amountToBuy * lumberLastPrice);
            if (inventory.money >= totalCost)
            {
                inventory.money -= totalCost;
                inventory.lumber += amountToBuy;
                amountToBuy = 0;
                UpdateMaxSell();
                UpdateMaxBuy();
                UpdateHUD();
            }
        }
    }
    #endregion

    private void UpdateHUD()
    {
        moneyUI.text = inventory.money.ToString();
        lumberUI.text = inventory.lumber.ToString();
        amountToBuyUI.text = amountToBuy.ToString();
        amountToSellUI.text = amountToSell.ToString();
    }

    private void UpdateMaxSell()
    {
        maxSell = inventory.lumber;
    }

    private void UpdateMaxBuy()
    {
        // Max buy = money / price
        if (usingSimulatedData)
        {
            lumberLastPrice = SimulatedRealWorldDataSet.tradeData[SimulatedRealWorldDataSet.tradeData.GetLength(0) - 1, 1];
            maxBuy = Mathf.FloorToInt(inventory.money / lumberLastPrice);
        }
        else if (realWorldData != null)
        {
            lumberLastPrice = realWorldData.costLumber;
            maxBuy = Mathf.FloorToInt(inventory.money / lumberLastPrice);
        }
        else
        {
            maxBuy = 0;
        }
    }

    public void toggleStockMarketUI()
    {
        if(panelOpen) 
        {
            stockMarketUIPanel.SetActive(false);
            panelOpen = false;
        }
        else
        {
            stockMarketUIPanel.SetActive(true);
            panelOpen = true;
        }
    }
}
