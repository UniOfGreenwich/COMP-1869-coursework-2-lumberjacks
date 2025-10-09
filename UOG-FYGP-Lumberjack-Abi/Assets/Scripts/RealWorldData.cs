using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RealWorldData : MonoBehaviour
{
    public float costLumber { get; private set; }
    public float costActualAverageLumber { get; private set; }
    public float costActualLumber { get; private set; }
    public float costGold { get; private set; }
    public float costActualAverageGold { get; private set; }
    public float costActualGold { get; private set; }
    public float costCopper { get; private set; }
    public float costActualAverageCopper { get; private set; }
    public float costActualCopper { get; private set; }
    [SerializeField] private float baseLumberPrice = 100f; //base price for lumber
    [SerializeField] private float baseGoldPrice = 1500f; //base price for gold
    [SerializeField] private float baseCopperPrice = 4f; //base price for copper
    [SerializeField] private TMPro.TextMeshProUGUI lumberPriceText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /* 
        The prices of each resource will be based on real world data but not exactly the same instead it will be 
        calculated as a base value * % difference from the daily average.

        - get last 24 hours of lumber, gold, and copper prices from an API
        - average the price for each based on 1 hour intervals (this will be costActualAverage variables)
        - get the most recent price for each (this will be costActual variables)

        */
        CalculatePrices();
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("X-Api-Key", "bAnnAk5/Mmvn+8M8txQdHA==p4aCP8grE70FfbI7");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("HTTP Response: " + webRequest.error);
                Debug.LogError("Server Response: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }
    
    private void CalculatePrices()
    {
        StartCoroutine(GetRequest("https://api.api-ninjas.com/v1/commoditypricehistorical?name=lumber&period=1h"));

        //calculate percentage difference from average
        float lumberPriceDifference = costActualLumber / costActualAverageLumber;
        float goldPriceDifference = costActualGold / costActualAverageGold;
        float copperPriceDifference = costActualCopper / costActualAverageCopper;

        //adjust base prices based on percentage difference
        costLumber = baseLumberPrice * lumberPriceDifference;
        lumberPriceText.text = "Lumber Price: " + costLumber;
        costGold = baseGoldPrice * goldPriceDifference;
        costCopper = baseCopperPrice * copperPriceDifference;
    }
}
