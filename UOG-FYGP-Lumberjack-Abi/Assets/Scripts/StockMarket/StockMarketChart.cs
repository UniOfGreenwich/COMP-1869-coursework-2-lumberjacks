using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StockMarketChart : MonoBehaviour
{
    /*
    Get array size of realWorldData then - timePeriod and store as startTime
    use this to generate the last 12 hours of candlesticks
    Take the abs(open-close) to get the height of the candlestick
    take open+(open-close)/2 to position to generate the candlestick
    Repeat 11 times and add width each time
    */

    [SerializeField] List<Image> candleSticks;
    [SerializeField] private int timePeriod = 12;
    private int startTime;
    private float height;
    private float position;
    [SerializeField] private float positionOffset;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = SimulatedRealWorldDataSet.tradeData.GetLength(0) - timePeriod;
        GenerateCandleSticks();
    }

    void GenerateCandleSticks()
    {   
        for(int i=0;i < timePeriod; i++)
        {
            GameObject candleStick = new GameObject("CandleStick");    
            candleStick.transform.SetParent(this.transform);
            candleStick.transform.localScale = Vector3.one;
            candleStick.transform.localPosition = Vector3.zero;
            Image image = candleStick.AddComponent<Image>();
            candleSticks.Add(image);

            height = SimulatedRealWorldDataSet.tradeData[startTime+i, 1] - SimulatedRealWorldDataSet.tradeData[startTime+i, 0];
            position = SimulatedRealWorldDataSet.tradeData[startTime+i, 0] + height/2;
            candleSticks[i].rectTransform.sizeDelta = new Vector2(25, Mathf.Abs(height));
            candleSticks[i].rectTransform.anchoredPosition = new Vector2(i * 25 - 600, position-positionOffset);
            
            //red or green candlestick based on increase or decrease in value
            if (height>0)
            {
                candleSticks[i].color = Color.green;
            }
            else
            {
                candleSticks[i].color = Color.red;
            }
        }
    }
}
