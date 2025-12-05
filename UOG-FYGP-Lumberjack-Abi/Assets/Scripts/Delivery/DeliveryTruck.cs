using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryTruck : MonoBehaviour
{
    public DeliveryPanelUI deliveryPanel;

    void OnMouseDown()
    {
        if (!deliveryPanel) return;

        if (deliveryPanel.gameObject.activeSelf)
            deliveryPanel.Close();
        else
            deliveryPanel.Open();
    }
}
