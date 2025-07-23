using UnityEngine;
using InventoryAndCrafting;
using System.Linq;

public enum QuestType
{
    Gather,
    Kill,
    Explore,
    Craft,
    Special
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Details")]
    public string questName;
    public string description;
    public QuestType questType;
    public int questTier = 1;
    public Sprite questIcon;

    [Header("Objectives")]
    public string targetItem;
    public string targetEnemy;
    public string targetLocation;
    public string targetNPC;
    public int targetAmount = 1;

    [Header("Rewards")]
    public int xpReward;
    public int goldReward;
    public ItemData[] itemRewards;

    public string GetObjectiveDescription() => questType switch
    {
        QuestType.Gather => $"Samla {targetAmount} {targetItem}",
        QuestType.Kill => $"Döda {targetAmount} {targetEnemy}",
        QuestType.Explore => $"Utforska {targetLocation}",
        QuestType.Craft => $"Crafta {targetAmount} {targetItem}",
        QuestType.Special => description,
        _ => "Okänt uppdrag"
    };

    public string GetRewardDescription()
    {
        var rewards = new System.Text.StringBuilder();

        if (xpReward > 0)
            rewards.Append($"{xpReward} XP ");

        if (goldReward > 0)
            rewards.Append($"{goldReward} Guld ");

        if (itemRewards is { Length: > 0 })
        {
            rewards.Append("Föremål: ");
            rewards.AppendJoin(", ", itemRewards.Select(item => item.itemName));
        }

        return rewards.ToString().Trim();
    }
}