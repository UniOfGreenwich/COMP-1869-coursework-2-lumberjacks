using System.Collections.Generic;
using UnityEngine;

public class ProductionMachine : MonoBehaviour
{
    public ProductionMachineUI ui;
    public List<ProductionRecipeSO> recipes = new List<ProductionRecipeSO>();

    StorageManager storage;

    void Awake()
    {
        storage = Object.FindAnyObjectByType<StorageManager>();
    }

    void OnMouseDown()
    {
        if (ui == null) return;
        ui.gameObject.SetActive(true);
        ui.Init(this, recipes);
    }

    public void OnAssemble(ProductionRecipeSO recipe, int errors)
    {
        if (storage == null) return;
        if (recipe == null) return;
        if (recipe.finishedProduct == null) return;

        storage.Put(recipe.finishedProduct, 1);
        string key = string.IsNullOrEmpty(recipe.id) ? recipe.name : recipe.id;
        Debug.Log("[ProductionMachine] Built " + key + " with errors " + errors);
    }
}
