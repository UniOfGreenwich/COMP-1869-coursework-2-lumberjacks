using UnityEngine;

public class WorldShopBuilding : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject computerUI;  // @ANDREI NEW
    // shopPanel removed – now opened only from inside Computer_UI

    private void OnMouseDown()
    {
        if (computerUI == null)
        {
            Debug.LogWarning("[WorldShopBuilding] Computer UI not assigned.");
            return;
        }

        Debug.Log("[WorldShopBuilding] Opening Computer UI.");
        computerUI.SetActive(true);
    }
}
