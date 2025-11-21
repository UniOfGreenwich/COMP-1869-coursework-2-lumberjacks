using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    [SerializeField] private GameObject panel, UIBlocker;
    private bool panelOpen = false;

    public void OpenPanel()
    {
        if(panelOpen = false) 
        {
            panel.SetActive(true);
            UIBlocker.SetActive(true);
            panelOpen = true;
        }
    }

    public void ClosePanel()
    {
        if(panelOpen) 
        {
            panel.SetActive(false);
            UIBlocker.SetActive(false);
            panelOpen = false;
        }
    }
}
