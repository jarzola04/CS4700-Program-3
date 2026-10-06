using UnityEngine;

public enum ItemType { Consumable, Equipment, QuestItem }

[CreateAssetMenu(fileName = "New DQ Item", menuName = "Inventory/DQ Item")]
public class ItemData : ScriptableObject
{
    public string itemName; // like "Herb" or "Magic Key"
    public ItemType itemType;
    public bool isStackable; // Only true for herbs and keys in DQ1
}
