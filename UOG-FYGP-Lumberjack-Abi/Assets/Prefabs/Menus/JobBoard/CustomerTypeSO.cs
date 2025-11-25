using UnityEngine;

[CreateAssetMenu(menuName = "Customers/Customer Type", fileName = "CustomerType_")]
public class CustomerTypeSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite portrait;
    public Color uiColor = Color.white;

    [Min(0f)] public float moneyMultiplier = 1f;
    [Min(0f)] public float xpMultiplier = 1f;
}
