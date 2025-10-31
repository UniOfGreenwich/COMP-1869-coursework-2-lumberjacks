using UnityEngine;
using TMPro;

public class StockMarketMarquee : MonoBehaviour
{
    [SerializeField] private TMP_Text lumber;
    private Vector3 startPos;
    private Vector3 endPos;
    [SerializeField] private float lerpDuration = 2f;
    private float lerpTime;
    [SerializeField] RealWorldData prices;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = new Vector3(-100, 440, 0);
        endPos = new Vector3(1000, 440, 0);
        lumber.transform.position = startPos;
        lerpTime = 0f;
        lumber.text = "LUMBER: $" + prices.costLumber.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        //while transition isn't complete move the prices across the screen
        if(lerpTime < lerpDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / lerpDuration;
            lumber.transform.position = Vector3.Lerp(startPos, endPos, t);
        }
        else
        {
            lumber.transform.position = startPos;
            lerpTime = 0f;
        }
    }
}
