using UnityEngine;
using UnityEngine.EventSystems;

public class WorkshopComputer : MonoBehaviour
{
    public GameObject computerPanel;
    public GameShopPanelUI shopPanel;
    public StockMarket stockMarket;

    bool panelOpen;
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTapOrClick(touch.position);
            }
        }
    }

    void OnMouseDown()
    {
        if (PlayerController.IsInputLocked) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        ToggleComputerPanel();
    }
    void HandleTapOrClick(Vector2 screenPosition)
    {
        if (PlayerController.IsInputLocked) return;

        // Check if tapping on UI
        if (EventSystem.current != null && IsPointerOverUI(screenPosition)) return;

        // Raycast to check if this object was tapped
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                ToggleComputerPanel();
            }
        }
    }

    bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    public void ToggleComputerPanel()
    {
        panelOpen = !panelOpen;

        if (panelOpen) UIManager.Instance.Open(computerPanel);
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
