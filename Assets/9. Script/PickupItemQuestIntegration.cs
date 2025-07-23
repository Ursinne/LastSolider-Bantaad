using UnityEngine;
using InventoryAndCrafting;

public class PickupItemQuestIntegration
{
    void QuestIntegration(ItemData itemData, int amount)
    {
        QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, amount);
    }
}