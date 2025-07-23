using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int skillPoints = 0;

    public void GainXP(int amount)
    {
        currentXP += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (currentXP >= xpToNextLevel)
        {
            level++;
            currentXP -= xpToNextLevel;
            xpToNextLevel += xpToNextLevel / 2; // Öka XP-kravet för nästa nivå
            skillPoints++; // Ge spelaren en färdighetspoäng
            Debug.Log("Nivå Upp! Nuvarande Nivå: " + level);
        }
    }
}
