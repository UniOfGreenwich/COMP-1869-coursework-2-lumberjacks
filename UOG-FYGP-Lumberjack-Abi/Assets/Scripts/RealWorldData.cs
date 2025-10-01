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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /* 
        The prices of each resource will be based on real world data but not exactly the same instead it will be 
        calculated as a base value subtract % difference from the daily average.

        - get last 24 hours of lumber, gold, and copper prices from an API
        - average the price for each based on 5 min intervals (this will be costActualAverage variables)
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
        //example API calls
        StartCoroutine(GetRequest("https://api.api-ninjas.com/v1/commoditypricehistorical?name=lumber&period=4h"));
        StartCoroutine(GetRequest("https://api.api-ninjas.com/v1/commoditypricehistorical?name=gold&period=4h"));

        //example base prices
        float baseLumberPrice = 100f; //base price for lumber
        float baseGoldPrice = 1500f; //base price for gold
        float baseCopperPrice = 4f; //base price for copper

        //calculate percentage difference from average
        float lumberPriceDifference = (costActualAverageLumber - costActualLumber) / costActualAverageLumber;
        float goldPriceDifference = (costActualAverageGold - costActualGold) / costActualAverageGold;
        float copperPriceDifference = (costActualAverageCopper - costActualCopper) / costActualAverageCopper;

        //adjust base prices based on percentage difference
        costLumber = baseLumberPrice * (1 + lumberPriceDifference);
        costGold = baseGoldPrice * (1 + goldPriceDifference);
        costCopper = baseCopperPrice * (1 + copperPriceDifference);
    }
    //every 5 mins update prices to reflect real world data
}
