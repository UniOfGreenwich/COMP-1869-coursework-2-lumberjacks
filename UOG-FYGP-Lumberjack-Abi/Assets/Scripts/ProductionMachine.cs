using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Placeble))]
public class ProductionMachine : MonoBehaviour
{
    public GameObject uiPrefab;
    public List<ProductionRecipeSO> recipes = new List<ProductionRecipeSO>();

    ProductionMachineUI uiInstance;
    StorageManager storage;
    Placeble placeble;

    void Awake()
    {
        storage = Object.FindAnyObjectByType<StorageManager>();
        placeble = GetComponent<Placeble>();
    }

    void OnMouseDown()
    {
        if (placeble != null && !placeble.placed) return;
        OpenUI();
    }

    void OpenUI()
    {
        if (!uiPrefab) return;

        if (uiInstance == null)
        {
            Transform parent = GameManager.current ? GameManager.current.canvas.transform : null;
            GameObject go = Instantiate(uiPrefab, parent);
            uiInstance = go.GetComponent<ProductionMachineUI>();
            uiInstance.Init(this, recipes);
        }
        else
        {
            uiInstance.gameObject.SetActive(true);
        }
    }

    public void OnAssemble(ProductionRecipeSO recipe, int errors)
    {
        if (storage == null || recipe == null || recipe.finishedProduct == null) return;

        storage.Put(recipe.finishedProduct, 1);

        string key = string.IsNullOrEmpty(recipe.id) ? recipe.name : recipe.id;
        Debug.Log("[ProductionMachine] Built " + key + " with errors " + errors);
    }
}
