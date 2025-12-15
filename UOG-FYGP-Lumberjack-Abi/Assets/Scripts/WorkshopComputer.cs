using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorkshopComputer : MonoBehaviour
{
    public GameObject computerPanel;
    public GameShopPanelUI shopPanel;
    public StockMarket stockMarket;

    bool panelOpen;
    public UnityEvent Opened;

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
            Opened?.Invoke();
        }
        else UIManager.Instance.Close(computerPanel);
    }

    public void OnStockMarketButtonClicked()
    {
        // Open stock market UI first to avoid the close consuming the click
        if (stockMarket != null)
            stockMarket.toggleStockMarketUI();

        if (UIManager.Instance != null)
            UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }

    public void OnShopButtonClicked()
    {
        // Open shop UI before closing the computer panel to ensure the click is delivered
        if (shopPanel != null)
            shopPanel.Open();

        if (UIManager.Instance != null)
            UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }

    public void OnCloseComputerPanel()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }
}
