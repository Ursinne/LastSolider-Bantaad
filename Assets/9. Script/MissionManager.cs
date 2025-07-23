using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public List<Mission> activeMissions = new List<Mission>();
    public MissionUIController missionUIController;

    private void Start()
    {
        if (missionUIController == null)
        {
            Debug.LogError("MissionUIController could not be found on MissionUIPanel!");
            return;
        }

        InitializeMissions();

        Debug.Log("Missions added to MissionManager. Active missions: " + activeMissions.Count);

        missionUIController.UpdateMissionUI(activeMissions);
    }

    private void InitializeMissions()
    {
        // Exempel på olika missioner
        var collectWoodMission = new Mission(
            "Samla 10 ved",
            "Samla ihop 10 ved från skogen.",
            new Dictionary<string, int> { { "Wood", 10 } },
            100,  // XP belöning
            "Wood Bundle",  // Föremålsbelöning
            50  // Guld belöning
        );
        var collectFoodMission = new Mission(
            "Samla 2 Food", "Samla ihop 2 Food",
            new Dictionary<string, int> { { "Food", 2 } },
            150, // XP belöning
            "Mätt i magen", // Föremålsbelöning
             75 // Guld belöning );

                );
        var collectStoneMission = new Mission(
            "Samla 2 Stone", "Samla ihop 2 Stone",
            new Dictionary<string, int> { { "Stone", 2 } },
            150, // XP belöning
            "Stark du är", // Föremålsbelöning
             75 // Guld belöning );

         );

        activeMissions.Add(collectWoodMission);
        activeMissions.Add(collectFoodMission);
        activeMissions.Add(collectStoneMission);
    }

    public void UpdateMissionProgress(string itemName, int amount)
    {
        Debug.Log($"UpdateMissionProgress called with item: {itemName}, amount: {amount}");

        foreach (Mission mission in activeMissions)
        {
            Debug.Log($"Checking mission: {mission.title}");
            if (!mission.isCompleted && mission.requiredItems.ContainsKey(itemName))
            {
                if (mission.collectedItems.ContainsKey(itemName))
                {
                    mission.collectedItems[itemName] += amount;
                }
                else
                {
                    mission.collectedItems[itemName] = amount;
                }

                // Logga den aktuella statusen för insamlade föremål
                Debug.Log($"Collected {itemName}: {mission.collectedItems[itemName]}/{mission.requiredItems[itemName]}");

                if (IsMissionComplete(mission))
                {
                    mission.isCompleted = true;
                    CompleteMission(mission);
                }
                missionUIController.UpdateMissionUI(activeMissions);
            }
            else
            {
                // Logga om föremålet inte finns i missionens krav
                Debug.Log($"Item {itemName} not found in mission required items or mission already completed.");
            }
        }
    }

    private bool IsMissionComplete(Mission mission)
    {
        foreach (var item in mission.requiredItems)
        {
            if (!mission.collectedItems.ContainsKey(item.Key) || mission.collectedItems[item.Key] < item.Value)
            {
                return false;
            }
        }
        return true;
    }

    private void CompleteMission(Mission mission)
    {
        Debug.Log($"Mission completed: {mission.title}. Player receives {mission.xpReward} XP, {mission.rewardItem}, and {mission.goldReward} gold");
        missionUIController.UpdateMissionUI(activeMissions);
        missionUIController.ShowCompletionMessage(mission);
    }
}









//// Exempel på olika missions
//Mission collectWoodMission = new Mission(
//        "Samla 10 ved",
//        "Samla ihop 10 ved från skogen.",
//        new Dictionary<string, int> { { "Wood", 10 } },
//        100,  // XP belöning
//        "Wood Bundle",  // Föremålsbelöning
//        50  // Guld belöning
//    );

//Mission killEnemyMission = new Mission(
//    "Döda en fiende",
//    "Döda en fiende för att visa din styrka.",
//    new Dictionary<string, int> { { "Enemy", 1 } },
//    200,  // XP belöning
//    "Enemy Weapon",  // Föremålsbelöning
//    100  // Guld belöning
//);

//Mission craftAxeMission = new Mission(
//    "Crafta en yxa",
//    "Samla 2 ved, 2 sten och 1 tejp för att crafta en yxa.",
//    new Dictionary<string, int> { { "Wood", 2 }, { "Stone", 2 }, { "Tape", 1 } },
//    300,  // XP belöning
//    "Axe",  // Föremålsbelöning
//    150  // Guld belöning
//);


//FUNKAR 1 MISSION
//using System.Collections.Generic;
//using UnityEngine;

//public class MissionManager : MonoBehaviour
//{
//    public List<Mission> activeMissions = new List<Mission>();
//    public MissionUIController missionUIController;

//    private void Start()
//    {
//        if (missionUIController == null)
//        {
//            Debug.LogError("MissionUIController could not be found on MissionUIPanel!");
//            return;
//        }

//        InitializeMissions();

//        Debug.Log("Missions added to MissionManager. Active missions: " + activeMissions.Count);

//        missionUIController.UpdateMissionUI(activeMissions);
//    }

//    private void InitializeMissions()
//    {
//        Exempel på olika missioner
//        var collectWoodMission = new Mission(
//            "Samla 10 ved",
//            "Samla ihop 10 ved från skogen.",
//            new Dictionary<string, int> { { "Wood", 10 } },
//            100,
//            "Wood Bundle",
//            50
//        );

//        var killEnemyMission = new Mission(
//            "Döda en fiende",
//            "Döda en fiende för att visa din styrka.",
//            new Dictionary<string, int> { { "Enemy", 1 } },
//            200,
//            "Enemy Weapon",
//            100
//        );

//        var craftAxeMission = new Mission(
//            "Crafta en yxa",
//            "Samla 2 ved, 2 sten och 1 tejp för att crafta en yxa.",
//            new Dictionary<string, int> { { "Wood", 2 }, { "Stone", 2 }, { "Tape", 1 } },
//            300,
//            "Axe",
//            150
//        );

//        activeMissions.Add(collectWoodMission);
//        activeMissions.Add(killEnemyMission);
//        activeMissions.Add(craftAxeMission);
//    }

//    public void UpdateMissionProgress(string itemName, int amount)
//    {
//        foreach (Mission mission in activeMissions)
//        {
//            if (!mission.isCompleted && mission.requiredItems.ContainsKey(itemName))
//            {
//                if (mission.collectedItems.ContainsKey(itemName))
//                {
//                    mission.collectedItems[itemName] += amount;
//                }
//                else
//                {
//                    mission.collectedItems[itemName] = amount;
//                }

//                if (IsMissionComplete(mission))
//                {
//                    mission.isCompleted = true;
//                    CompleteMission(mission);
//                }
//                missionUIController.UpdateMissionUI(activeMissions);
//            }
//        }
//    }

//    private bool IsMissionComplete(Mission mission)
//    {
//        foreach (var item in mission.requiredItems)
//        {
//            if (mission.collectedItems.ContainsKey(item.Key) && mission.collectedItems[item.Key] < item.Value)
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    private void CompleteMission(Mission mission)
//    {
//        Debug.Log($"Mission completed: {mission.title}. Player receives {mission.xpReward} XP, {mission.rewardItem}, and {mission.goldReward} gold");
//        missionUIController.UpdateMissionUI(activeMissions);
//        missionUIController.ShowCompletionMessage(mission);
//    }
//}
