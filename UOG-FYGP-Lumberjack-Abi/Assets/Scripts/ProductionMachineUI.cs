using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionMachineUI : MonoBehaviour
{
    [Header("Grid")]
    public GridManager gridManager;

    [Header("Slots Pool")]
    public ProductioSlotUI[] slots;

    [Header("Product Picker")]
    public Transform productButtonContainer;
    public Button productButtonPrefab;

    [Header("UI Text")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI errorText;

    [Header("Buttons")]
    public Button assembleButton;
    public Button closeButton;

    ProductionMachine owner;
    readonly List<ProductionRecipeSO> recipes = new List<ProductionRecipeSO>();
    ProductionRecipeSO currentRecipe;

    public void Init(ProductionMachine machine, IList<ProductionRecipeSO> recipeList)
    {
        owner = machine;
        recipes.Clear();
        if (recipeList != null)
        {
            for (int i = 0; i < recipeList.Count; i++)
            {
                if (recipeList[i] != null) recipes.Add(recipeList[i]);
            }
        }

        WireButtons();
        BuildProductButtons();
        if (recipes.Count > 0)
        {
            SelectRecipe(recipes[0]);
        }
        else
        {
            ConfigureSlots(null);
            UpdateErrorLabel(0);
        }
    }

    void WireButtons()
    {
        if (assembleButton != null)
        {
            assembleButton.onClick.RemoveAllListeners();
            assembleButton.onClick.AddListener(OnAssembleClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    void BuildProductButtons()
    {
        if (!productButtonContainer || !productButtonPrefab) return;

        for (int i = productButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(productButtonContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < recipes.Count; i++)
        {
            ProductionRecipeSO recipe = recipes[i];
            Button btn = Object.Instantiate(productButtonPrefab, productButtonContainer);
            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = recipe.displayName;
            btn.onClick.AddListener(() => SelectRecipe(recipe));
        }
    }

    void SelectRecipe(ProductionRecipeSO recipe)
    {
        currentRecipe = recipe;

        if (titleText != null)
        {
            titleText.text = recipe != null ? recipe.displayName : string.Empty;
        }

        if (gridManager != null)
        {
            gridManager.blueprint = recipe != null ? recipe.blueprint : null;
            gridManager.Generate();
        }

        ConfigureSlots(recipe);
        UpdateErrorLabel(0);
    }

    void ConfigureSlots(ProductionRecipeSO recipe)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            ProductioSlotUI slot = slots[i];
            if (slot == null) continue;

            if (recipe != null && i < recipe.slots.Count)
            {
                var req = recipe.slots[i];
                slot.gameObject.SetActive(true);
                slot.Configure(req.slotId, req.label);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    void OnAssembleClicked()
    {
        if (owner == null || currentRecipe == null) return;

        int errors = CalculateErrors();
        UpdateErrorLabel(errors);
        owner.OnAssemble(currentRecipe, errors);
    }

    void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }

    int CalculateErrors()
    {
        if (currentRecipe == null) return 0;

        int errors = 0;
        for (int i = 0; i < currentRecipe.slots.Count; i++)
        {
            var req = currentRecipe.slots[i];
            ProductioSlotUI slot = GetSlotById(req.slotId);
            errors += SlotError(slot, req.requiredWidth, req.requiredHeight);
        }
        return errors;
    }

    ProductioSlotUI GetSlotById(string slotId)
    {
        if (slots == null) return null;
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot != null && slot.slotId == slotId) return slot;
        }
        return null;
    }

    int SlotError(ProductioSlotUI slot, int reqW, int reqH)
    {
        if (slot == null) return 1;
        ItemSO item = slot.CurrentItem;
        if (item == null) return 1;

        int w = Mathf.Max(1, item.gridWidth);
        int h = Mathf.Max(1, item.gridHeight);

        bool match = (w == reqW && h == reqH) || (w == reqH && h == reqW);
        return match ? 0 : 1;
    }

    void UpdateErrorLabel(int errors)
    {
        if (errorText == null) return;

        if (errors <= 0)
        {
            errorText.text = "Perfect fit. 0 wrong fits.";
        }
        else if (errors == 1)
        {
            errorText.text = "1 wrong fit.";
        }
        else
        {
            errorText.text = errors + " wrong fits.";
        }
    }
}
