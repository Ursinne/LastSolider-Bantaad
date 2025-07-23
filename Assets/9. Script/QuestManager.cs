using System.Collections.Generic;
using UnityEngine;
using InventoryAndCrafting;

public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestUIData
    {
        public Quest quest;
        public List<QuestObjective> objectives;
        public bool isComplete;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string type;
        public string target;
        public int requiredAmount;
        public int currentAmount;
        public string deliverTo;
    }

    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<Quest> allAvailableQuests = new List<Quest>(); // Alla möjliga quests i spelet
    [SerializeField] private int maxActiveQuests = 3;
    [SerializeField] private QuestPanelUI questUI;

    public List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    private List<string> completedQuestNames = new List<string>();
    private Player player;

    // För quest-progressionen
    [SerializeField] private int questTier = 1; // Nuvarande "nivå" av quests
    [SerializeField] private int questsCompletedInCurrentTier = 0;
    [SerializeField] private int questsRequiredForNextTier = 3; // Antal quests som behövs för att gå till nästa tier

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        ClearAllQuests();

        player = FindObjectOfType<Player>();

        // Debugga källan till quests
        //Debug.Log($"Antal quests innan rensning: {activeQuests.Count}");
        foreach (var quest in activeQuests)
        {
           // Debug.Log($"Quest från: {quest.QuestData.name} - Källa okänd");
        }

        StartQuestsForCurrentTier();

        player = FindObjectOfType<Player>();

        // Starta initiala quests baserat på Tier 1
        StartQuestsForCurrentTier();

       // Debug.Log("QuestManager Start - Initierar quests");
        player = FindObjectOfType<Player>();

        // Visa alla tillgängliga quests
        Debug.Log($"Tillgängliga quests: {allAvailableQuests.Count}");
        foreach (var quest in allAvailableQuests)
        {
           // Debug.Log($"Quest: {quest.name} - {quest.questName}");
        }

        // Starta initiala quests
        StartQuestsForCurrentTier();

        // Kontrollera aktiva quests efter start
        //Debug.Log($"Aktiva quests efter start: {activeQuests.Count}");
        foreach (var quest in activeQuests)
        {
           // Debug.Log($"Aktiv quest: {quest.QuestData.questName}");
        }
    }

    private void ClearAllQuests()
    {
        // Rensa alla aktiva quests
        activeQuests.Clear();

        // Rensa även listan över avklarade quests om du vill börja helt om
        completedQuestNames.Clear();

        //Debug.Log("Alla quests har rensats.");
    }

    private void StartQuestsForCurrentTier()
    {
        // Filtrera quests baserat på nuvarande tier
        List<Quest> availableQuestsForTier = GetQuestsForTier(questTier);

        // Starta quests upp till maxgränsen eller så många som finns tillgängliga
        int questsToStart = Mathf.Min(maxActiveQuests - activeQuests.Count, availableQuestsForTier.Count);

        for (int i = 0; i < questsToStart; i++)
        {
            // Hitta nästa tillgängliga quest som inte redan är aktiv eller avklarad
            Quest nextQuest = FindNextAvailableQuest(availableQuestsForTier);
            if (nextQuest != null)
            {
                StartQuest(nextQuest);
            }
        }

        // Uppdatera UI
        UpdateQuestUI();
    }

    private List<Quest> GetQuestsForTier(int tier)
    {
        // Filtrera quests baserat på tier (du behöver lägga till ett tier-fält i Quest.cs)
        return allAvailableQuests.FindAll(q =>
            // Om du inte har lagt till questTier i Quest-klassen ännu, ta bort denna kontroll tillfälligt
            // q.questTier == tier && 
            !completedQuestNames.Contains(q.name) &&
            !activeQuests.Exists(aq => aq.QuestData.name == q.name));
    }

    private Quest FindNextAvailableQuest(List<Quest> availableQuests)
    {
        foreach (var quest in availableQuests)
        {
            if (!completedQuestNames.Contains(quest.name) &&
                !activeQuests.Exists(aq => aq.QuestData.name == quest.name))
            {
                return quest;
            }
        }
        return null;
    }

    public void StartQuest(Quest quest)
    {
        if (activeQuests.Count >= maxActiveQuests)
        {
            //Debug.LogWarning("Max antal aktiva quests nått!");
            return;
        }

        if (completedQuestNames.Contains(quest.name))
        {
            //Debug.Log($"Quest {quest.name} är redan slutförd");
            return;
        }

        if (activeQuests.Exists(aq => aq.QuestData.name == quest.name))
        {
            //Debug.Log($"Quest {quest.name} är redan aktiv");
            return;
        }

        ActiveQuest newQuest = new ActiveQuest(quest);
        activeQuests.Add(newQuest);
        UpdateQuestUI();

       // Debug.Log($"Startade quest: {quest.questName}");
    }

    public void UpdateQuestProgress(string type, string target, int amount = 1)
    {
        bool questsUpdated = false;

        foreach (var activeQuest in activeQuests.ToArray())
        {
            if (activeQuest.UpdateProgress(type, target, amount))
            {
                questsUpdated = true;

                if (activeQuest.IsCompleted)
                {
                    CompleteQuest(activeQuest);
                }
            }
        }

        if (questsUpdated)
        {
            UpdateQuestUI();
        }
    }

    public void CompleteQuest(ActiveQuest activeQuest)
    {
        Quest quest = activeQuest.QuestData;

        // Ge belöningar
        if (player != null)
        {
            player.GainXP(quest.xpReward);
            player.GainGold(quest.goldReward);

            if (quest.itemRewards != null)
            {
                foreach (var item in quest.itemRewards)
                {
                    InventoryManager.Instance?.AddItem(item);
                }
            }
        }

        // Markera quest som avklarad
        completedQuestNames.Add(quest.name);
        activeQuests.Remove(activeQuest);

        // Öka antal avklarade quests i nuvarande tier
        questsCompletedInCurrentTier++;

        // Kontrollera om vi ska gå till nästa tier
        if (questsCompletedInCurrentTier >= questsRequiredForNextTier)
        {
            AdvanceToNextTier();
        }

        // Starta ny quest för att fylla på
        StartQuestsForCurrentTier();

        // Uppdatera UI
        UpdateQuestUI();

        // Visa CompletionPanel utan att ändra på questUI's aktiva status
        if (questUI != null)
        {
            questUI.ShowCompletionPanel(quest);
        }

        Debug.Log($"Quest avklarad: {quest.questName}. {questsCompletedInCurrentTier}/{questsRequiredForNextTier} klara i Tier {questTier}");
    }

    private void AdvanceToNextTier()
    {
        questTier++;
        questsCompletedInCurrentTier = 0;

        // Öka svårigheten för nästa tier
        questsRequiredForNextTier += 1; // Öka gradvis antal quests som behövs

        //Debug.Log($"Avancerade till Tier {questTier}! Krav för nästa tier: {questsRequiredForNextTier} quests");
    }

    private void UpdateQuestUI()
    {
        //Debug.Log($"UpdateQuestUI anropas. questUI är {(questUI == null ? "NULL" : "GILTIG")}");
        if (questUI != null)
        {
            //Debug.Log($"Skickar {activeQuests.Count} aktiva quests till UI");
            questUI.DisplayQuests(activeQuests.ConvertAll(aq =>
                new QuestUIData
                {
                    quest = aq.QuestData,
                    objectives = aq.Objectives,
                    isComplete = aq.IsCompleted
                }
            ));
        }
        else
        {
           // Debug.LogError("questUI referens saknas i QuestManager!");
        }
    }
}