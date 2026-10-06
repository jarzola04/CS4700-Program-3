using UnityEngine;

public enum ItemType { Consumable, Equipment, QuestItem }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory")]
public class ItemData : ScriptableObject
{
    public string itemName; // like "Herb" or "Magic Key"
    public ItemType itemType;
    public bool isStackable; // Only true for herbs and keys in DQ1
}
