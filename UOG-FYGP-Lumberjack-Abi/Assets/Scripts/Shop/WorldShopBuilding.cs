using UnityEngine;
using UnityEngine.EventSystems;

public class WorldShopBuilding : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject computerUI;

    private void OnMouseDown()
    {
        if (PlayerController.IsInputLocked)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (computerUI == null)
        {
            Debug.LogWarning("[WorldShopBuilding] Computer UI not assigned.");
            return;
        }

        Debug.Log("[WorldShopBuilding] Opening Computer UI.");
        computerUI.SetActive(true);
        PlayerController.IsInputLocked = true;
    }
}
