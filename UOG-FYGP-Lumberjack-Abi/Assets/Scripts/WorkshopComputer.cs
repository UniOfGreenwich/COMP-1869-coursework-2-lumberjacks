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
            UIManager.Instance.Open(computerPanel);
        }
        else
        {
            UIManager.Instance.Close(computerPanel);
        }
    }

    // Called when stock market button is clicked
    public void OnStockMarketButtonClicked()
    {
        if (stockMarket != null)
            stockMarket.toggleStockMarketUI();

        // Always close computer panel and reset lock
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }

    // Called when shop button is clicked
    public void OnShopButtonClicked()
    {
        if (shopPanel != null)
            shopPanel.Open();

        // Always close computer panel and reset lock
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }

    // Called when close button is clicked
    public void OnCloseComputerPanel()
    {
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }
    void OnEnable()
    {
        if (computerPanel != null && computerPanel.activeSelf)
            PlayerController.IsInputLocked = true;
    }

    void OnDisable()
    {
        PlayerController.IsInputLocked = false;
    }

}
