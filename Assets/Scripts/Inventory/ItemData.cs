using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] string itemName; // as shown in the ITEM menu, e.g. "Magic Key"
    [SerializeField] bool isStackable; // only herbs and keys stack in Dragon Warrior

    public string ItemName => itemName;
    public bool IsStackable => isStackable;
}
