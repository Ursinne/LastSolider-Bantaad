using UnityEngine;
using System.Collections.Generic;

public class MissionDatabase : MonoBehaviour
{
    public List<Mission> missions = new List<Mission>();

    private void Start()
    {
        InitializeMissions();
    }

    public void InitializeMissions()
    {
        // Exempel på olika missions
        Mission collectWoodMission = new Mission(
            "Samla 10 ved",
            "Samla ihop 10 ved från skogen.",
            new Dictionary<string, int> { { "Wood", 10 } },
            100,  // XP belöning
            "Wood Bundle",  // Föremålsbelöning
            50  // Guld belöning
        );

        Mission killEnemyMission = new Mission(
            "Döda en fiende",
            "Döda en fiende för att visa din styrka.",
            new Dictionary<string, int> { { "Enemy", 1 } },
            200,  // XP belöning
            "Enemy Weapon",  // Föremålsbelöning
            100  // Guld belöning
        );

        Mission craftAxeMission = new Mission(
            "Crafta en yxa",
            "Samla 2 ved, 2 sten och 1 tejp för att crafta en yxa.",
            new Dictionary<string, int> { { "Wood", 2 }, { "Stone", 2 }, { "Tape", 1 } },
            300,  // XP belöning
            "Axe",  // Föremålsbelöning
            150  // Guld belöning
        );

        missions.Add(collectWoodMission);
        missions.Add(killEnemyMission);
        missions.Add(craftAxeMission);

        Debug.Log("Missions initialized in MissionDatabase. Total missions: " + missions.Count);
    }

    public Mission GetMissionByIndex(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            return missions[index];
        }
        return null;
    }
}
