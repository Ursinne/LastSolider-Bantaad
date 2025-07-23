using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveQuest
{
    public Quest QuestData { get; private set; }
    public List<QuestManager.QuestObjective> Objectives { get; private set; }
    public bool IsCompleted { get; private set; }

    public ActiveQuest(Quest quest)
    {
        QuestData = quest;
        Objectives = new List<QuestManager.QuestObjective>
        {
            new QuestManager.QuestObjective
            {
                type = quest.questType.ToString().ToLower(),
                target = GetTargetForQuestType(quest),
                requiredAmount = quest.targetAmount,
                currentAmount = 0
            }
        };
    }

    private string GetTargetForQuestType(Quest quest)
    {
        switch (quest.questType)
        {
            case QuestType.Gather: return quest.targetItem;
            case QuestType.Kill: return quest.targetEnemy;
            case QuestType.Explore: return quest.targetLocation;
            case QuestType.Craft: return quest.targetItem;
            default: return quest.description;
        }
    }

    public bool UpdateProgress(string type, string target, int amount)
    {
        foreach (var objective in Objectives)
        {
            if (objective.type.ToLower() == type.ToLower() &&
                objective.target.ToLower() == target.ToLower() &&
                objective.currentAmount < objective.requiredAmount)
            {
                objective.currentAmount += amount;
                objective.currentAmount = Mathf.Min(
                    objective.currentAmount,
                    objective.requiredAmount
                );

                CheckCompletion();
                return true;
            }
        }
        return false;
    }

    private void CheckCompletion()
    {
        IsCompleted = Objectives.TrueForAll(
            obj => obj.currentAmount >= obj.requiredAmount
        );
    }
}