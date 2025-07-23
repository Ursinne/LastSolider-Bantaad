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

    [SerializeField] private List<Quest> allAvailableQuests = new List<Quest>(); // Alla m�jliga quests i spelet
    [SerializeField] private int maxActiveQuests = 3;
    [SerializeField] private QuestPanelUI questUI;

    public List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    private List<string> completedQuestNames = new List<string>();
    private Player player;

    // F�r quest-progressionen
    [SerializeField] private int questTier = 1; // Nuvarande "niv�" av quests
    [SerializeField] private int questsCompletedInCurrentTier = 0;
    [SerializeField] private int questsRequiredForNextTier = 3; // Antal quests som beh�vs f�r att g� till n�sta tier

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

        // Debugga k�llan till quests
        //Debug.Log($"Antal quests innan rensning: {activeQuests.Count}");
        foreach (var quest in activeQuests)
        {
           // Debug.Log($"Quest fr�n: {quest.QuestData.name} - K�lla ok�nd");
        }

        StartQuestsForCurrentTier();

        player = FindObjectOfType<Player>();

        // Starta initiala quests baserat p� Tier 1
        StartQuestsForCurrentTier();

       // Debug.Log("QuestManager Start - Initierar quests");
        player = FindObjectOfType<Player>();

        // Visa alla tillg�ngliga quests
        Debug.Log($"Tillg�ngliga quests: {allAvailableQuests.Count}");
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

        // Rensa �ven listan �ver avklarade quests om du vill b�rja helt om
        completedQuestNames.Clear();

        //Debug.Log("Alla quests har rensats.");
    }

    private void StartQuestsForCurrentTier()
    {
        // Filtrera quests baserat p� nuvarande tier
        List<Quest> availableQuestsForTier = GetQuestsForTier(questTier);

        // Starta quests upp till maxgr�nsen eller s� m�nga som finns tillg�ngliga
        int questsToStart = Mathf.Min(maxActiveQuests - activeQuests.Count, availableQuestsForTier.Count);

        for (int i = 0; i < questsToStart; i++)
        {
            // Hitta n�sta tillg�ngliga quest som inte redan �r aktiv eller avklarad
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
        // Filtrera quests baserat p� tier (du beh�ver l�gga till ett tier-f�lt i Quest.cs)
        return allAvailableQuests.FindAll(q =>
            // Om du inte har lagt till questTier i Quest-klassen �nnu, ta bort denna kontroll tillf�lligt
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
            //Debug.LogWarning("Max antal aktiva quests n�tt!");
            return;
        }

        if (completedQuestNames.Contains(quest.name))
        {
            //Debug.Log($"Quest {quest.name} �r redan slutf�rd");
            return;
        }

        if (activeQuests.Exists(aq => aq.QuestData.name == quest.name))
        {
            //Debug.Log($"Quest {quest.name} �r redan aktiv");
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

        // Ge bel�ningar
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

        // �ka antal avklarade quests i nuvarande tier
        questsCompletedInCurrentTier++;

        // Kontrollera om vi ska g� till n�sta tier
        if (questsCompletedInCurrentTier >= questsRequiredForNextTier)
        {
            AdvanceToNextTier();
        }

        // Starta ny quest f�r att fylla p�
        StartQuestsForCurrentTier();

        // Uppdatera UI
        UpdateQuestUI();

        // Visa CompletionPanel utan att �ndra p� questUI's aktiva status
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

        // �ka sv�righeten f�r n�sta tier
        questsRequiredForNextTier += 1; // �ka gradvis antal quests som beh�vs

        //Debug.Log($"Avancerade till Tier {questTier}! Krav f�r n�sta tier: {questsRequiredForNextTier} quests");
    }

    private void UpdateQuestUI()
    {
        //Debug.Log($"UpdateQuestUI anropas. questUI �r {(questUI == null ? "NULL" : "GILTIG")}");
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