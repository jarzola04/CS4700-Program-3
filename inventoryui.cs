using UnityEngine;
using TMPro; // Remove if using legacy Text components

public class InventoryUI : MonoBehaviour
{
    public Transform contentParent; // Your menu panel with the Vertical Layout Group
    public GameObject textLinePrefab; // The InventoryTextLine prefab

    void Start()
    {
        InventoryManager.Instance.onInventoryChangedCallback += Update_UI;
        Update_UI();
    }

    void Update_UI()
    {
        // Clear previous menu options
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Generate the text lines dynamically
        foreach (var slot in InventoryManager.Instance.slots)
        {
            GameObject newLine = Instantiate(textLinePrefab, contentParent);
            TextMeshProUGUI textComponent = newLine.GetComponent<TextMeshProUGUI>();

            // If stackable, show quantity. Else, just print the name.
            if (slot.item.isStackable)
            {
                textComponent.text = $"{slot.item.itemName} x{slot.count}";
            }
            else
            {
                textComponent.text = slot.item.itemName;
            }
        }
    }
}
