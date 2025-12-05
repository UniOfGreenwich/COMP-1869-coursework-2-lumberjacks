using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopRowUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI priceLabel;
    public Button buyButton;

    private ShopItemSO data;
    private GameShopPanelUI owner;

    public void Bind(GameShopPanelUI owner, ShopItemSO data)
    {
        this.owner = owner;
        this.data = data;

        if (nameLabel != null)
            nameLabel.text = string.IsNullOrEmpty(data.displayName) ? data.name : data.displayName;

        if (priceLabel != null)
            priceLabel.text = data.price.ToString();

        if (icon != null)
            icon.sprite = data.icon;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        Debug.Log("[ShopRow] Bound row for " + nameLabel.text);
    }

    void OnBuyClicked()
    {
        if (owner == null || data == null)
        {
            Debug.LogWarning("[ShopRow] Owner or data is missing.");
            return;
        }

        owner.HandleBuy(data);
    }
}
