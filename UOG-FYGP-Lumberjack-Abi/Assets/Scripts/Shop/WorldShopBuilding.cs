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
              
        computerUI.SetActive(true);
        PlayerController.IsInputLocked = true;
    }
}
