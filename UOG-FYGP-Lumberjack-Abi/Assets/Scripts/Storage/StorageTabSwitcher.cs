using UnityEngine;

public class StorageTabSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject utilitiesPage;
    [SerializeField] private GameObject productsPage;

    // When you click the "Utilities" button
    public void ShowUtilities()
    {
        utilitiesPage.SetActive(true);
        productsPage.SetActive(false);
    }

    // When you click the "Products" button
    public void ShowProducts()
    {
        utilitiesPage.SetActive(false);
        productsPage.SetActive(true);
    }

    private void Start()
    {
        // Start with Utilities visible by default
        ShowUtilities();
    }
}
