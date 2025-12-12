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
        if (PlayerController.IsInputLocked)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        ToggleComputerPanel();
    }


    public void ToggleComputerPanel()
    {
        panelOpen = !panelOpen;

        if (computerPanel != null)
            computerPanel.SetActive(panelOpen);

        PlayerController.IsInputLocked = panelOpen;

        Debug.Log("[WorkshopComputer] Computer panel " +
                  (panelOpen ? "opened" : "closed"));
    }

    public void OnStockMarketButtonClicked()
    {
        if (stockMarket != null)
        {
            stockMarket.toggleStockMarketUI();
            Debug.Log("[WorkshopComputer] Toggled stock market UI.");
        }
        else
        {
            Debug.LogWarning("[WorkshopComputer] No StockMarket reference set.");
        }

        if (computerPanel != null)
            computerPanel.SetActive(false);

        panelOpen = false;
    }

    public void OnShopButtonClicked()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("[WorkshopComputer] No shopPanel reference set.");
            return;
        }

        shopPanel.Open();
        Debug.Log("[WorkshopComputer] Opened shop panel.");

        if (computerPanel != null)
            computerPanel.SetActive(false);

        panelOpen = false;
    }

    public void OnCloseComputerPanel()
    {
        panelOpen = false;

        if (computerPanel != null)
            computerPanel.SetActive(false);

        PlayerController.IsInputLocked = false;

        Debug.Log("[WorkshopComputer] Computer panel closed.");
    }
}
