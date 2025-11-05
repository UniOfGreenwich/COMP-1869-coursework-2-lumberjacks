using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public float money { get; set; } = 100;
    public int lumber { get; set; }
    public int gold { get; set; }
    public int copper { get; set; }

    [SerializeField] TMP_Text moneyUI;
    [SerializeField] TMP_Text lumberUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moneyUI.text = (money).ToString();
        lumberUI.text = (lumber).ToString();
    }
}
