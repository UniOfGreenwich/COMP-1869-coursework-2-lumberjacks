using UnityEngine;
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
            feedbackLabel.text = "";
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
        else
            gameObject.SetActive(false);
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

            var row = Instantiate(rowPrefab, contentRoot);
            row.Bind(this, item);
        }

        Debug.Log("[ShopPanel] Built " + items.Length + " rows.");
    }

    public void HandleBuy(ShopItemSO item)
    {
        if (inventory == null)
        {
            Debug.LogError("[ShopPanel] No Inventory found.");
            return;
        }

        if (item.price <= 0)
        {
            Debug.LogWarning("[ShopPanel] Item price is not valid for " + item.name);
            if (feedbackLabel != null)
                feedbackLabel.text = "Config error.";
            return;
        }

        // Spend money first
        if (!inventory.TrySpend(item.price))
        {
            Debug.Log("[ShopPanel] Not enough money for " + item.displayName);
            if (feedbackLabel != null)
                feedbackLabel.text = "Not enough money.";
            return;
        }

        bool success = false;

        switch (item.type)
        {
            case ShopItemType.BuyItemToStorage:
                success = BuyItemToStorage(item);
                break;

            case ShopItemType.BuyMachineToPlace:
            case ShopItemType.BuyFieldToPlace:
                success = BuyPlaceable(item);
                break;
        }

        if (!success)
        {
            Debug.LogWarning("[ShopPanel] Buy failed for " + item.displayName);
            if (feedbackLabel != null)
                feedbackLabel.text = "Buy failed.";

            // Refund if something went wrong
            inventory.AddMoney(item.price);
            return;
        }

        Debug.Log("[ShopPanel] Buy success for " + item.displayName);
        if (feedbackLabel != null)
            feedbackLabel.text = "Bought " + item.displayName;
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
        if (item.prefabToPlace == null)
        {
            Debug.LogWarning("[ShopPanel] Prefab is missing for " + item.name);
            return false;
        }

        if (BuildingSystem.instance == null)
        {
            Debug.LogError("[ShopPanel] No BuildingSystem in scene.");
            return false;
        }

        Debug.Log("[ShopPanel] Starting placement for " + item.displayName);
        BuildingSystem.instance.StartPlacement(item.prefabToPlace);
        return true;
    }
}
