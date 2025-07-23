using System.Collections.Generic;
using UnityEngine;
using InventoryAndCrafting;

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventoryPanel;
    public PickupDisplay pickupDisplay;
    private MissionManager missionManager;

    private void Start()
    {
        Debug.Log("InventoryPanel reference: " + (inventoryPanel != null));

        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryPanel is not assigned!");
        }
        else
        {
            Debug.Log("InventoryPanel assigned correctly.");
        }

        if (pickupDisplay == null)
        {
            Debug.LogError("PickupDisplay is not assigned!");
        }
        else
        {
            Debug.Log("PickupDisplay assigned correctly.");
        }

        // Kontrollera om InventoryManager finns
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance saknas i scenen!");
        }

        inventoryPanel.SetActive(false);

        missionManager = Object.FindFirstObjectByType<MissionManager>();
    }

    // Ta bort referensen till inventoryItems
    // public List<ItemData> inventoryItems = new List<ItemData>();

    public void AddItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("Försöker lägga till null itemData i inventory!");
            return;
        }

        Debug.Log($"Adding item to inventory: {itemData.itemName}, amount: {itemData.amount}, icon: {itemData.itemIcon?.name}");

        // Använd InventoryManager från InventoryAndCrafting istället för egen lista
        if (InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemData, itemData.amount);

            if (added)
            {
                // Hantera questprogression
                if (missionManager != null)
                {
                    Debug.Log("Calling UpdateMissionProgress...");
                    missionManager.UpdateMissionProgress(itemData.itemName, itemData.amount);
                }

                // Visa notifikation via PickupDisplay eller NotificationManager
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowItemNotification(itemData, itemData.amount);
                }
                else if (pickupDisplay != null)
                {
                    pickupDisplay.ShowPickup(itemData.itemName, itemData.itemIcon);
                }
            }
        }
        else
        {
            Debug.LogError("InventoryManager.Instance saknas!");
        }
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(itemToRemove);
        }
    }
}