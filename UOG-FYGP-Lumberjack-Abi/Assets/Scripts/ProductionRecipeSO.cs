using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Production/Recipe", fileName = "Recipe_")]
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
        [Min(1)] public int requiredWidth = 1;
        [Min(1)] public int requiredHeight = 1;
    }
}
