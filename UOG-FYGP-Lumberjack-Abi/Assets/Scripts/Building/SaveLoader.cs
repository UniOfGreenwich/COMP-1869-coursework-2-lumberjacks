using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Inventory inventory;
    [SerializeField] public StorageManager storage;
    [SerializeField] public BuildingSystem buildingSystem;
    [SerializeField] public ShopItemSO[] shopItems;

    void Awake()
    {
        LoadInventory();
        LoadStorage();
        LoadPlacedObjects();
    }

    void OnApplicationQuit()
    {
        SaveInventory();
        SaveStorage();
    }

    private void SaveInventory()
    {
        PlayerPrefs.SetFloat("Money", inventory.money);
        PlayerPrefs.SetInt("Xp", inventory.xp);
        PlayerPrefs.SetInt("Lumber", inventory.lumber);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        inventory.money = PlayerPrefs.GetFloat("Money", 5000f);
        inventory.AddXp(PlayerPrefs.GetInt("Xp", 0));
        inventory.lumber = PlayerPrefs.GetInt("Lumber", 0);
        inventory.RefreshUI();
    }

    private void SaveStorage()
    {
        foreach (var kv in storage.AllItems())
        {
            if (kv.Key == null) continue;
            PlayerPrefs.SetInt("Storage_" + kv.Key.id, kv.Value);
        }
        PlayerPrefs.Save();
    }

    private void LoadStorage()
    {
        foreach (var e in storage.startingItems)
        {
            if (!e.item) continue;
            int saved = PlayerPrefs.GetInt("Storage_" + e.item.id, e.count);
            storage.SetCount(e.item, saved);
        }
    }

    private void LoadPlacedObjects()
    {
        foreach (var item in shopItems)
        {
            if (item == null || item.prefabToPlace == null) continue;

            if (PlayerPrefs.GetInt("MachineOwned_" + item.id, 0) == 1)
            {
                Vector3 pos = new Vector3(
                    PlayerPrefs.GetFloat("MachinePosX_" + item.id, 0f),
                    PlayerPrefs.GetFloat("MachinePosY_" + item.id, 0f),
                    PlayerPrefs.GetFloat("MachinePosZ_" + item.id, 0f)
                );
                Quaternion rot = Quaternion.Euler(
                    0f,
                    PlayerPrefs.GetFloat("MachineRotY_" + item.id, 0f),
                    0f
                );

                Debug.Log("[SaveLoader] Reloading " + item.id + " at " + pos);

                GameObject obj = buildingSystem.InitializeWithObject(item.prefabToPlace, pos);
                obj.transform.rotation = rot;

                var p = obj.GetComponentInChildren<Placeble>();
                if (p != null)
                {
                    p.prefabId = item.id;
                    p.Load();
                    Vector3Int start = buildingSystem.gridLayout.WorldToCell(p.GetStartPosition());
                    buildingSystem.TakeArea(start, p.Size);
                }
                else
                {
                    Debug.LogError("[SaveLoader] No Placeble found on " + obj.name);
                }
            }
        }
    }

}
