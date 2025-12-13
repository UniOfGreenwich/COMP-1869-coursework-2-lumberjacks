using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class GameShopPanelUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject rootPanel;

    [Header("List Setup")]
    public Transform contentRoot;
    public ShopRowUI rowPrefab;
    public ShopItemSO[] items;

    [Header("External")]
    public StorageManager storage;
    public Inventory inventory;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackLabel;

    const string PrefOwnedPrefix = "ShopOwned_";

    void Awake()
    {
        if (storage == null)
            storage = FindFirstObjectByType<StorageManager>();

        if (inventory == null)
        {
            GameObject gm = GameObject.FindWithTag("GameController");
            if (gm != null)
                inventory = gm.GetComponent<Inventory>();
        }

        Debug.Log("[ShopPanel] Awake, storage = " + (storage ? storage.name : "null") +
                  ", inventory = " + (inventory ? inventory.name : "null"));
    }

    void Start()
    {
        BuildList();
        Close();
    }

    public void Open()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
        else
            gameObject.SetActive(true);

        if (feedbackLabel != null)
            feedbackLabel.text = string.Empty;

        PlayerController.IsInputLocked = true;
        Debug.Log("[ShopPanel] Opened.");
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
        else
            gameObject.SetActive(false);

        PlayerController.IsInputLocked = false;
        Debug.Log("[ShopPanel] Closed.");
    }

    void BuildList()
    {
        if (contentRoot == null || rowPrefab == null)
        {
            Debug.LogError("[ShopPanel] contentRoot or rowPrefab not set.");
            return;
        }

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("[ShopPanel] No items configured.");
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;

            bool owned = IsOwned(item);
            var row = Instantiate(rowPrefab, contentRoot);
            row.Bind(this, item, owned);
        }

        Debug.Log("[ShopPanel] Built " + items.Length + " rows.");
    }

    public void HandleBuy(ShopItemSO item)
    {
        if (item == null) return;

        if (inventory == null)
        {
            Debug.LogError("[ShopPanel] No Inventory found.");
            return;
        }

        // stop extra buys for single-purchase items
        if (item.singlePurchase && IsOwned(item))
        {
            Debug.Log("[ShopPanel] " + item.displayName + " already owned, skipping buy.");
            if (feedbackLabel != null)
                feedbackLabel.text = "Owned.";
            return;
        }

        if (item.price > 0 && !inventory.TrySpend(item.price))
        {
            Debug.Log("[ShopPanel] Not enough money for " + item.displayName);
            if (feedbackLabel != null)
                feedbackLabel.text = "Not enough money.";
            return;
        }

        bool success = false;

        try
        {
            // Always close shop before doing any buy
            Close();

            switch (item.type)
            {
                case ShopItemType.BuyItemToStorage:
                    success = BuyItemToStorage(item);
                    break;

                case ShopItemType.BuyMachineToPlace:
                case ShopItemType.BuyFieldToPlace:
                    success = BuyPlaceable(item);
                    break;

                case ShopItemType.BuyRecipe:
                    success = BuyRecipe(item);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ShopPanel] Exception during buy: " + ex);
            success = false;
        }

        if (!success)
        {
            Debug.LogWarning("[ShopPanel] Buy failed for " + item.displayName);
            if (feedbackLabel != null)
                feedbackLabel.text = "Buy failed.";

            if (item.price > 0)
                inventory.AddMoney(item.price); // refund
            return;
        }

        if (item.singlePurchase)
            SetOwned(item);

        Debug.Log("[ShopPanel] Buy success for " + item.displayName);

        if (feedbackLabel != null)
            feedbackLabel.text = "Bought " + item.displayName;

        BuildList();
    }
    bool BuyItemToStorage(ShopItemSO item)
    {
        if (storage == null)
        {
            storage = FindFirstObjectByType<StorageManager>();
            if (storage == null)
            {
                Debug.LogError("[ShopPanel] No StorageManager found.");
                return false;
            }
        }

        if (item.item == null || item.itemCount <= 0)
        {
            Debug.LogWarning("[ShopPanel] Item config is not valid for " + item.name);
            return false;
        }

        storage.Put(item.item, item.itemCount);
        Debug.Log("[ShopPanel] Added " + item.itemCount + " x " + item.item.displayName + " to storage.");
        return true;
    }

    bool BuyPlaceable(ShopItemSO item)
    {
        if (item.prefabToPlace == null) return false;
        if (BuildingSystem.instance == null) return false;

        Close(); // close shop before placement
        BuildingSystem.instance.StartPlacement(item.prefabToPlace);
        return true;
    }

    bool BuyRecipe(ShopItemSO item)
    {
        if (item.recipeToUnlock == null) return false;
        if (string.IsNullOrEmpty(item.recipeToUnlock.id)) return false;

        PlayerPrefs.SetInt("RecipeUnlocked_" + item.recipeToUnlock.id, 1);
        PlayerPrefs.Save();
        return true;
    }


    public bool IsOwned(ShopItemSO item)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return false;
        return PlayerPrefs.GetInt(PrefOwnedPrefix + item.id, 0) == 1;
    }

    void SetOwned(ShopItemSO item)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return;

        PlayerPrefs.SetInt(PrefOwnedPrefix + item.id, 1);
        PlayerPrefs.Save();

        Debug.Log("[ShopPanel] Marked owned: " + item.displayName);
    }
}
