using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPC : MonoBehaviour
{
    // ...

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestPanelUI questPanelUI = FindObjectOfType<QuestPanelUI>();
            if (questPanelUI != null)
            {
                // Konvertera List<Quest> till List<QuestManager.QuestUIData>
                //var questUIDataList = quests.Select(q => new QuestManager.QuestUIData
                //{
                //    quest = q,
                //    objectives = new List<QuestManager.QuestObjective>(),
                //    isComplete = false
                //}).ToList();
                //questPanelUI.DisplayQuests(questUIDataList);
            }
        }
    }
}