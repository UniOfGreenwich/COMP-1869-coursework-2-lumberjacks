using UnityEngine;
using TMPro;

public class StockMarket : MonoBehaviour
{
    [SerializeField] RealWorldData realWorldData;
    [SerializeField] Inventory inventory;
    [SerializeField] private bool usingSimulatedData = true;
    
    [Header("Sell Panel")]
    [SerializeField] TMP_Text amountToSellUI;
    [SerializeField] TMP_Text totalPriceSell;
    [SerializeField] private int amountToSell = 0;
    [SerializeField] private int maxSell;

    [Header("Buy Panel")]
    [SerializeField] TMP_Text amountToPurchaseUI;
    [SerializeField] TMP_Text totalPriceBuy;
    [SerializeField] private int amountToPurchase = 0;
    [SerializeField] private int maxBuy;

    [Header("HUD")]
    [SerializeField] TMP_Text moneyUI;
    [SerializeField] TMP_Text lumberUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        amountToPurchaseUI.text = amountToPurchase.ToString();
        amountToSellUI.text = amountToSell.ToString();
        maxSell = //get wood amount
        if(usingSimulatedData)
        {
            maxBuy = Mathf.FloorToInt(Inventory.money/ tradeData[tradeData.GetLength[0]-1, 1]);  //get amount of money/price of wood rounded down
        }
        else
        {
            maxBuy = Mathf.FloorToInt(Inventory.money/costLumber);  //get amount of money/price of wood rounded down
        }
        
    }

    //Sell Panel
    public void AddAmountSell(int amount)
    {
        amountToSell += amount;
        amountToSellUI.text = amountToSell.ToString();
        UpdateTotalPriceSell();
    }

    public void SubtractAmountSell(int amount)
    {
        //validation to prevent negatives
        if((amountToSell - amount) >= 0)
        {
            amountToSell -= amount;
            amountToSellUI.text = amountToSell.ToString();
            UpdateTotalPriceSell();
        }
    }

    public void UpdateTotalPriceSell()
    {
        totalPriceSell.text = (amountToSell*realWorldData.costLumber).ToString();
    }

    public void ExecuteSell()
    {
        if((inventory.lumber - amountToSell) >= 0)
        {
            inventory.money += amountToSell*realWorldData.costLumber;
            inventory.lumber -= amountToSell;
            moneyUI.text = (inventory.money).ToString();
            lumberUI.text = (inventory.lumber).ToString();
        }
    }

    //Buy Panel
    public void AddAmountBuy(int amount)
    {
        amountToPurchase += amount;
        amountToPurchaseUI.text = amountToPurchase.ToString();
        UpdateTotalPriceBuy();
    }

    public void SubtractAmountBuy(int amount)
    {
        //validation to prevent negatives
        if((amountToPurchase - amount) >= 0)
        {
            amountToPurchase -= amount;
            amountToPurchaseUI.text = amountToPurchase.ToString();
            UpdateTotalPriceBuy();
        }
    }

    public void UpdateTotalPriceBuy()
    {
        totalPriceBuy.text = (amountToPurchase*realWorldData.costLumber).ToString();
    }

    public void ExecuteBuy()
    {
        if((inventory.money - amountToPurchase*realWorldData.costLumber) >= 0)
        {
            inventory.money -= amountToPurchase*realWorldData.costLumber;
            inventory.lumber += amountToPurchase;
            moneyUI.text = (inventory.money).ToString();
            lumberUI.text = (inventory.lumber).ToString();
        }
    }
}
