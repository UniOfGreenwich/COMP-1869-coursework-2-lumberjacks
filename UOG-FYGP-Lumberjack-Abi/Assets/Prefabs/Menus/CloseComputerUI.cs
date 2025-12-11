using UnityEngine;
using UnityEngine.UI;

public class CloseComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject computerUI; // Assign Computer_UI prefab here

    private void Awake()
    {
        computerUI = transform.root.gameObject; // closes whole prefab
        GetComponent<Button>().onClick.AddListener(() => computerUI.SetActive(false));
    }

    private void CloseUI()
    {
        if (computerUI != null)
        {
            Debug.Log("[CloseComputerUI] Closing Computer UI.");
            computerUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CloseComputerUI] computerUI reference missing.");
        }
    }


}
