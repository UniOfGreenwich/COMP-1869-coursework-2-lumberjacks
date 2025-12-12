using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class DeliveryTruck : MonoBehaviour
{
    public DeliveryPanelUI deliveryPanel;

    void OnMouseDown()
    {
        if (PlayerController.IsInputLocked) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (deliveryPanel == null)
        {
            Debug.LogWarning("[DeliveryTruck] DeliveryPanelUI reference not set.");
            return;
        }

        if (deliveryPanel.gameObject.activeSelf)
            deliveryPanel.Close();
        else
            deliveryPanel.Open();
    }

}
