using UnityEngine;

public enum ItemCategory { Utility, FinishedProduct }

[CreateAssetMenu(menuName = "Utilities/Item", fileName = "Item_")]
public class ItemSO : ScriptableObject
{
    public string id;                 
    public string displayName;        
    public Sprite icon;               
    public ItemCategory category;     
    [Min(1)] public int maxStack = 20;
}
