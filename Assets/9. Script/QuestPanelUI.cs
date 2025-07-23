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
        // S�kerst�ll att completion panel �r dold fr�n b�rjan
        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void Start()
    {
        // D�lj hela quest-panelen fr�n start
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

        // Visa/d�lj 'inga quests' meddelande
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
            // Spara nuvarande tillst�nd f�r huvudpanelen
            wasQuestPanelActive = gameObject.activeSelf;

            // Om huvudpanelen var inaktiv men vi beh�ver visa completion panel
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
            completionTitleText.text = $"Quest slutf�rt: {quest.questName}";
            completionRewardsText.text = $"Bel�ningar:\n{quest.GetRewardDescription()}";

            // Avbryt eventuell tidigare k�rande d�lj-operation
            CancelInvoke("HideCompletionPanel");

            // Anv�nd Invoke ist�llet f�r coroutine (mer tillf�rlitlig n�r objekt aktiveras/inaktiveras)
            Invoke("HideCompletionPanel", 5f);

            Debug.Log("Quest completion panel visas nu");
        }
    }

    // Metod som anropas av Invoke f�r att d�lja kompletteringspanelen
    private void HideCompletionPanel()
    {
        if (completionPanel != null)
        {
            // �terst�ll parent om den flyttades ut
            if (!wasQuestPanelActive)
            {
                completionPanel.transform.SetParent(transform);
            }

            completionPanel.SetActive(false);
            Debug.Log("Quest completion panel d�ljs nu");
        }
    }

    public void ToggleQuestPanel()
    {
        // F�rhindra togglen innan initialisering �r klar
        if (!isInitialized) return;

        // Om completion panelen �r aktiv, st�ng den f�rst
        if (completionPanel != null && completionPanel.activeSelf)
        {
            completionPanel.SetActive(false);
            return;
        }

        // V�xla quest-panelens synlighet
        gameObject.SetActive(!gameObject.activeSelf);
    }
}