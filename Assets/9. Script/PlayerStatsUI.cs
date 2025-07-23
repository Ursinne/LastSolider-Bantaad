using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public Text levelText;
    public Text xpText;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        UpdatePlayerStatsUI();
    }

    void Update()
    {
        UpdatePlayerStatsUI();
    }

    void UpdatePlayerStatsUI()
    {
        levelText.text = "Nivå: " + playerStats.level;
        xpText.text = "XP: " + playerStats.currentXP + "/" + playerStats.xpToNextLevel;
    }
}
