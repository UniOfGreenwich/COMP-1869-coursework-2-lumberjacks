using UnityEngine;
using UnityEngine.Events;

public class TogglePanel : MonoBehaviour
{
    [SerializeField] private GameObject panel, UIBlocker;
    private bool panelOpen = false;
    
    public UnityEvent Activated;

    public void OpenPanel()
    {
        if(!panelOpen) 
        {
            panel.SetActive(true);
            UIBlocker.SetActive(true);
            Activated.Invoke();
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
