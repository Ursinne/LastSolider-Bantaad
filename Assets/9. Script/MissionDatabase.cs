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
        // Exempel p� olika missions
        Mission collectWoodMission = new Mission(
            "Samla 10 ved",
            "Samla ihop 10 ved fr�n skogen.",
            new Dictionary<string, int> { { "Wood", 10 } },
            100,  // XP bel�ning
            "Wood Bundle",  // F�rem�lsbel�ning
            50  // Guld bel�ning
        );

        Mission killEnemyMission = new Mission(
            "D�da en fiende",
            "D�da en fiende f�r att visa din styrka.",
            new Dictionary<string, int> { { "Enemy", 1 } },
            200,  // XP bel�ning
            "Enemy Weapon",  // F�rem�lsbel�ning
            100  // Guld bel�ning
        );

        Mission craftAxeMission = new Mission(
            "Crafta en yxa",
            "Samla 2 ved, 2 sten och 1 tejp f�r att crafta en yxa.",
            new Dictionary<string, int> { { "Wood", 2 }, { "Stone", 2 }, { "Tape", 1 } },
            300,  // XP bel�ning
            "Axe",  // F�rem�lsbel�ning
            150  // Guld bel�ning
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
