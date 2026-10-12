using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] ItemData item;
    [SerializeField] int count;

    public ItemData Item => item;
    public int Count { get => count; internal set => count = value; }

    public InventorySlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
