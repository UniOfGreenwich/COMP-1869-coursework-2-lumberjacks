using UnityEngine;

public enum ShopItemType
{
    BuyItemToStorage,
    BuyMachineToPlace,
    BuyFieldToPlace
}

[CreateAssetMenu(menuName = "Shop/Shop Item", fileName = "ShopItem_")]
public class ShopItemSO : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Price")]
    [Min(1)] public int price = 10;

    [Header("Item Settings")]
    public ShopItemType type = ShopItemType.BuyItemToStorage;
    public ItemSO item;
    [Min(1)] public int itemCount = 1;

    [Header("Prefab Settings")]
    public GameObject prefabToPlace;
}
