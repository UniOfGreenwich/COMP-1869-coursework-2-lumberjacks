using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI storageTypeText;
    [SerializeField] private TextMeshProUGUI maxItemsText;
    [SerializeField] private Slider maxItemsSlider;

    [SerializeField] private GameObject itemsView;
    [SerializeField] private GameObject increaseView;

    [SerializeField] private Transform itemsContent;
    [SerializeField] private Transform increaseContent;

    [SerializeField] private GameObject itemPrefab;

    public void SetNameText(string name)
    {
        storageTypeText.text = name;
    }
    public void CloseButton_()
    {
        gameObject.SetActive(false);
    }
    public void Upgrade_()
    {
        increaseView.SetActive(true);
    }
    public void Back()
    {
        increaseView.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
    
