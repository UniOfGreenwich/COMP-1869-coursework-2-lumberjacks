using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorageUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (titleText != null)
            titleText.text = "STORAGE";
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
