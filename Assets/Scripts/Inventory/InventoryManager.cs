using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] List<InventorySlot> slots = new(); // starting items are set in the scene

    const int MaxSlots = 10;
    const int MaxStack = 6;

    public IReadOnlyList<InventorySlot> Slots => slots;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool Has(ItemData item) => slots.Exists(s => s.Item == item);

    // Stacks onto an existing slot when the item allows it, otherwise takes a new slot. False if the bag is full.
    public bool AddItem(ItemData item)
    {
        if (item.IsStackable)
        {
            InventorySlot stack = slots.Find(s => s.Item == item && s.Count < MaxStack);
            if (stack != null)
            {
                stack.Count++;
                return true;
            }
        }

        if (slots.Count >= MaxSlots) return false;
        slots.Add(new InventorySlot(item, 1));
        return true;
    }

    // Removes one of the item, dropping its slot when the stack runs out
    public void RemoveItem(ItemData item)
    {
        int i = slots.FindIndex(s => s.Item == item);
        if (i < 0) return;
        if (--slots[i].Count <= 0) slots.RemoveAt(i);
    }
}
