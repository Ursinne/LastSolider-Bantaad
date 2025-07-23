using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] float Exp;
    public float maxExp = 500;
    public int level = 1;
    public int maxLevel = 10;
    public int xpToNextLevel = 100;
    public Slider experienceBar;
    public TextMeshProUGUI levelText;             // Använd TextMeshProUGUI för levelText
    public TextMeshProUGUI messageText;          // Ny TextMeshProUGUI-komponent för meddelanden
    public TextMeshProUGUI expToNextLevelText;  // Ny TextMeshProUGUI-komponent för erfarenhetspoäng till nästa nivå


    void Start()
    {
        Exp = 0;
        // Initial slider and text values
        SetExperience(Exp);
        SetLevel(level);
        expToNextLevelText.text = " " + (xpToNextLevel);
        messageText.text = "";
    }

    void Update()
    {
        // Update experience slider
        SetExperience(Exp);

        // Uppdatera text för erfarenhetspoäng till nästa nivå
        expToNextLevelText.text = " " + (xpToNextLevel - Exp);

        // Exp SKILL
        if (Input.GetKeyDown(KeyCode.L))
        {
            HandleSkill();
        }

        // EXP MOVEMENT
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            HandleMovement();
        }

        // EXP JUMP
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
        }

        // Check for Level Up
        if (Exp >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void HandleSkill()
    {
        Exp += 0.2f;
    }

    public void HandleJump()
    {
        Exp += 10.2f;
    }

    public void HandleMovement()
    {
        Exp += 1.1f * Time.deltaTime;
    }

    public void SetExperience(float exp)
    {
        experienceBar.value = exp / xpToNextLevel;
    }

    public void SetLevel(int level)
    {
        levelText.text = " " + level;
    }

    public void LevelUp()
    {
        Exp -= xpToNextLevel;
        level++;
        xpToNextLevel += 100; // Increase XP required for next level
        SetLevel(level);
        ShowMessage(" You have now reached level " + level + ""); // Visa meddelande om nivåuppgång
    }
    public void ShowMessage(string message)
    {
            messageText.text = message;
            StartCoroutine(ClearMessageAfterDelay(2.0f)); // Meddelandet försvinner efter 2 sekunder } private IEnumerator ClearMessageAfterDelay(float delay)
    }
    private IEnumerator ClearMessageAfterDelay(float delay)
    { 
        yield return new WaitForSeconds(delay); 
        messageText.text = "";
    }
}