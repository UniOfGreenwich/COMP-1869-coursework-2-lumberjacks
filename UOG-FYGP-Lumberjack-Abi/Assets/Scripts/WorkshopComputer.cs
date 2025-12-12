using UnityEngine;
using UnityEngine.EventSystems;

public class WorkshopComputer : MonoBehaviour
{
    [Header("Panels")]
    public GameObject computerPanel;      // small panel with the 2 icons
    public GameShopPanelUI shopPanel;     // your GameShopPanelUI
    public StockMarket stockMarket;       // your StockMarket script

    bool panelOpen;

    void OnMouseDown()
    {
        if (PlayerController.IsInputLocked) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        ToggleComputerPanel();
    }

    public void ToggleComputerPanel()
    {
        panelOpen = !panelOpen;

        if (panelOpen)
        {
            UIManager.Instance.Open(computerPanel);   // locks input
        }
        else
        {
            UIManager.Instance.Close(computerPanel);  // unlocks input
        }
    }

    // Called when stock market button is clicked
    public void OnStockMarketButtonClicked()
    {
        if (stockMarket != null)
            stockMarket.toggleStockMarketUI();

        UIManager.Instance.Close(computerPanel); // unlocks input
        panelOpen = false;
    }

    // Called when shop button is clicked
    public void OnShopButtonClicked()
    {
        if (shopPanel != null)
            shopPanel.Open();

        UIManager.Instance.Close(computerPanel); // unlocks input
        panelOpen = false;
    }

    // Called when close button is clicked
    public void OnCloseComputerPanel()
    {
        UIManager.Instance.Close(computerPanel); // unlocks input
        panelOpen = false;
    }
}
