using UnityEngine;
using UnityEngine.UI;

public class CloseComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject computerUI;
    private Button button;

    private void Awake()
    {
        if (computerUI == null)
            computerUI = transform.root.gameObject;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(CloseUI);
        }
    }

    private void CloseUI()
    {
        var computer = FindObjectOfType<WorkshopComputer>();
        if (computer != null)
        {
            computer.OnCloseComputerPanel();
            return;
        }

        if (computerUI != null)
            computerUI.SetActive(false);

        PlayerController.IsInputLocked = false;
    }
}
