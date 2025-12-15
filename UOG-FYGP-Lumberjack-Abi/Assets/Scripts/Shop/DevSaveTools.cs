using UnityEngine;

public class DevSaveTools : MonoBehaviour
{
    const string PrefRecipePrefix = "RecipeUnlocked_";
    const string PrefShopOwnedPrefix = "ShopOwned_";

    [Header("Clear on start")]
    public bool clearAllOnStart = false;
    public bool clearRecipesOnStart = false;
    public bool clearShopOnStart = false;

    [Header("Keyboard shortcuts")]
    public bool enableHotkeys = true;

    void Awake()
    {
        if (clearAllOnStart)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            if (FindObjectOfType<StorageManager>() is StorageManager sm)
                sm.ClearAll(); // add a method to reset stock dictionary

            if (FindObjectOfType<Inventory>() is Inventory inv)
            {
                inv.money = 0;
                inv.lumber = 0;
                inv.RefreshUI();
            }
        }

        else
        {
            if (clearRecipesOnStart)
            {
                ClearRecipeUnlocks();
                Debug.Log("[DevSaveTools] Cleared recipe unlocks on start.");
            }

            if (clearShopOnStart)
            {
                ClearShopOwnership();
                Debug.Log("[DevSaveTools] Cleared shop ownership on start.");
            }
        }
    }

    void Update()
    {
        if (!enableHotkeys) return;

        if (Input.GetKeyDown(KeyCode.F5))
        {
            ClearRecipeUnlocks();
            Debug.Log("[DevSaveTools] F5 pressed, cleared recipe unlocks.");
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            ClearShopOwnership();
            Debug.Log("[DevSaveTools] F6 pressed, cleared shop ownership.");
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            ClearAllPlayerPrefs();
            Debug.Log("[DevSaveTools] F7 pressed, cleared ALL PlayerPrefs.");
        }
    }

    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DevSaveTools] Cleared ALL PlayerPrefs.");
    }

    public void ClearRecipeUnlocks()
    {
        ProductionRecipeSO[] allRecipes = Resources.FindObjectsOfTypeAll<ProductionRecipeSO>();

        int count = 0;
        foreach (var r in allRecipes)
        {
            if (r == null || string.IsNullOrEmpty(r.id)) continue;
            PlayerPrefs.DeleteKey(PrefRecipePrefix + r.id);
            count++;
        }

        PlayerPrefs.Save();
        Debug.Log("[DevSaveTools] Cleared recipe unlocks for " + count + " recipes.");
    }

    public void ClearShopOwnership()
    {
        ShopItemSO[] allShopItems = Resources.FindObjectsOfTypeAll<ShopItemSO>();

        int count = 0;
        foreach (var s in allShopItems)
        {
            if (s == null || string.IsNullOrEmpty(s.id)) continue;
            PlayerPrefs.DeleteKey(PrefShopOwnedPrefix + s.id);
            count++;
        }

        PlayerPrefs.Save();
        Debug.Log("[DevSaveTools] Cleared shop ownership for " + count + " items.");
    }
}
