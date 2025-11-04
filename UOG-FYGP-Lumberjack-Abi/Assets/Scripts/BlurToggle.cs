using UnityEngine;

public class BlurToggle : MonoBehaviour
{
    [SerializeField] private GameObject blurOverlay; // full-screen Image with blur material

    private void OnEnable()
    {
        if (blurOverlay) blurOverlay.SetActive(true);
    }

    private void OnDisable()
    {
        if (blurOverlay) blurOverlay.SetActive(false);
    }
}
