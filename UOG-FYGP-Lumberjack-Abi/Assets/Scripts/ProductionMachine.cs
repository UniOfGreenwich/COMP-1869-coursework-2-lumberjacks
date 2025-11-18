using System.Collections.Generic;
using UnityEngine;

public class ProductionMachine : MonoBehaviour
{
    public List<ProductionRecipeSO> recipes = new List<ProductionRecipeSO>();
    public GameObject uiPrefab;

    private StorageManager storage;
    private Placeble placeble;
    private ProductionMachineUI uiInstance;
    private readonly Dictionary<string, int> totalErrors = new Dictionary<string, int>();

    void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        placeble = GetComponent<Placeble>();
    }

    void OnMouseDown()
    {
        if (placeble && !placeble.placed) return;
        OpenUI();
    }

    void OpenUI()
    {
        if (!uiPrefab) return;

        if (uiInstance == null)
        {
            Transform parent = GameManager.current ? GameManager.current.canvas.transform : null;
            var go = Instantiate(uiPrefab, parent);
            uiInstance = go.GetComponent<ProductionMachineUI>();
            uiInstance.Init(this, recipes);
        }
        else
        {
            uiInstance.gameObject.SetActive(true);
        }
    }

    public void OnAssembleRequested(ProductionRecipeSO recipe, int errors)
    {
        if (!storage || recipe == null || recipe.finishedProduct == null) return;

        storage.Put(recipe.finishedProduct, 1);

        string key = string.IsNullOrEmpty(recipe.id) ? recipe.name : recipe.id;
        if (!totalErrors.ContainsKey(key)) totalErrors[key] = 0;
        totalErrors[key] += errors;

        Debug.Log("[ProductionMachine] Product " + key + " errors " + errors + " total " + totalErrors[key]);
    }
}
