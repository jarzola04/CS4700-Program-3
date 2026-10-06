using UnityEngine;
using TMPro; // I dunno if we need this one

public class InventoryUI : MonoBehaviour
{
    public Transform contentParent;
    public GameObject textLinePrefab;

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
