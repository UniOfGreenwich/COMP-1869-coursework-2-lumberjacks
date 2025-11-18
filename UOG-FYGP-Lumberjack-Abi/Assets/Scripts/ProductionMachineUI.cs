using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionMachineUI : MonoBehaviour
{
    public GridManager gridManager;
    public Transform productButtonContainer;
    public Button productButtonPrefab;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI errorText;
    public Button assembleButton;
    public Button closeButton;
    public ProductioSlotUI[] slots;

    private ProductionMachine owner;
    private readonly List<ProductionRecipeSO> recipes = new List<ProductionRecipeSO>();
    private ProductionRecipeSO currentRecipe;

    public void Init(ProductionMachine machine, List<ProductionRecipeSO> recipeList)
    {
        owner = machine;
        recipes.Clear();
        if (recipeList != null) recipes.AddRange(recipeList);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (assembleButton != null)
        {
            assembleButton.onClick.RemoveAllListeners();
            assembleButton.onClick.AddListener(Assemble);
        }

        BuildProductButtons();
        gameObject.SetActive(true);
    }

    void BuildProductButtons()
    {
        if (!productButtonContainer || !productButtonPrefab) return;

        foreach (Transform child in productButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipe in recipes)
        {
            var btn = Instantiate(productButtonPrefab, productButtonContainer);
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = recipe.displayName;
            var localRecipe = recipe;
            btn.onClick.AddListener(() => SelectRecipe(localRecipe));
        }

        if (recipes.Count > 0) SelectRecipe(recipes[0]);
    }

    void SelectRecipe(ProductionRecipeSO recipe)
    {
        currentRecipe = recipe;
        if (titleText) titleText.text = recipe != null ? recipe.displayName : string.Empty;
        ApplyBlueprint();
        ConfigureSlots();
        UpdateErrorLabel(0);
    }

    void ApplyBlueprint()
    {
        if (!gridManager) return;

        gridManager.blueprint = currentRecipe ? currentRecipe.blueprint : null;

        if (currentRecipe && currentRecipe.blueprint && !string.IsNullOrEmpty(currentRecipe.blueprint.asciiLayout))
        {
            var lines = currentRecipe.blueprint.asciiLayout.Replace("\r", string.Empty).Split('\n');
            gridManager.height = lines.Length;
            int w = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > w) w = lines[i].Length;
            }
            gridManager.width = w;
        }

        gridManager.Generate();
    }

    void ConfigureSlots()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (currentRecipe != null && i < currentRecipe.slots.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Configure(currentRecipe.slots[i]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    ProductioSlotUI FindSlot(string slotId)
    {
        if (slots == null) return null;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s != null && s.slotId == slotId) return s;
        }
        return null;
    }

    int ComputeErrors()
    {
        if (currentRecipe == null) return 0;

        int errors = 0;

        foreach (var requirement in currentRecipe.slots)
        {
            var slot = FindSlot(requirement.slotId);
            if (slot == null)
            {
                errors++;
                continue;
            }

            var item = slot.CurrentItem;
            if (item == null)
            {
                errors++;
                continue;
            }

            if (requirement.expectedPiece && item != requirement.expectedPiece)
            {
                errors++;
            }
        }

        return errors;
    }

    void Assemble()
    {
        if (owner == null || currentRecipe == null) return;
        int errors = ComputeErrors();
        UpdateErrorLabel(errors);
        owner.OnAssembleRequested(currentRecipe, errors);
    }

    void UpdateErrorLabel(int errors)
    {
        if (errorText) errorText.text = "Errors " + errors;
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
