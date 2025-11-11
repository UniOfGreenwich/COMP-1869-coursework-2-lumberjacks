using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StockMarketChart : MonoBehaviour
{
    [SerializeField] List<Image> candleSticks;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateCandleSticks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateCandleSticks()
    {
        for(int i = 0; i < 12; i++)
        {
            GameObject candleStick = new GameObject("CandleStick");    
            candleStick.transform.SetParent(this.transform);
            candleStick.transform.localScale = Vector3.one;
            candleStick.transform.localPosition = Vector3.zero;
            Image image = candleStick.AddComponent<Image>();
            RectTransform rectTransform = candleStick.GetComponent<RectTransform>();
            candleSticks.Add(image);
            candleSticks[i].rectTransform.anchoredPosition = new Vector2(i*50-600, 0);

            //random height between 50 and 500
            float height = Random.Range(50, 500);
            candleSticks[i].rectTransform.sizeDelta = new Vector2(25, height);
            
            //random color red or green
            if (Random.value > 0.5f)
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
