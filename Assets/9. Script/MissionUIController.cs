using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MissionUIController : MonoBehaviour
{
    public TextMeshProUGUI missionText; // Dra din Text-komponent hit i inspektorn
    public GameObject completionMessagePanel;
    public TextMeshProUGUI completionMessageText;
    public void UpdateMissionUI(List<Mission> missions)
    {
        missionText.text = "";
        foreach (var mission in missions)
        {
            Debug.Log("UpdateMissionUI called...");
            missionText.text += mission.title + "\n";
            missionText.text += mission.description + "\n";

            foreach (var item in mission.requiredItems)
            {
                int collectedAmount = mission.collectedItems.ContainsKey(item.Key) ? mission.collectedItems[item.Key] : 0;
                missionText.text += $"{item.Key}: {collectedAmount}/{item.Value}\n";
            }

            missionText.text += mission.isCompleted ? "Status: Klar\n\n" : "Status: Pågående\n\n";
        }
    }

    public void ShowCompletionMessage(Mission mission)
    {
        completionMessagePanel.SetActive(true);
        completionMessageText.text = $"Mission klar: {mission.title}\nDu får {mission.xpReward} XP, {mission.rewardItem}, och {mission.goldReward} guld.";
        Invoke("HideCompletionMessage", 5); // Döljer meddelandet efter 5 sekunder
    }

    private void HideCompletionMessage()
    {
        completionMessagePanel.SetActive(false);
    }
}





// FUNKAR ! MISSION
//    public void UpdateMissionUI(List<Mission> missions)
//    {
//        missionText.text = "";
//        foreach (var mission in missions)
//        {
//            missionText.text += mission.title + "\n";
//            missionText.text += mission.description + "\n";
//            missionText.text += mission.isCompleted ? "Status: Klar\n\n" : "Status: Pågående\n\n";
//        }
//    }

//    public void ShowCompletionMessage(Mission mission)
//    {
//        completionMessagePanel.SetActive(true);
//        completionMessageText.text = $"Mission klar: {mission.title}\nDu får {mission.xpReward} XP, {mission.rewardItem}, och {mission.goldReward} guld.";
//        Invoke("HideCompletionMessage", 5); // Döljer meddelandet efter 5 sekunder
//    }

//    private void HideCompletionMessage()
//    {
//        completionMessagePanel.SetActive(false);
//    }
//}
