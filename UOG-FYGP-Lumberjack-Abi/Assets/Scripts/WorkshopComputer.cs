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
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;

        if (stockMarket != null)
            stockMarket.toggleStockMarketUI();
    }

    public void OnShopButtonClicked()
    {
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;

        if (shopPanel != null)
            shopPanel.Open();
    }

    public void OnCloseComputerPanel()
    {
        UIManager.Instance.Close(computerPanel);
        panelOpen = false;
    }
}
