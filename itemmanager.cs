using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public ItemData item;
    public int count;

    public InventorySlotData(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    // 10 slots max
    public List<InventorySlotData> slots = new List<InventorySlotData>();
    private const int MAX_SLOTS = 10;
    private const int MAX_STACK = 6;

    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool AddItem(ItemData newItem)
    {
        // Try to stack if the item allows it
        if (newItem.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == newItem && slot.count < MAX_STACK)
                {
                    slot.count++;
                    onInventoryChangedCallback?.Invoke();
                    return true;
                }
            }
        }

        // If it can't stack or existing stacks are full, try to add a new slot
        if (slots.Count >= MAX_SLOTS)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        slots.Add(new InventorySlotData(newItem, 1));
        onInventoryChangedCallback?.Invoke();
        return true;
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        foreach (var slot in slots)
        {
            if (slot.item == itemToRemove)
            {
                slot.count--;
                if (slot.count <= 0)
                {
                    slots.Remove(slot);
                }
                onInventoryChangedCallback?.Invoke();
                break;
            }
        }
    }
}
