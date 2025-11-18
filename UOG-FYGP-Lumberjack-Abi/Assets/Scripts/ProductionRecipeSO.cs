using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Production/Product Recipe", fileName = "ProductRecipe_")]
public class ProductionRecipeSO : ScriptableObject
{
    public string id;
    public string displayName;
    public ItemSO finishedProduct;
    public Blueprint blueprint;
    public List<SlotRequirement> slots = new List<SlotRequirement>();

    [System.Serializable]
    public class SlotRequirement
    {
        public string slotId;
        public string label;
        public ItemSO expectedPiece;
    }
}
