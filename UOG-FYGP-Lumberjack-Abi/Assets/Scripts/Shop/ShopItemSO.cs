using UnityEngine;

public enum ShopItemType
{
    BuyItemToStorage,   // utilities / bulk items
    BuyMachineToPlace,  // machines with Placeble
    BuyFieldToPlace,    // tree fields with Placeble
    BuyRecipe           // unlock a ProductionRecipeSO
}

[CreateAssetMenu(menuName = "Shop/Shop Item", fileName = "ShopItem_")]
public class ShopItemSO : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Price")]
    [Min(0)] public int price = 10;

    [Header("Type")]
    public ShopItemType type = ShopItemType.BuyItemToStorage;

    [Header("Item Settings")]
    public ItemSO item;
    [Min(1)] public int itemCount = 1;

    [Header("Prefab Settings")]
    public GameObject prefabToPlace;

    [Header("Recipe Settings")]
    public ProductionRecipeSO recipeToUnlock;
}
