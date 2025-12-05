using UnityEngine;

public class WorldShopBuilding : MonoBehaviour
{
    public GameShopPanelUI shopPanel;

    void OnMouseDown()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("[WorldShopBuilding] ShopPanel not assigned.");
            return;
        }

        Debug.Log("[WorldShopBuilding] Opening shop.");
        shopPanel.Open();
    }
}
