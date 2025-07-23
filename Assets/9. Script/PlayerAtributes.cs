using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    // Attributes
    public float maxHunger = 100f, maxThirst = 100f, maxTemp = 100f, maxSickness = 100f, maxAge = 100f, maxSex = 100f;
    public float hunger, thirst, temp, sickness, sex, aging;
    public float health = 100f, maxHealth = 100f;

    public Slider hungerBar;
    public Slider thirstBar;
    public Slider tempBar;
    public Slider sicknessBar;
    public Slider ageBar;
    public Slider sexBar;

    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI sicknessText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI sexText;

    private bool triggeringFireplace;
    private bool isRunning;

    // Referens till animationskontrollern
    private PlayerAnimationController animController;

    // Färdigheter och nivåer
    public int playerLevel = 1;
    public int runningSkillLevel = 1;

    void Start()
    {
        hunger = 0;
        thirst = 0;
        temp = 0;
        sickness = 0;
        aging = 0;
        sex = 0;

        // Initial slider values
        SetHunger(hunger);
        SetThirst(thirst);
        SetTemp(temp);
        SetSickness(sickness);
        SetAge(aging);
        SetSex(sex);

        // Hitta animationskontrollern
        animController = GetComponent<PlayerAnimationController>();
        if (animController == null)
        {
            Debug.LogWarning("PlayerAnimationController saknas på spelaren. Animationer kommer inte spelas.");
        }
    }

    void Update()
    {
        // Update sliders
        SetHunger(hunger);
        SetThirst(thirst);
        SetTemp(temp);
        SetSickness(sickness);
        SetAge(aging);
        SetSex(sex);

        // Modifiering av attributshastighet baserat på nivåer och färdigheter
        float levelModifier = 1 - (playerLevel * 0.05f);
        float skillModifier = 1 - (runningSkillLevel * 0.03f);

        // Hunger
        if (hunger < maxHunger)
            hunger += 0.02f * Time.deltaTime * levelModifier * skillModifier;

        // Thirst
        if (thirst < maxThirst)
            thirst += 0.02f * Time.deltaTime * levelModifier * skillModifier;

        // Aging
        if (aging < maxAge)
            aging += 0.002f * Time.deltaTime * levelModifier * skillModifier;

        // Sex
        if (sex < maxSex)
            sex += 0.002f * Time.deltaTime * levelModifier * skillModifier;

        // Coldness
        if (triggeringFireplace && temp > 0)
        {
            temp -= 1 * Time.deltaTime;
        }
        if (!triggeringFireplace && !isRunning)
        {
            temp += 1 * Time.deltaTime;
        }
        if (isRunning)
        {
            temp -= 1 * Time.deltaTime;
        }

        // Sickness
        if (sickness < maxSickness)
        {
            // Logik för att öka sickness baserat på spelvillkor
        }

        // Knappar för att testa äta och dricka funktioner
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EatFood();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DrinkWater();
        }

        // Die if hunger or thirst is too high
        if (hunger >= maxHunger || thirst >= maxThirst || sickness >= maxSickness)
            Die();
    }

    // I PlayerAttributes.cs
    void EatFood()
    {
        hunger = Mathf.Max(0, hunger - 50);
        health = Mathf.Min(health + 20, maxHealth);
        sickness = Mathf.Max(sickness - 10, 0);
        aging = Mathf.Max(aging - 5, 0);

        Debug.Log("Äter mat: Hunger minskad till " + hunger);

        // Använd den generella metoden
        if (animController != null)
        {
            animController.PlayAnimation("isEating", 3.0f);
        }
        else
        {
            Debug.LogError("animController är null, kan inte spela äta-animation");
        }
    }

    void DrinkWater()
    {
        thirst = Mathf.Max(0, thirst - 25);
        health = Mathf.Min(health + 10, maxHealth);
        sickness = Mathf.Max(sickness - 5, 0);
        aging = Mathf.Max(aging - 2, 0);

        Debug.Log("Dricker vatten: Törst minskad till " + thirst);

        // Använd den generella metoden
        if (animController != null)
        {
            animController.PlayAnimation("isDrinking", 3.0f);
        }
        else
        {
            Debug.LogError("animController är null, kan inte spela drick-animation");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            EatFood();
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Water"))
        {
            DrinkWater();
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Sex"))
        {
            sex -= 100;
        }

        if (other.CompareTag("Fireplace"))
            triggeringFireplace = true;

        if (other.CompareTag("BadFood"))
        {
            sickness += 25; // Ökar sjukdom när man äter dålig mat
            Destroy(other.gameObject);
        }

        if (other.CompareTag("ColdEnvironment"))
        {
            temp += 1; // Ökar kyla när man har för lite kläder
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fireplace"))
            triggeringFireplace = false;

        if (other.CompareTag("ColdEnvironment"))
        {
            temp -= 50; // Minskar kyla när man lämnar kall miljö
        }
    }

    void Die()
    {
        Destroy(gameObject);
        print("You are dead because of thirst or hunger");
    }

    public void SetHunger(float hunger)
    {
        hungerBar.value = hunger / maxHunger;
        hungerText.text = "Hunger";
    }

    public void SetThirst(float thirst)
    {
        thirstBar.value = thirst / maxThirst;
        thirstText.text = "Thirst";
    }

    public void SetTemp(float coldness)
    {
        tempBar.value = coldness / maxTemp;
        tempText.text = "Temp";
    }

    public void SetSickness(float sickness)
    {
        sicknessBar.value = sickness / maxSickness;
        sicknessText.text = "Sickness";
    }

    public void SetAge(float age)
    {
        ageBar.value = age / maxAge;
        ageText.text = "Age";
    }

    public void SetSex(float sex)
    {
        sexBar.value = sex / maxSex;
        sexText.text = "Sex";
    }

    // Metoder för att öka nivåer och färdigheter
    public void IncreasePlayerLevel()
    {
        playerLevel++;
    }

    public void IncreaseRunningSkillLevel()
    {
        runningSkillLevel++;
    }
}