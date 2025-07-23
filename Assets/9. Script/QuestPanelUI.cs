using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using InventoryAndCrafting;

public class QuestPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject questListContent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private GameObject noQuestsMessage;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI completionTitleText;
    [SerializeField] private TextMeshProUGUI completionRewardsText;

    private bool isInitialized = false;
    private bool wasQuestPanelActive = false;

    private void Awake()
    {
        // Säkerställ att completion panel är dold från början
        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void Start()
    {
        // Dölj hela quest-panelen från start
        gameObject.SetActive(false);
        isInitialized = true;
    }

    public void DisplayQuests(List<QuestManager.QuestUIData> quests)
    {
        // Rensa befintliga quest-objekt
        foreach (Transform child in questListContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Visa/dölj 'inga quests' meddelande
        if (noQuestsMessage != null)
            noQuestsMessage.SetActive(quests.Count == 0);

        foreach (var questData in quests)
        {
            GameObject questItem = Instantiate(questItemPrefab, questListContent.transform);
            var titleText = questItem.transform.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
            var descriptionText = questItem.transform.Find("QuestDescription")?.GetComponent<TextMeshProUGUI>();
            var rewardText = questItem.transform.Find("QuestReward")?.GetComponent<TextMeshProUGUI>();

            if (titleText != null)
                titleText.text = questData.quest.questName;

            if (descriptionText != null)
                descriptionText.text = questData.quest.description;

            if (rewardText != null)
                rewardText.text = questData.quest.GetRewardDescription();
        }
    }

    public void ShowCompletionPanel(Quest quest)
    {
        if (completionPanel != null)
        {
            // Spara nuvarande tillstånd för huvudpanelen
            wasQuestPanelActive = gameObject.activeSelf;

            // Om huvudpanelen var inaktiv men vi behöver visa completion panel
            if (!wasQuestPanelActive)
            {
                // Aktivera bara completion panel, inte hela quest-panelen
                completionPanel.transform.SetParent(transform.parent);
                completionPanel.SetActive(true);
            }
            else
            {
                // Om huvudpanelen redan var aktiv, visa completion panel som vanligt
                completionPanel.SetActive(true);
            }

            // Uppdatera UI-element
            completionTitleText.text = $"Quest slutfört: {quest.questName}";
            completionRewardsText.text = $"Belöningar:\n{quest.GetRewardDescription()}";

            // Avbryt eventuell tidigare körande dölj-operation
            CancelInvoke("HideCompletionPanel");

            // Använd Invoke istället för coroutine (mer tillförlitlig när objekt aktiveras/inaktiveras)
            Invoke("HideCompletionPanel", 5f);

            Debug.Log("Quest completion panel visas nu");
        }
    }

    // Metod som anropas av Invoke för att dölja kompletteringspanelen
    private void HideCompletionPanel()
    {
        if (completionPanel != null)
        {
            // Återställ parent om den flyttades ut
            if (!wasQuestPanelActive)
            {
                completionPanel.transform.SetParent(transform);
            }

            completionPanel.SetActive(false);
            Debug.Log("Quest completion panel döljs nu");
        }
    }

    public void ToggleQuestPanel()
    {
        // Förhindra togglen innan initialisering är klar
        if (!isInitialized) return;

        // Om completion panelen är aktiv, stäng den först
        if (completionPanel != null && completionPanel.activeSelf)
        {
            completionPanel.SetActive(false);
            return;
        }

        // Växla quest-panelens synlighet
        gameObject.SetActive(!gameObject.activeSelf);
    }
}