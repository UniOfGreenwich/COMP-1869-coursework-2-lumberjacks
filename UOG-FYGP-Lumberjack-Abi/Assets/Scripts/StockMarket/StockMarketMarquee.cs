using UnityEngine;
using TMPro;

public class StockMarketMarquee : MonoBehaviour
{
    [SerializeField] private TMP_Text lumberPrice;
    private RealWorldData realWorldData;
    private Vector3 startPos;
    private Vector3 endPos;
    [SerializeField] private float lerpDuration = 2f;
    private float lerpTime;
    [SerializeField] private bool usingSimulatedData = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        realWorldData = GameObject.FindWithTag("GameController").GetComponent<RealWorldData>();
        startPos = new Vector3(-100, 940, 0);
        endPos = new Vector3(2020, 940, 0);
        lumberPrice.transform.position = startPos;
        lerpTime = 0f;

        if (usingSimulatedData)
        {
            lumberPrice.text = "LUMBER $" + SimulatedRealWorldDataSet.tradeData[SimulatedRealWorldDataSet.tradeData.GetLength(0) - 1, 1].ToString();
        }
        else if (realWorldData != null)
        {
            lumberPrice.text = "LUMBER $" + realWorldData.costLumber.ToString();
        }
        else
        {
            lumberPrice.text = "LUMBER $0";
        }
    }

    // Update is called once per frame
    void Update()
    {
        //while transition isn't complete move the prices across the screen
        if(lerpTime < lerpDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / lerpDuration;
            lumberPrice.transform.position = Vector3.Lerp(startPos, endPos, t);
        }
        else
        {
            lumberPrice.transform.position = startPos;
            lerpTime = 0f;
        }
    }
}
