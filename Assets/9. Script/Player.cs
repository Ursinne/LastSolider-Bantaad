using System.Collections.Generic;
using UnityEngine;
using InventoryAndCrafting;

// Add missing enum definition
public enum PersonalityType
{
    Analyst,
    Survivor,
    Leader,
    Diplomat
    // Add more personality types as needed
}

public class Player : MonoBehaviour
{
    public PersonalityType personality;
    private Dictionary<PersonalityType, SpecialAbility> specialAbilities;
    public float willpower = 100f;
    public float maxWillpower = 100f;
    public float willpowerRegenRate = 5f;

    public List<Quest> playerQuests = new List<Quest>();
    public int gold = 0;
    public int xp = 0;
    public GameObject questPanel;

    public Quest questLocateClanCrazy;
    public Quest questLocateClanSurvivors;
    public Quest questLocateClanInbreds;

    private void Awake()
    {
        // Sätt startvärden
        willpower = maxWillpower;

        specialAbilities = new Dictionary<PersonalityType, SpecialAbility>()
        {
            { PersonalityType.Analyst, new TacticalAnalysis() },
            // Add more special abilities here
        };

        //Debug.Log($"Willpower initialized to: {willpower}/{maxWillpower}");
    }

    private void Start()
    {
// Använd QuestManager istället för att hantera quests direkt
    if (questLocateClanCrazy != null) QuestManager.Instance.StartQuest(questLocateClanCrazy);
    if (questLocateClanSurvivors != null) QuestManager.Instance.StartQuest(questLocateClanSurvivors);
    if (questLocateClanInbreds != null) QuestManager.Instance.StartQuest(questLocateClanInbreds);
    }

    private void Update()
    {
        // Willpower regenereras nu i PlayerHealth-scriptet om det finns,
        // annars görs det här
        if (!GetComponent<PlayerHealth>())
        {
            willpower = Mathf.Min(willpower + willpowerRegenRate * Time.deltaTime, maxWillpower);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (questPanel != null) questPanel.SetActive(!questPanel.activeSelf);
        }

        // Använd K-knappen för att aktivera specialförmågan
        if (Input.GetKeyDown(KeyCode.K))
        {
            ActivateSpecialAbility();
            Debug.Log($"Nuvarande willpower: {willpower}/{maxWillpower}");
        }
    }

    public void AddQuest(Quest quest)
    {
        playerQuests.Add(quest);
       //Debug.Log($"Quest added: {quest.questName}");
    }

    public void GainGold(int amount)
    {
        gold += amount;
        //Debug.Log($"Gold increased by {amount}. Total gold: {gold}");
    }

    public void GainXP(int amount)
    {
        xp += amount;
        //Debug.Log($"XP increased by {amount}. Total XP: {xp}");
    }

    // Definition of SpecialAbility abstract class (removed duplicate)
    public abstract class SpecialAbility
    {
        public string name;
        public float willpowerCost;
        public abstract void Activate(Player player);
    }

    // Definition of TacticalAnalysis class (removed duplicate)
    public class TacticalAnalysis : SpecialAbility
    {
        public TacticalAnalysis()
        {
            name = "Tactical Analysis";
            willpowerCost = 20f; // Sänkt kostnad för testning
        }

        public override void Activate(Player player)
        {
            // Logic for scanning the environment and marking objects
            Debug.Log("Tactical Analysis activated! Scanning environment...");

            // Example implementation:
            // Find all interactable objects within a certain radius
            Collider[] colliders = Physics.OverlapSphere(player.transform.position, 20f);
            foreach (var collider in colliders)
            {
                // Mark important objects
                if (collider.CompareTag("Resource") || collider.CompareTag("Enemy") || collider.CompareTag("Item"))
                {
                    // Add temporary highlight effect or marker
                    Debug.Log($"Marked: {collider.gameObject.name}");
                }
            }
        }
    }

    public void ActivateSpecialAbility()
    {
        if (specialAbilities.TryGetValue(personality, out SpecialAbility ability))
        {
            Debug.Log($"Försöker aktivera: {ability.name}, Kostnad: {ability.willpowerCost}, Tillgänglig willpower: {willpower}");

            if (willpower >= ability.willpowerCost)
            {
                willpower -= ability.willpowerCost;
                ability.Activate(this);
                Debug.Log($"Aktiverade specialförmåga: {ability.name}, Återstående willpower: {willpower}");
            }
            else
            {
                Debug.Log($"Otillräcklig willpower för att aktivera specialförmågan! Behöver {ability.willpowerCost}, har {willpower}");
            }
        }
        else
        {
            Debug.Log($"Ingen specialförmåga hittades för personlighetstypen: {personality}");
        }
    }
}