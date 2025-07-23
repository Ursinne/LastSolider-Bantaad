using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkills : MonoBehaviour
{
    // Maxvärden för färdigheter
    public float maxSkillValue = 300f;

    // Erfarenhet till nästa nivå för varje färdighet
    public float expToNextLevel = 100f;

    // Ordna färdigheter enligt kategorier - exakt 5 skills per kategori
    private Dictionary<string, Dictionary<string, float>> skillCategories = new Dictionary<string, Dictionary<string, float>>
    {
        {
            "Gathering", new Dictionary<string, float>
            {
                { "logging", 0f },      // Trädfällning
                { "mining", 0f },       // Stenbrytning
                { "foraging", 0f },     // Bärplockning, örtsamling
                { "digging", 0f },      // Grävning
                { "skinning", 0f }      // Skinnflåning
            }
        },
        {
            "Crafting", new Dictionary<string, float>
            {
                { "woodworking", 0f },   // Trähantverk
                { "stoneworking", 0f },  // Stenhantverk (inkl. metallarbete)
                { "leatherworking", 0f }, // Läderarbete
                { "cooking", 0f },       // Matlagning (inkl. alkemi/brygd)
                { "building", 0f }       // Byggnadskonst
            }
        },
        {
            "Survival", new Dictionary<string, float>
            {
                { "hunting", 0f },       // Jakt
                { "fishing", 0f },       // Fiske
                { "willpower", 0f },     // Viljestyrka
                { "tracking", 0f },      // Spårning
                { "scavenging", 0f }     // Söka förnödenheter
            }
        },
        {
            "Movement", new Dictionary<string, float>
            {
                { "running", 0f },       // Löpning
                { "athletics", 0f },     // Allmän rörlighet
                { "sneaking", 0f },      // Smyga
                { "swimming", 0f },      // Simning
                { "stamina", 0f }        // Uthållighet
            }
        },
        {
            "Combat", new Dictionary<string, float>
            {
                { "melee", 0f },         // Närstrid
                { "ranged", 0f },        // Distansstrid
                { "defense", 0f },       // Försvar
                { "firearms", 0f },      // Skjutvapen
                { "traps", 0f }          // Fällor
            }
        }
    };

    // Ordbok för att mappa färdigheter till visningsnamn
    private Dictionary<string, string> skillDisplayNames = new Dictionary<string, string>
    {
        // Gathering
        { "logging", "Logging" },
        { "mining", "Mining" },
        { "foraging", "Foraging" },
        { "digging", "Digging" },
        { "skinning", "Skinning" },
        
        // Crafting
        { "woodworking", "Woodworking" },
        { "stoneworking", "Stoneworking" },
        { "leatherworking", "Leatherworking" },
        { "cooking", "Cooking" },
        { "building", "Building" },
        
        // Survival
        { "hunting", "Hunting" },
        { "fishing", "Fishing" },
        { "willpower", "Willpower" },
        { "tracking", "Tracking" },
        { "scavenging", "Scavenging" },
        
        // Movement
        { "running", "Running" },
        { "athletics", "Athletics" },
        { "sneaking", "Sneaking" },
        { "swimming", "Swimming" },
        { "stamina", "Stamina" },
        
        // Combat
        { "melee", "Melee" },
        { "ranged", "Ranged" },
        { "defense", "Defence" },
        { "firearms", "Firearms" },
        { "traps", "Traps" }
    };

    // Samlingshastighet per färdighet
    private Dictionary<string, float> collectionRates = new Dictionary<string, float>();

    // Materialmängdsmultiplikator per färdighet
    private Dictionary<string, float> materialAmountMultiplier = new Dictionary<string, float>();

    // Nivåer per färdighet
    private Dictionary<string, int> levels = new Dictionary<string, int>();

    // Erfarenhet för nästa nivå per färdighet
    private Dictionary<string, float> expToNextLevelDict = new Dictionary<string, float>();

    // Nivå-text komponenter
    [Header("Gathering Text")]
    public TextMeshProUGUI loggingLevelText;
    public TextMeshProUGUI miningLevelText;
    public TextMeshProUGUI foragingLevelText;
    public TextMeshProUGUI diggingLevelText;
    public TextMeshProUGUI skinningLevelText;

    // UI-komponenter (Sliders)
    [Header("Gathering UI")]
    public Slider loggingSlider;
    public Slider miningSlider;
    public Slider foragingSlider;
    public Slider diggingSlider;
    public Slider skinningSlider;

    [Header("Crafting Text")]
    public TextMeshProUGUI woodworkingLevelText;
    public TextMeshProUGUI stoneworkingLevelText;
    public TextMeshProUGUI leatherworkingLevelText;
    public TextMeshProUGUI cookingLevelText;
    public TextMeshProUGUI buildingLevelText;

    [Header("Crafting UI")]
    public Slider woodworkingSlider;
    public Slider stoneworkingSlider;
    public Slider leatherworkingSlider;
    public Slider cookingSlider;
    public Slider buildingSlider;

    [Header("Survival Text")]
    public TextMeshProUGUI huntingLevelText;
    public TextMeshProUGUI fishingLevelText;
    public TextMeshProUGUI willpowerLevelText;
    public TextMeshProUGUI trackingLevelText;
    public TextMeshProUGUI scavengingLevelText;

    [Header("Survival UI")]
    public Slider huntingSlider;
    public Slider fishingSlider;
    public Slider willpowerSlider;
    public Slider trackingSlider;
    public Slider scavengingSlider;

    [Header("Movement Text")]
    public TextMeshProUGUI runningLevelText;
    public TextMeshProUGUI athleticsLevelText;
    public TextMeshProUGUI sneakingLevelText;
    public TextMeshProUGUI swimmingLevelText;
    public TextMeshProUGUI staminaLevelText;

    [Header("Movement UI")]
    public Slider runningSlider;
    public Slider athleticsSlider;
    public Slider sneakingSlider;
    public Slider swimmingSlider;
    public Slider staminaSlider;

    [Header("Combat Text")]
    public TextMeshProUGUI meleeLevelText;
    public TextMeshProUGUI rangedLevelText;
    public TextMeshProUGUI defenseLevelText;
    public TextMeshProUGUI firearmsLevelText;
    public TextMeshProUGUI trapsLevelText;

    [Header("Combat UI")]
    public Slider meleeSlider;
    public Slider rangedSlider;
    public Slider defenseSlider;
    public Slider firearmsSlider;
    public Slider trapsSlider;

    [Header("UI Elements")]
    public TextMeshProUGUI messageText; // Text-komponent för meddelanden

    // En mappning för att konvertera gamla färdigheter till nya (för kompatibilitet)
    private Dictionary<string, string> oldToNewSkillMapping = new Dictionary<string, string>
    {
        { "wood", "logging" },
        { "stone", "mining" },
        { "iron", "stoneworking" },
        { "leather", "skinning" },
        { "soil", "digging" },
        { "bone", "foraging" },
        { "sand", "digging" },
        { "glass", "stoneworking" },
        { "claying", "stoneworking" },
        { "cooking", "cooking" },
        { "fishing", "fishing" },
        { "hunting", "hunting" },
        { "running", "running" },
        { "jumping", "athletics" },
        { "sneaking", "sneaking" },
        { "swimming", "swimming" },
        { "armed", "melee" },
        { "fist", "melee" },
        { "range", "ranged" },
        { "sheild", "defense" },
        { "avoid", "defense" },
        { "block", "defense" }
    };

    void Start()
    {
        InitializeSkills();
        UpdateSkillSliders();
        UpdateSkillLevelTexts();
        messageText.text = ""; // Initiera meddelandetext till tomt
    }

    void InitializeSkills()
    {
        // Initiera alla dictionaries
        foreach (var category in skillCategories)
        {
            foreach (var skill in category.Value)
            {
                string skillName = skill.Key;

                // Sätt standardvärden för alla färdigheter
                levels[skillName] = 1;
                expToNextLevelDict[skillName] = expToNextLevel;
                materialAmountMultiplier[skillName] = 1f;
                collectionRates[skillName] = 1f;
            }
        }
    }

    void Update()
    {
        // Exempel för test
        if (Input.GetKeyDown(KeyCode.L))
        {
            UseItem("axe"); // Exempel: Använd en yxa
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            UseItem("shovel"); // Exempel: Använd en spade
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseItem("fishing_rod"); // Exempel: Använd ett fiskespö
        }
    }

    private void UseItem(string itemName)
    {
        switch (itemName)
        {
            case "axe":
                GainSkillExp("logging", 10f * materialAmountMultiplier["logging"]);
                break;
            case "pickaxe":
                GainSkillExp("mining", 10f * materialAmountMultiplier["mining"]);
                break;
            case "shovel":
                GainSkillExp("digging", 10f * materialAmountMultiplier["digging"]);
                break;
            case "fishing_rod":
                GainSkillExp("fishing", 10f * materialAmountMultiplier["fishing"]);
                break;
            case "hunting_bow":
                GainSkillExp("hunting", 10f * materialAmountMultiplier["hunting"]);
                GainSkillExp("ranged", 5f * materialAmountMultiplier["ranged"]);
                break;
            case "sword":
                GainSkillExp("melee", 10f * materialAmountMultiplier["melee"]);
                break;
            case "shield":
                GainSkillExp("defense", 10f * materialAmountMultiplier["defense"]);
                break;
            case "hammer":
                GainSkillExp("building", 10f * materialAmountMultiplier["building"]);
                GainSkillExp("woodworking", 5f * materialAmountMultiplier["woodworking"]);
                break;
            case "cooking_pot":
                GainSkillExp("cooking", 10f * materialAmountMultiplier["cooking"]);
                break;
                // Lägg till fler föremål efter behov
        }

        UpdateSkillSliders();
        UpdateSkillLevelTexts();
    }

    // Metod för att få kategori för en färdighet
    private string GetCategoryForSkill(string skillName)
    {
        foreach (var category in skillCategories)
        {
            if (category.Value.ContainsKey(skillName))
                return category.Key;
        }
        return null;
    }

    private void GainSkillExp(string skillName, float expGain)
    {
        // Hitta kategorin för denna färdighet
        string category = GetCategoryForSkill(skillName);
        if (category == null)
        {
            Debug.LogError("Skill not found in any category: " + skillName);
            return;
        }

        // Lägg till erfarenhetspoäng till färdigheten
        skillCategories[category][skillName] += expGain * levels[skillName];

        // Hitta motsvarande slider
        Slider skillSlider = GetSliderForSkill(skillName);
        if (skillSlider != null)
        {
            skillSlider.value = skillCategories[category][skillName];
        }

        // Kontrollera om nivå ska ökas
        if (skillCategories[category][skillName] >= expToNextLevelDict[skillName])
        {
            LevelUpSkill(category, skillName);
        }
    }

    private void LevelUpSkill(string category, string skillName)
    {
        // Öka nivå
        levels[skillName]++;

        // Återställ erfarenhet och öka krav för nästa nivå
        skillCategories[category][skillName] = 0;
        expToNextLevelDict[skillName] *= 1.5f;

        // Uppdatera slider max value
        Slider skillSlider = GetSliderForSkill(skillName);
        if (skillSlider != null)
        {
            skillSlider.maxValue = expToNextLevelDict[skillName];
            skillSlider.value = 0;
        }

        // Förbättra samlingshastigheten och materialmängdsmultiplikatorn
        collectionRates[skillName] *= 1.2f;
        materialAmountMultiplier[skillName] *= 1.1f;

        // Visa meddelande med namnet
        string displayName = GetDisplayName(skillName);
        ShowMessage($"Du har nått nivå {levels[skillName]} i {displayName}!");

        // Uppdatera texterna
        UpdateSkillLevelTexts();
    }

    // Returnera visningsnamnet för en färdighet
    private string GetDisplayName(string skillName)
    {
        if (skillDisplayNames.TryGetValue(skillName, out string displayName))
        {
            return displayName;
        }
        return skillName; // Fallback till interna namnet om inget displaynamn finns
    }

    // Hitta slider för en given färdighet
    private Slider GetSliderForSkill(string skillName)
    {
        switch (skillName)
        {
            // Gathering
            case "logging": return loggingSlider;
            case "mining": return miningSlider;
            case "foraging": return foragingSlider;
            case "digging": return diggingSlider;
            case "skinning": return skinningSlider;

            // Crafting
            case "woodworking": return woodworkingSlider;
            case "stoneworking": return stoneworkingSlider;
            case "leatherworking": return leatherworkingSlider;
            case "cooking": return cookingSlider;
            case "building": return buildingSlider;

            // Survival
            case "hunting": return huntingSlider;
            case "fishing": return fishingSlider;
            case "willpower": return willpowerSlider;
            case "tracking": return trackingSlider;
            case "scavenging": return scavengingSlider;

            // Movement
            case "running": return runningSlider;
            case "athletics": return athleticsSlider;
            case "sneaking": return sneakingSlider;
            case "swimming": return swimmingSlider;
            case "stamina": return staminaSlider;

            // Combat
            case "melee": return meleeSlider;
            case "ranged": return rangedSlider;
            case "defense": return defenseSlider;
            case "firearms": return firearmsSlider;
            case "traps": return trapsSlider;

            default: return null;
        }
    }

    // Hitta TextMeshProUGUI för en given färdighet
    private TextMeshProUGUI GetTextComponentForSkill(string skillName)
    {
        switch (skillName)
        {
            // Gathering
            case "logging": return loggingLevelText;
            case "mining": return miningLevelText;
            case "foraging": return foragingLevelText;
            case "digging": return diggingLevelText;
            case "skinning": return skinningLevelText;

            // Crafting
            case "woodworking": return woodworkingLevelText;
            case "stoneworking": return stoneworkingLevelText;
            case "leatherworking": return leatherworkingLevelText;
            case "cooking": return cookingLevelText;
            case "building": return buildingLevelText;

            // Survival
            case "hunting": return huntingLevelText;
            case "fishing": return fishingLevelText;
            case "willpower": return willpowerLevelText;
            case "tracking": return trackingLevelText;
            case "scavenging": return scavengingLevelText;

            // Movement
            case "running": return runningLevelText;
            case "athletics": return athleticsLevelText;
            case "sneaking": return sneakingLevelText;
            case "swimming": return swimmingLevelText;
            case "stamina": return staminaLevelText;

            // Combat
            case "melee": return meleeLevelText;
            case "ranged": return rangedLevelText;
            case "defense": return defenseLevelText;
            case "firearms": return firearmsLevelText;
            case "traps": return trapsLevelText;

            default: return null;
        }
    }

    // Uppdatera alla sliders baserat på nuvarande värden
    private void UpdateSkillSliders()
    {
        foreach (var category in skillCategories)
        {
            foreach (var skill in category.Value)
            {
                Slider slider = GetSliderForSkill(skill.Key);
                if (slider != null)
                {
                    slider.value = skill.Value;
                    slider.maxValue = expToNextLevelDict[skill.Key];
                }
            }
        }
    }

    // Uppdatera alla nivåtexter med önskat format
    private void UpdateSkillLevelTexts()
    {
        foreach (var category in skillCategories)
        {
            foreach (var skill in category.Value.Keys)
            {
                TextMeshProUGUI textComponent = GetTextComponentForSkill(skill);
                if (textComponent != null)
                {
                    string displayName = GetDisplayName(skill);
                    textComponent.text = displayName + " Lvl " + levels[skill];
                }
            }
        }
    }

    // Konvertera från gamla till nya färdigheter (för kompatibilitet med tidigare kod)
    public void GainOldSkillExp(string oldSkillName, float expGain)
    {
        if (oldToNewSkillMapping.TryGetValue(oldSkillName, out string newSkillName))
        {
            GainSkillExp(newSkillName, expGain);
        }
        else
        {
            Debug.LogWarning("Inget nytt färdighetsnamn hittades för: " + oldSkillName);
        }
    }

    // Visa meddelande till spelaren
    public void ShowMessage(string message)
    {
        messageText.text = message;
        StartCoroutine(ClearMessageAfterDelay(3.0f)); // Meddelandet försvinner efter 3 sekunder
    }

    private IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }

    // Publik metod för att hämta en färdighets nivå
    public int GetSkillLevel(string skillName)
    {
        if (levels.ContainsKey(skillName))
        {
            return levels[skillName];
        }

        // Försök konvertera från gammalt till nytt namn
        if (oldToNewSkillMapping.TryGetValue(skillName, out string newSkillName))
        {
            if (levels.ContainsKey(newSkillName))
            {
                return levels[newSkillName];
            }
        }

        Debug.LogWarning("Försöker hämta nivå för okänd färdighet: " + skillName);
        return 0;
    }

    // Publik metod för att hämta en färdighets multiplikator
    public float GetMaterialMultiplier(string skillName)
    {
        if (materialAmountMultiplier.ContainsKey(skillName))
        {
            return materialAmountMultiplier[skillName];
        }

        // Försök konvertera från gammalt till nytt namn
        if (oldToNewSkillMapping.TryGetValue(skillName, out string newSkillName))
        {
            if (materialAmountMultiplier.ContainsKey(newSkillName))
            {
                return materialAmountMultiplier[newSkillName];
            }
        }

        Debug.LogWarning("Försöker hämta multiplikator för okänd färdighet: " + skillName);
        return 1.0f;
    }
}
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;

//public class PlayerSkills : MonoBehaviour
//{
//    // Maxvärden för färdigheter
//    public float maxSkillValue = 300f;

//    // Erfarenhet till nästa nivå för varje färdighet
//    public float expToNextLevel = 100f;

//    // Ordna färdigheter enligt kategorier
//    private Dictionary<string, Dictionary<string, float>> skillCategories = new Dictionary<string, Dictionary<string, float>>
//    {
//        {
//            "Gathering", new Dictionary<string, float>
//            {
//                { "logging", 0f },      // Trädfällning
//                { "mining", 0f },       // Stenbrytning
//                { "foraging", 0f },     // Bärplockning, örtsamling
//                { "digging", 0f },      // Grävning (ersätter excavation)
//                { "skinning", 0f }      // Skinnflåning
//            }
//        },
//        {
//            "Crafting", new Dictionary<string, float>
//            {
//                { "woodworking", 0f },   // Trähantverk
//                { "stoneworking", 0f },  // Stenhantverk
//                { "metalworking", 0f },  // Metallarbete
//                { "leatherworking", 0f }, // Läderarbete
//                { "cooking", 0f },       // Matlagning
//                { "alchemy", 0f }        // Alkemi (ny)
//            }
//        },
//        {
//            "Survival", new Dictionary<string, float>
//            {
//                { "hunting", 0f },       // Jakt
//                { "fishing", 0f },       // Fiske
//                { "fire_making", 0f },   // Eldhantering (ny)
//                { "medicine", 0f },      // Medicin (ny)
//                { "weather_adaptation", 0f } // Klimatanpassning (ny)
//            }
//        },
//        {
//            "Movement", new Dictionary<string, float>
//            {
//                { "agility", 0f },      // Smidighet
//                { "sneaking", 0f },     // Smygande
//                { "swimming", 0f },     // Simning
//                { "climbing", 0f }      // Klättring (ny)
//            }
//        },
//        {
//            "Combat", new Dictionary<string, float>
//            {
//                { "melee", 0f },        // Närstrid
//                { "ranged", 0f },       // Distansstrid
//                { "defense", 0f }       // Försvar
//            }
//        }
//    };

//    // Samlingshastighet per färdighet
//    private Dictionary<string, float> collectionRates = new Dictionary<string, float>();

//    // Materialmängdsmultiplikator per färdighet
//    private Dictionary<string, float> materialAmountMultiplier = new Dictionary<string, float>();

//    // Nivåer per färdighet
//    private Dictionary<string, int> levels = new Dictionary<string, int>();

//    // Erfarenhet för nästa nivå per färdighet
//    private Dictionary<string, float> expToNextLevelDict = new Dictionary<string, float>();

//    // UI-komponenter (Sliders)
//    [Header("Gathering UI")]
//    public Slider loggingSlider;
//    public Slider miningSlider;
//    public Slider foragingSlider;
//    public Slider diggingSlider;
//    public Slider skinningSlider;

//    [Header("Crafting UI")]
//    public Slider woodworkingSlider;
//    public Slider stoneworkingSlider;
//    public Slider metalworkingSlider;
//    public Slider leatherworkingSlider;
//    public Slider cookingSlider;
//    public Slider alchemySlider;

//    [Header("Survival UI")]
//    public Slider huntingSlider;
//    public Slider fishingSlider;
//    public Slider fireMakingSlider;
//    public Slider medicineSlider;
//    public Slider weatherAdaptationSlider;

//    [Header("Movement UI")]
//    public Slider agilitySlider;
//    public Slider sneakingSlider;
//    public Slider swimmingSlider;
//    public Slider climbingSlider;

//    [Header("Combat UI")]
//    public Slider meleeSlider;
//    public Slider rangedSlider;
//    public Slider defenseSlider;

//    // Nivå-text komponenter
//    [Header("Gathering Text")]
//    public TextMeshProUGUI loggingLevelText;
//    public TextMeshProUGUI miningLevelText;
//    public TextMeshProUGUI foragingLevelText;
//    public TextMeshProUGUI diggingLevelText;
//    public TextMeshProUGUI skinningLevelText;

//    [Header("Crafting Text")]
//    public TextMeshProUGUI woodworkingLevelText;
//    public TextMeshProUGUI stoneworkingLevelText;
//    public TextMeshProUGUI metalworkingLevelText;
//    public TextMeshProUGUI leatherworkingLevelText;
//    public TextMeshProUGUI cookingLevelText;
//    public TextMeshProUGUI alchemyLevelText;

//    [Header("Survival Text")]
//    public TextMeshProUGUI huntingLevelText;
//    public TextMeshProUGUI fishingLevelText;
//    public TextMeshProUGUI fireMakingLevelText;
//    public TextMeshProUGUI medicineLevelText;
//    public TextMeshProUGUI weatherAdaptationLevelText;

//    [Header("Movement Text")]
//    public TextMeshProUGUI agilityLevelText;
//    public TextMeshProUGUI sneakingLevelText;
//    public TextMeshProUGUI swimmingLevelText;
//    public TextMeshProUGUI climbingLevelText;

//    [Header("Combat Text")]
//    public TextMeshProUGUI meleeLevelText;
//    public TextMeshProUGUI rangedLevelText;
//    public TextMeshProUGUI defenseLevelText;

//    [Header("UI Elements")]
//    public TextMeshProUGUI messageText; // Text-komponent för meddelanden

//    // En mappning för att konvertera gamla färdigheter till nya (för kompatibilitet)
//    private Dictionary<string, string> oldToNewSkillMapping = new Dictionary<string, string>
//    {
//        { "wood", "logging" },
//        { "stone", "mining" },
//        { "iron", "metalworking" },
//        { "leather", "skinning" },
//        { "soil", "digging" },
//        { "bone", "foraging" },  
//        { "sand", "digging" },
//        { "glass", "stoneworking" },
//        { "claying", "stoneworking" },
//        { "cooking", "cooking" },
//        { "fishing", "fishing" },
//        { "hunting", "hunting" },
//        { "running", "agility" },
//        { "jumping", "agility" },
//        { "sneaking", "sneaking" },
//        { "swimming", "swimming" },
//        { "armed", "melee" },
//        { "fist", "melee" },
//        { "range", "ranged" },
//        { "sheild", "defense" },
//        { "avoid", "defense" },
//        { "block", "defense" }
//    };

//    void Start()
//    {
//        InitializeSkills();
//        UpdateSkillSliders();
//        UpdateSkillLevelTexts();
//        messageText.text = ""; // Initiera meddelandetext till tomt
//    }

//    void InitializeSkills()
//    {
//        // Initiera alla dictionaries
//        foreach (var category in skillCategories)
//        {
//            foreach (var skill in category.Value)
//            {
//                string skillName = skill.Key;

//                // Sätt standardvärden för alla färdigheter
//                levels[skillName] = 1;
//                expToNextLevelDict[skillName] = expToNextLevel;
//                materialAmountMultiplier[skillName] = 1f;
//                collectionRates[skillName] = 1f;
//            }
//        }
//    }

//    void Update()
//    {
//        // Exempel för test
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            UseItem("axe"); // Exempel: Använd en yxa
//        }
//        if (Input.GetKeyDown(KeyCode.P))
//        {
//            UseItem("shovel"); // Exempel: Använd en spade
//        }
//        if (Input.GetKeyDown(KeyCode.F))
//        {
//            UseItem("fishing_rod"); // Exempel: Använd ett fiskespö
//        }
//    }

//    private void UseItem(string itemName)
//    {
//        switch (itemName)
//        {
//            case "axe":
//                GainSkillExp("logging", 10f * materialAmountMultiplier["logging"]);
//                break;
//            case "pickaxe":
//                GainSkillExp("mining", 10f * materialAmountMultiplier["mining"]);
//                break;
//            case "shovel":
//                GainSkillExp("digging", 10f * materialAmountMultiplier["digging"]);
//                break;
//            case "fishing_rod":
//                GainSkillExp("fishing", 10f * materialAmountMultiplier["fishing"]);
//                break;
//            case "hunting_bow":
//                GainSkillExp("hunting", 10f * materialAmountMultiplier["hunting"]);
//                GainSkillExp("ranged", 5f * materialAmountMultiplier["ranged"]);
//                break;
//            case "sword":
//                GainSkillExp("melee", 10f * materialAmountMultiplier["melee"]);
//                break;
//            case "shield":
//                GainSkillExp("defense", 10f * materialAmountMultiplier["defense"]);
//                break;
//            // Lägg till fler föremål efter behov
//        }

//        UpdateSkillSliders();
//        UpdateSkillLevelTexts();
//    }

//    // Metod för att få kategori för en färdighet
//    private string GetCategoryForSkill(string skillName)
//    {
//        foreach (var category in skillCategories)
//        {
//            if (category.Value.ContainsKey(skillName))
//                return category.Key;
//        }
//        return null;
//    }

//    private void GainSkillExp(string skillName, float expGain)
//    {
//        // Hitta kategorin för denna färdighet
//        string category = GetCategoryForSkill(skillName);
//        if (category == null)
//        {
//            Debug.LogError("Skill not found in any category: " + skillName);
//            return;
//        }

//        // Lägg till erfarenhetspoäng till färdigheten
//        skillCategories[category][skillName] += expGain * levels[skillName];

//        // Hitta motsvarande slider
//        Slider skillSlider = GetSliderForSkill(skillName);
//        if (skillSlider != null)
//        {
//            skillSlider.value = skillCategories[category][skillName];
//        }

//        // Kontrollera om nivå ska ökas
//        if (skillCategories[category][skillName] >= expToNextLevelDict[skillName])
//        {
//            LevelUpSkill(category, skillName);
//        }
//    }

//    private void LevelUpSkill(string category, string skillName)
//    {
//        // Öka nivå
//        levels[skillName]++;

//        // Återställ erfarenhet och öka krav för nästa nivå
//        skillCategories[category][skillName] = 0;
//        expToNextLevelDict[skillName] *= 1.5f;

//        // Uppdatera slider max value
//        Slider skillSlider = GetSliderForSkill(skillName);
//        if (skillSlider != null)
//        {
//            skillSlider.maxValue = expToNextLevelDict[skillName];
//            skillSlider.value = 0;
//        }

//        // Förbättra samlingshastigheten och materialmängdsmultiplikatorn
//        collectionRates[skillName] *= 1.2f;
//        materialAmountMultiplier[skillName] *= 1.1f;

//        // Visa meddelande
//        ShowMessage($"Level {levels[skillName]} i {GetSkillDisplayName(skillName)}!");

//        // Uppdatera texterna
//        UpdateSkillLevelTexts();
//    }

//    // Returnera användarvänligt namn för en färdighet
//    private string GetSkillDisplayName(string skillName)
//    {
//        // Konvertera från snake_case till läsbart format med stora bokstäver
//        string displayName = "";
//        string[] words = skillName.Split('_');

//        foreach (string word in words)
//        {
//            if (word.Length > 0)
//            {
//                displayName += char.ToUpper(word[0]) + word.Substring(1) + " ";
//            }
//        }

//        return displayName.Trim();
//    }

//    // Hitta slider för en given färdighet
//    private Slider GetSliderForSkill(string skillName)
//    {
//        switch (skillName)
//        {
//            // Gathering
//            case "logging": return loggingSlider;
//            case "mining": return miningSlider;
//            case "foraging": return foragingSlider;
//            case "digging": return diggingSlider;
//            case "skinning": return skinningSlider;

//            // Crafting
//            case "woodworking": return woodworkingSlider;
//            case "stoneworking": return stoneworkingSlider;
//            case "metalworking": return metalworkingSlider;
//            case "leatherworking": return leatherworkingSlider;
//            case "cooking": return cookingSlider;
//            case "alchemy": return alchemySlider;

//            // Survival
//            case "hunting": return huntingSlider;
//            case "fishing": return fishingSlider;
//            case "fire_making": return fireMakingSlider;
//            case "medicine": return medicineSlider;
//            case "weather_adaptation": return weatherAdaptationSlider;

//            // Movement
//            case "agility": return agilitySlider;
//            case "sneaking": return sneakingSlider;
//            case "swimming": return swimmingSlider;
//            case "climbing": return climbingSlider;

//            // Combat
//            case "melee": return meleeSlider;
//            case "ranged": return rangedSlider;
//            case "defense": return defenseSlider;

//            default: return null;
//        }
//    }

//    // Uppdatera alla sliders baserat på nuvarande värden
//    private void UpdateSkillSliders()
//    {
//        foreach (var category in skillCategories)
//        {
//            foreach (var skill in category.Value)
//            {
//                Slider slider = GetSliderForSkill(skill.Key);
//                if (slider != null)
//                {
//                    slider.value = skill.Value;
//                    slider.maxValue = expToNextLevelDict[skill.Key];
//                }
//            }
//        }
//    }

//    // Uppdatera alla nivåtexter
//    private void UpdateSkillLevelTexts()
//    {
//        // Gathering
//        if (loggingLevelText != null) loggingLevelText.text = "Logging Level " + levels["logging"];
//        if (miningLevelText != null) miningLevelText.text = "Mining Level " + levels["mining"];
//        if (foragingLevelText != null) foragingLevelText.text = "Foraging Level " + levels["foraging"];
//        if (diggingLevelText != null) diggingLevelText.text = "Digging level " + levels["digging"];
//        if (skinningLevelText != null) skinningLevelText.text = "Skinning Level " + levels["skinning"];

//        // Crafting
//        if (woodworkingLevelText != null) woodworkingLevelText.text = "Trähantverk: " + levels["woodworking"];
//        if (stoneworkingLevelText != null) stoneworkingLevelText.text = "Stenhantverk: " + levels["stoneworking"];
//        if (metalworkingLevelText != null) metalworkingLevelText.text = "Metallarbete: " + levels["metalworking"];
//        if (leatherworkingLevelText != null) leatherworkingLevelText.text = "Läderarbete: " + levels["leatherworking"];
//        if (cookingLevelText != null) cookingLevelText.text = "Matlagning: " + levels["cooking"];
//        if (alchemyLevelText != null) alchemyLevelText.text = "Alkemi: " + levels["alchemy"];

//        // Survival
//        if (huntingLevelText != null) huntingLevelText.text = "Jakt: " + levels["hunting"];
//        if (fishingLevelText != null) fishingLevelText.text = "Fiske: " + levels["fishing"];
//        if (fireMakingLevelText != null) fireMakingLevelText.text = "Eldhantering: " + levels["fire_making"];
//        if (medicineLevelText != null) medicineLevelText.text = "Medicin: " + levels["medicine"];
//        if (weatherAdaptationLevelText != null) weatherAdaptationLevelText.text = "Klimatanpassning: " + levels["weather_adaptation"];

//        // Movement
//        if (agilityLevelText != null) agilityLevelText.text = "Smidighet: " + levels["agility"];
//        if (sneakingLevelText != null) sneakingLevelText.text = "Smygande: " + levels["sneaking"];
//        if (swimmingLevelText != null) swimmingLevelText.text = "Simning: " + levels["swimming"];
//        if (climbingLevelText != null) climbingLevelText.text = "Klättring: " + levels["climbing"];

//        // Combat
//        if (meleeLevelText != null) meleeLevelText.text = "Närstrid: " + levels["melee"];
//        if (rangedLevelText != null) rangedLevelText.text = "Distansstrid: " + levels["ranged"];
//        if (defenseLevelText != null) defenseLevelText.text = "Försvar: " + levels["defense"];
//    }

//    // Konvertera från gamla till nya färdigheter (för kompatibilitet med tidigare kod)
//    public void GainOldSkillExp(string oldSkillName, float expGain)
//    {
//        if (oldToNewSkillMapping.TryGetValue(oldSkillName, out string newSkillName))
//        {
//            GainSkillExp(newSkillName, expGain);
//        }
//        else
//        {
//            Debug.LogWarning("Inget nytt färdighetsnamn hittades för: " + oldSkillName);
//        }
//    }

//    // Visa meddelande till spelaren
//    public void ShowMessage(string message)
//    {
//        messageText.text = message;
//        StartCoroutine(ClearMessageAfterDelay(3.0f)); // Meddelandet försvinner efter 3 sekunder
//    }

//    private IEnumerator ClearMessageAfterDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        messageText.text = "";
//    }

//    // Publik metod för att hämta en färdighets nivå
//    public int GetSkillLevel(string skillName)
//    {
//        if (levels.ContainsKey(skillName))
//        {
//            return levels[skillName];
//        }

//        // Försök konvertera från gammalt till nytt namn
//        if (oldToNewSkillMapping.TryGetValue(skillName, out string newSkillName))
//        {
//            if (levels.ContainsKey(newSkillName))
//            {
//                return levels[newSkillName];
//            }
//        }

//        Debug.LogWarning("Försöker hämta nivå för okänd färdighet: " + skillName);
//        return 0;
//    }

//    // Publik metod för att hämta en färdighets multiplikator
//    public float GetMaterialMultiplier(string skillName)
//    {
//        if (materialAmountMultiplier.ContainsKey(skillName))
//        {
//            return materialAmountMultiplier[skillName];
//        }

//        // Försök konvertera från gammalt till nytt namn
//        if (oldToNewSkillMapping.TryGetValue(skillName, out string newSkillName))
//        {
//            if (materialAmountMultiplier.ContainsKey(newSkillName))
//            {
//                return materialAmountMultiplier[newSkillName];
//            }
//        }

//        Debug.LogWarning("Försöker hämta multiplikator för okänd färdighet: " + skillName);
//        return 1.0f;
//    }
//}

////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.UI;
////using TMPro;
////using System.Collections;

////public class PlayerSkills : MonoBehaviour
////{
////    // Maxvärden för färdigheter
////    public float maxSkillValue = 300f;

////    // Erfarenhet till nästa nivå för varje färdighet
////    public float expToNextLevel = 100f;

////    // Färdigheter och deras erfarenhetspoäng
////    private Dictionary<string, float> materialAmountMultiplier = new Dictionary<string, float>
////{
////    { "wood", 1f }, { "stone", 1f }, { "iron", 1f }, { "leather", 1f }, { "soil", 1f },
////    { "sand", 1f }, { "bone", 1f }, { "glass", 1f }, { "claying", 1f }, { "cooking", 1f },
////    { "fishing", 1f }, { "hunting", 1f }, { "running", 1f }, { "jumping", 1f }, { "sneaking", 1f },
////    { "swimming", 1f }, { "armed", 1f }, { "fist", 1f }, { "range", 1f }, { "sheild", 1f },
////    { "avoid", 1f }, { "block", 1f }
////    };

////    private Dictionary<string, float> skills = new Dictionary<string, float>
////    {
////       { "wood", 0f }, { "stone", 0f }, { "iron", 0f }, { "leather", 0f }, { "soil", 0f },
////       { "sand", 0f }, { "bone", 0f }, { "glass", 0f }, { "claying", 0f }, { "cooking", 0f },
////       { "fishing", 0f }, { "hunting", 0f }, { "running", 0f }, { "jumping", 0f }, { "sneaking", 0f },
////       { "swimming", 0f }, { "armed", 0f }, { "fist", 0f }, { "range", 0f }, { "sheild", 0f },
////       { "avoid", 0f }, { "block", 0f }
////    };

////    private Dictionary<string, float> collectionRates = new Dictionary<string, float>
////    {
////        { "wood", 1f }, { "stone", 1f }, { "iron", 1f }, { "leather", 1f }, { "soil", 1f },
////        { "sand", 1f }, { "bone", 1f }, { "glass", 1f }, { "claying", 1f }, { "cooking", 1f },
////        { "fishing", 1f }, { "hunting", 1f }, { "running", 1f }, { "jumping", 1f }, { "sneaking", 1f },
////        { "swimming", 1f }, { "armed", 1f }, { "fist", 1f }, { "range", 1f }, { "sheild", 1f },
////        { "avoid", 1f }, { "block", 1f }
////    };

////    private Dictionary<string, int> levels = new Dictionary<string, int>
////    {
////        { "wood", 1 }, { "stone", 1 }, { "iron", 1 }, { "leather", 1 }, { "soil", 1 },
////        { "sand", 1 }, { "bone", 1 }, { "glass", 1 }, { "claying", 1 }, { "cooking", 1 },
////        { "fishing", 1 }, { "hunting", 1 }, { "running", 1 }, { "jumping", 1 }, { "sneaking", 1 },
////        { "swimming", 1 }, { "armed", 1 }, { "fist", 1 }, { "range", 1 }, { "sheild", 1 },
////        { "avoid", 1 }, { "block", 1 }
////    };

////    private Dictionary<string, float> expToNextLevelDict = new Dictionary<string, float>
////    {
////        { "wood", 100f }, { "stone", 100f }, { "iron", 100f }, { "leather", 100f }, { "soil", 100f },
////        { "sand", 100f }, { "bone", 100f }, { "glass", 100f }, { "claying", 100f }, { "cooking", 100f },
////        { "fishing", 100f }, { "hunting", 100f }, { "running", 100f }, { "jumping", 100f }, { "sneaking", 100f },
////        { "swimming", 100f }, { "armed", 100f }, { "fist", 100f }, { "range", 100f }, { "sheild", 100f },
////        { "avoid", 100f }, { "block", 100f }
////    };

////    [Header("UI Elements")]
////    public Slider woodSlider;
////    public Slider stoneSlider;
////    public Slider ironSlider;
////    public Slider leatherSlider;
////    public Slider soilSlider;
////    public Slider sandSlider;
////    public Slider boneSlider;
////    public Slider glassSlider;
////    public Slider clayingSlider;
////    public Slider cookingSlider;
////    public Slider fishingSlider;
////    public Slider huntingSlider;
////    public Slider runningSlider;
////    public Slider jumpingSlider;
////    public Slider sneakingSlider;
////    public Slider swimmingSlider;
////    public Slider armedSlider;
////    public Slider fistSlider;
////    public Slider rangeSlider;
////    public Slider sheildSlider;
////    public Slider avoidSlider;
////    public Slider blockSlider;

////    // Nivå textkomponenter
////    public TextMeshProUGUI woodLevelText;
////    public TextMeshProUGUI stoneLevelText;
////    public TextMeshProUGUI ironLevelText;
////    public TextMeshProUGUI leatherLevelText;
////    public TextMeshProUGUI soilLevelText;
////    public TextMeshProUGUI sandLevelText;
////    public TextMeshProUGUI boneLevelText;
////    public TextMeshProUGUI glassLevelText;
////    public TextMeshProUGUI clayingLevelText;
////    public TextMeshProUGUI cookingLevelText;
////    public TextMeshProUGUI fishingLevelText;
////    public TextMeshProUGUI huntingLevelText;
////    public TextMeshProUGUI runningLevelText;
////    public TextMeshProUGUI jumpingLevelText;
////    public TextMeshProUGUI sneakingLevelText;
////    public TextMeshProUGUI swimmingLevelText;
////    public TextMeshProUGUI armedLevelText;
////    public TextMeshProUGUI fistLevelText;
////    public TextMeshProUGUI rangeLevelText;
////    public TextMeshProUGUI sheildLevelText;
////    public TextMeshProUGUI avoidLevelText;
////    public TextMeshProUGUI blockLevelText;

////    public TextMeshProUGUI messageText; // Text-komponent för meddelanden

////    void Start()
////    {
////        //Debug.Log(woodSlider == null ? "woodSlider is null" : "woodSlider is set"); 
////        //Debug.Log(woodLevelText == null ? "woodLevelText is null" : "woodLevelText is set");
////        //Debug.Log("Start method called");
////        UpdateSkillSliders();
////        UpdateSkillLevelTexts();
////        messageText.text = ""; // Initiera meddelandetext till tomt
////    }
////    void Update()
////    {
////        if (Input.GetKeyDown(KeyCode.L))
////        {
////            UseItem("axe"); // Exempel: Använd en yxa
////        }
////        if (Input.GetKeyDown(KeyCode.P))
////        {
////            UseItem("shovel"); // Exempel: Använd en yxa
////        }
////    }
////    private void UseItem(string itemName)
////    {
////        //Debug.Log("Using item: " + itemName);
////        switch (itemName)
////        {
////            case "axe":
////                GainSkillExp("wood", 10f * materialAmountMultiplier["wood"], woodSlider);
////                break;
////            case "shovel":
////                GainSkillExp("soil", 10f * materialAmountMultiplier["soil"], soilSlider);
////                break;
////                // Lägg till fler föremål och koppla till rätt färdigheter
////        }

////        UpdateSkillSliders();
////        UpdateSkillLevelTexts();
////    }
////    private void GainSkillExp(string skillName, float expGain, Slider skillSlider)
////    {
////        if (!skills.ContainsKey(skillName))
////        {
////            Debug.LogError("Skill not found: " + skillName);
////            return;
////        }
////            // Multiplicera erfarenhetspoängen med nivån för att ge bonus
////            skills[skillName] += expGain * levels[skillName];
////            skillSlider.value = skills[skillName];

////        if (skills[skillName] >= expToNextLevelDict[skillName])
////        {
////            levels[skillName]++;
////            skills[skillName] = 0;
////            expToNextLevelDict[skillName] *= 1.5f;
////            skillSlider.maxValue = expToNextLevelDict[skillName];
////            skillSlider.value = skills[skillName];

////            // Uppdatera samlingshastigheten och materialmängdsfaktorn
////            collectionRates[skillName] *= 1.2f;
////            materialAmountMultiplier[skillName] *= 1.1f;

////            ShowMessage($"You have reached level {levels[skillName]} in skill {skillName}!");
////            UpdateSkillLevelTexts();
////        }
////    }

////    private void UpdateSkillSliders()
////    {
////        //Debug.Log(woodSlider == null ? "woodSlider is null" : "woodSlider is set"); 
////        //Debug.Log(stoneSlider == null ? "stoneSlider is null" : "stoneSlider is set"); 
////        //Debug.Log(ironSlider == null ? "ironSlider is null" : "ironSlider is set"); 
////        //Debug.Log(leatherSlider == null ? "leatherSlider is null" : "leatherSlider is set");
////        //Debug.Log(soilSlider == null ? "soilSlider is null" : "soilSlider is set");
////        //Debug.Log(sandSlider == null ? "sandSlider is null" : "sandSlider is set");
////        //Debug.Log(boneSlider == null ? "boneSlider is null" : "boneSlider is set"); 
////        //Debug.Log(glassSlider == null ? "glassSlider is null" : "glassSlider is set");
////        //Debug.Log(clayingSlider == null ? "clayingSlider is null" : "clayingSlider is set"); 
////        //Debug.Log(cookingSlider == null ? "cookingSlider is null" : "cookingSlider is set"); 
////        //Debug.Log(fishingSlider == null ? "fishingSlider is null" : "fishingSlider is set");
////        //Debug.Log(huntingSlider == null ? "huntingSlider is null" : "huntingSlider is set");
////        //Debug.Log(runningSlider == null ? "runningSlider is null" : "runningSlider is set"); 
////        //Debug.Log(jumpingSlider == null ? "jumpingSlider is null" : "jumpingSlider is set"); 
////        //Debug.Log(sneakingSlider == null ? "sneakingSlider is null" : "sneakingSlider is set");
////        //Debug.Log(swimmingSlider == null ? "swimmingSlider is null" : "swimmingSlider is set");
////        //Debug.Log(armedSlider == null ? "armedSlider is null" : "armedSlider is set");
////        //Debug.Log(fistSlider == null ? "fistSlider is null" : "fistSlider is set"); 
////        //Debug.Log(rangeSlider == null ? "rangeSlider is null" : "rangeSlider is set");
////        //Debug.Log(sheildSlider == null ? "sheildSlider is null" : "sheildSlider is set");
////        //Debug.Log(avoidSlider == null ? "avoidSlider is null" : "avoidSlider is set");
////        //Debug.Log(blockSlider == null ? "blockSlider is null" : "blockSlider is set");

////        woodSlider.value = skills["wood"];
////        stoneSlider.value = skills["stone"];
////        ironSlider.value = skills["iron"];
////        leatherSlider.value = skills["leather"];
////        soilSlider.value = skills["soil"];
////        sandSlider.value = skills["sand"];
////        boneSlider.value = skills["bone"];
////        glassSlider.value = skills["glass"];
////        clayingSlider.value = skills["claying"];
////        cookingSlider.value = skills["cooking"];
////        fishingSlider.value = skills["fishing"];
////        huntingSlider.value = skills["hunting"];
////        runningSlider.value = skills["running"];
////        jumpingSlider.value = skills["jumping"];
////        sneakingSlider.value = skills["sneaking"];
////        swimmingSlider.value = skills["swimming"];
////        armedSlider.value = skills["armed"];
////        fistSlider.value = skills["fist"];
////        rangeSlider.value = skills["range"];
////        sheildSlider.value = skills["sheild"];
////        avoidSlider.value = skills["avoid"];
////        blockSlider.value = skills["block"];
////    }
////    private void UpdateSkillLevelTexts()
////    {
////        woodLevelText.text = "Wood: " + levels["wood"];
////        stoneLevelText.text = "Stone: " + levels["stone"];
////        ironLevelText.text = "Iron: " + levels["iron"];
////        leatherLevelText.text = "Leather: " + levels["leather"];
////        soilLevelText.text = "Soil: " + levels["soil"];
////        sandLevelText.text = "Sand: " + levels["sand"];
////        boneLevelText.text = "Bone: " + levels["bone"];
////        glassLevelText.text = "Glass: " + levels["glass"];
////        clayingLevelText.text = "Claying: " + levels["claying"];
////        cookingLevelText.text = "Cooking: " + levels["cooking"];
////        fishingLevelText.text = "Fishing: " + levels["fishing"];
////        huntingLevelText.text = "Hunting: " + levels["hunting"];
////        runningLevelText.text = "Running: " + levels["running"];
////        jumpingLevelText.text = "Jumping: " + levels["jumping"];
////        sneakingLevelText.text = "Sneaking: " + levels["sneaking"];
////        swimmingLevelText.text = "Swimming: " + levels["swimming"];
////        armedLevelText.text = "Armed: " + levels["armed"];
////        fistLevelText.text = "Fist: " + levels["fist"];
////        rangeLevelText.text = "Range: " + levels["range"];
////        sheildLevelText.text = "Sheild: " + levels["sheild"];
////        avoidLevelText.text = "Avoid: " + levels["avoid"];
////        blockLevelText.text = "Block: " + levels["block"];
////    }
////       public void ShowMessage(string message)
////    {
////        messageText.text = message;
////        StartCoroutine(ClearMessageAfterDelay(20.0f)); // Meddelandet försvinner efter 2 sekunder
////    }

////    private IEnumerator ClearMessageAfterDelay(float delay)
////    {
////        yield return new WaitForSeconds(delay);
////        messageText.text = "";
////    }
////}





//////using System.Collections;
//////using System.Collections.Generic;
//////using UnityEngine;
//////using UnityEngine.UI;
//////using TMPro;
//////using System.Reflection;
//////using UnityEngine.XR;
//////using System;

//////public class PlayerSkills : MonoBehaviour
//////{
//////    Maxvärden
//////    public float maxSkillWood = 300f, maxSkillStone = 300f, maxSkillIron = 300f, maxSkillLeather = 300f, maxSkillSoil = 300f, maxSkillSand = 300f, maxSkillBone = 300f, maxSkillGlass = 300f,
//////                 maxSkillClaying = 300f, maxSkillCooking = 300f, maxSkillFishing = 300f, maxSkillHunting = 300f, maxSkillRunning = 300f, maxSkillJumping = 300f, maxSkillSneaking = 300f, maxSkillSwimming = 300f,
//////                 maxSkillArmed = 300f, maxSkillFist = 300f, maxSkillRange = 300f, maxSkillSheild = 300f, maxSkillAvoid = 300f, maxSkillBlock = 300f;

//////    [Header("SKILLS")]
//////    [Header("Materials")]
//////    [SerializeField] private float wood = 0;
//////    [SerializeField] private float stone = 0;
//////    [SerializeField] private float iron = 0;
//////    [SerializeField] private float leather = 0;
//////    [SerializeField] private float soil = 0;
//////    [SerializeField] private float sand = 0;
//////    [SerializeField] private float bone = 0;
//////    [SerializeField] private float glass = 0;

//////    [Header("Crafting")]
//////    [SerializeField] private float claying = 0;
//////    [SerializeField] private float cooking = 0;
//////    [SerializeField] private float fishing = 0;
//////    [SerializeField] private float hunting = 0;

//////    [Header("Movement")]
//////    [SerializeField] private float running = 0;
//////    [SerializeField] private float jumping = 0;
//////    [SerializeField] private float sneaking = 0;
//////    [SerializeField] private float swimming = 0;

//////    [Header("Attack")]
//////    [SerializeField] private float armed = 0;
//////    [SerializeField] private float fist = 0;
//////    [SerializeField] private float range = 0;

//////    [Header("Defence")]
//////    [SerializeField] private float sheild = 0;
//////    [SerializeField] private float avoid = 0;
//////    [SerializeField] private float block = 0;

//////    Nivåer och erfarenhetspoäng för färdigheter
//////    private int levelWood = 1,
//////    levelStone = 1, levelIron = 1,
//////    levelLeather = 1, levelSoil = 1,
//////    levelSand = 1, levelBone = 1,
//////    levelGlass = 1, levelClaying = 1,
//////    levelCooking = 1, levelFishing = 1,
//////    levelHunting = 1, levelRunning = 1,
//////    levelJumping = 1, levelSneaking = 1,
//////    levelSwimming = 1, levelArmed = 1,
//////    levelFist = 1, levelRange = 1,
//////    levelSheild = 1, levelAvoid = 1,
//////    levelBlock = 1;
//////    private float expWood = 0,
//////    expStone = 0, expIron = 0,
//////    expLeather = 0, expSoil = 0,
//////    expSand = 0, expBone = 0,
//////    expGlass = 0, expClaying = 0,
//////    expCooking = 0, expFishing = 0,
//////    expHunting = 0, expRunning = 0,
//////    expJumping = 0, expSneaking = 0,
//////    expSwimming = 0, expArmed = 0,
//////    expFist = 0, expRange = 0, expSheild = 0,
//////    expAvoid = 0, expBlock = 0;

//////    public float expToNextLevel = 100;

//////    [Header("UI")]
//////    public GameObject inventoryPanel;
//////    public Slider woodSlider;
//////    public Slider stoneSlider;
//////    public Slider ironSlider;
//////    public Slider leatherSlider;
//////    public Slider soilSlider;
//////    public Slider sandSlider;
//////    public Slider boneSlider;
//////    public Slider glassSlider;
//////    public Slider clayingSlider;
//////    public Slider cookingSlider;
//////    public Slider fishingSlider;
//////    public Slider huntingSlider;
//////    public Slider runningSlider;
//////    public Slider jumpingSlider;
//////    public Slider sneakingSlider;
//////    public Slider swimmingSlider;
//////    public Slider armedSlider;
//////    public Slider fistSlider;
//////    public Slider rangeSlider;
//////    public Slider sheildSlider;
//////    public Slider avoidSlider;
//////    public Slider blockSlider;

//////    public TextMeshProUGUI woodText;
//////    public TextMeshProUGUI stoneText;
//////    public TextMeshProUGUI ironText;
//////    public TextMeshProUGUI leatherText;
//////    public TextMeshProUGUI soilText;
//////    public TextMeshProUGUI sandText;
//////    public TextMeshProUGUI boneText;
//////    public TextMeshProUGUI glassText;
//////    public TextMeshProUGUI clayingText;
//////    public TextMeshProUGUI cookingText;
//////    public TextMeshProUGUI fishingText;
//////    public TextMeshProUGUI huntingText;
//////    public TextMeshProUGUI runningText;
//////    public TextMeshProUGUI jumpingText;
//////    public TextMeshProUGUI sneakingText;
//////    public TextMeshProUGUI swimmingText;
//////    public TextMeshProUGUI armedText;
//////    public TextMeshProUGUI fistText;
//////    public TextMeshProUGUI rangeText;
//////    public TextMeshProUGUI sheildText;
//////    public TextMeshProUGUI avoidText;
//////    public TextMeshProUGUI blockText;

//////    public TextMeshProUGUI messageText; // Text-komponent för meddelanden


//////    private bool isInventoryVisible = false;

//////    void Start()
//////    {
//////        InitializeSkills();
//////        UpdateSkillSliders();
//////        inventoryPanel.SetActive(false);
//////        messageText.text = ""; // Initiera meddelandetext till tomt
//////    }


//////    void Update()
//////    {
//////        Debug.Log("Update method is running");
//////        Debug.Log("Key L pressed");

//////        if (Input.GetKeyDown(KeyCode.Tab))
//////        {
//////            isInventoryVisible = !isInventoryVisible;
//////            inventoryPanel.SetActive(isInventoryVisible);
//////        }
//////        if (Input.GetKeyDown(KeyCode.L))
//////        {
//////            Debug.Log("Key l pressed at gainskill");
//////            GainSkillExpWood();
//////            GainSkillExp("Wood", ref expWood, ref levelWood, ref wood, maxSkillWood, woodSlider);
//////            GainSkillExp("Stone", ref expStone, ref levelStone, ref stone, maxSkillStone, stoneSlider);
//////            GainSkillExp("Iron", ref expIron, ref levelIron, ref iron, maxSkillIron, ironSlider);
//////            GainSkillExp("Leather", ref expLeather, ref levelLeather, ref leather, maxSkillLeather, leatherSlider);
//////            GainSkillExp("Soil", ref expSoil, ref levelSoil, ref soil, maxSkillSoil, soilSlider);
//////            GainSkillExp("Sand", ref expSand, ref levelSand, ref sand, maxSkillSand, sandSlider);
//////            GainSkillExp("Bone", ref expBone, ref levelBone, ref bone, maxSkillBone, boneSlider);
//////            GainSkillExp("Glass", ref expGlass, ref levelGlass, ref glass, maxSkillGlass, glassSlider);
//////            GainSkillExp("Claying", ref expClaying, ref levelClaying, ref claying, maxSkillClaying, clayingSlider);
//////            GainSkillExp("Cooking", ref expCooking, ref levelCooking, ref cooking, maxSkillCooking, cookingSlider);
//////            GainSkillExp("Fishing", ref expFishing, ref levelFishing, ref fishing, maxSkillFishing, fishingSlider);
//////            GainSkillExp("Hunting", ref expHunting, ref levelHunting, ref hunting, maxSkillHunting, huntingSlider);
//////            GainSkillExp("Running", ref expRunning, ref levelRunning, ref running, maxSkillRunning, runningSlider);
//////            GainSkillExp("Jumping", ref expJumping, ref levelJumping, ref jumping, maxSkillJumping, jumpingSlider);
//////            GainSkillExp("Sneaking", ref expSneaking, ref levelSneaking, ref sneaking, maxSkillSneaking, sneakingSlider);
//////            GainSkillExp("Swimming", ref expSwimming, ref levelSwimming, ref swimming, maxSkillSwimming, swimmingSlider);
//////            GainSkillExp("Armed", ref expArmed, ref levelArmed, ref armed, maxSkillArmed, armedSlider);
//////            GainSkillExp("Fist", ref expFist, ref levelFist, ref fist, maxSkillFist, fistSlider);
//////            GainSkillExp("Range", ref expRange, ref levelRange, ref range, maxSkillRange, rangeSlider);
//////            GainSkillExp("Sheild", ref expSheild, ref levelSheild, ref sheild, maxSkillSheild, sheildSlider);
//////            GainSkillExp("Avoid", ref expAvoid, ref levelAvoid, ref avoid, maxSkillAvoid, avoidSlider);
//////            GainSkillExp("Block", ref expBlock, ref levelBlock, ref block, maxSkillBlock, blockSlider);
//////            Debug.Log("GainEXP uppdateras");


//////            UpdateSkillSliders(); // Update sliders after increasing skill

//////            ShowMessage(""); // Visa meddelande när en skicklighet ökar ShowMessage
//////        }


//////        Lägg till exempelvis kod för att använda yxa för träning av wood-skill
//////        if (Input.GetKey(KeyCode.L)) // Sätt rätt tangent för yxa
//////        {
//////            GainSkillExp(ref expWood, ref levelWood, ref wood, maxSkillWood);
//////        }
//////        //Lägg till liknande kod för andra färdigheter beroende på vilka verktyg som används... UpdateSkillSliders(); // Uppdatera sliders efter att färdighet har använts

//////        UpdateSkillSliders(); // Uppdatera sliders efter att färdighet har använts
//////    }

//////    private void GainSkillExp(ref float expWood, ref int levelWood, ref float wood, float maxSkillWood)
//////    {
//////        GainSkillExp("Wood", ref expWood, ref levelWood, ref wood, maxSkillWood);
//////        //Debug.Log("L trycks på Gainskill");
//////        //throw new NotImplementedException();
//////    }

//////    private void GainSkillExpWood()
//////    {
//////        GainSkillExp("Wood", ref expWood, ref levelWood, ref wood, maxSkillWood, woodSlider);
//////    }

//////    private void InitializeSkills()
//////    {
//////        wood = 0;
//////        stone = 0;
//////        iron = 0;
//////        leather = 0;
//////        soil = 0;
//////        sand = 0;
//////        bone = 0;
//////        glass = 0;
//////        claying = 0;
//////        cooking = 0;
//////        fishing = 0;
//////        hunting = 0;
//////        running = 0;
//////        jumping = 0;
//////        sneaking = 0;
//////        swimming = 0;
//////        armed = 0;
//////        fist = 0;
//////        range = 0;
//////        sheild = 0;
//////        avoid = 0;
//////        block = 0;
//////    }
//////    private void GainSkillExp(string v, ref float exp, ref int level, ref float skill, float maxSkill, Slider skillSlider)
//////    {
//////        exp += 10f; // Öka erfarenhetspoäng
//////        Debug.Log($"{v} exp increased to {exp}"); // Kontrollera att exp ökar
//////        skillSlider.value = exp; // Uppdatera slider
//////        Debug.Log($"{v} slider value updated to {skillSlider.value}"); // Kontrollera att slidern uppdateras

//////        if (exp >= expToNextLevel)
//////        {
//////            level++; // Öka nivå
//////            exp = 0; // Återställ erfarenhetspoäng
//////            skill = Mathf.Min(skill + 50, maxSkill); // Öka färdighetspoäng, begränsa till maxvärdet
//////            expToNextLevel *= 2f; // Öka gränsen för nästa nivå
//////            skillSlider.maxValue = expToNextLevel; // Uppdatera slider maxvärde
//////            skillSlider.value = exp; // Återställ slider
//////            ShowMessage($"You have reached level {level} in skill {v} with {skill} points!");
//////            Debug.Log($"{v} leveled up to {level}"); // Kontrollera att nivå ökar }
//////        }


//////    private float expToNextLevel = 100f; // Startvärde

//////    private void GainSkillExp(string v, ref float exp, ref int level, ref float skill, float maxSkill)
//////    {
//////        exp += 1f; // Öka erfarenhetspoäng
//////        if (exp >= expToNextLevel)
//////        {
//////            level++; // Öka nivå
//////            exp = 0; // Återställ erfarenhetspoäng
//////            skill = Mathf.Min(skill + 100, maxSkill); // Öka färdighetspoäng, begränsa till maxvärdet
//////            ShowMessage($"You have reached level {level} in skill {v} with {skill} points!");
//////            //ShowMessage($"You have reached level {level} in skill {v} with {skill} points!")
//////            expToNextLevel *= 2f; // Öka erfarenhetspoängsgränsen för nästa nivå
//////        }
//////    }
//////    private void HandleSkillIncrease(ref float skill, float maxSkill)
//////    {
//////        if (skill < maxSkill)
//////        {
//////            skill += 1.0f;
//////            Debug.Log("Skill increased: " + skill);
//////            Kontrollera att skickligheten ökar
//////        }
//////        else
//////        {
//////            //Debug.Log("Skill is already at maximum: " + skill);
//////        }
//////    }
//////    private void UpdateSkillSliders()
//////    {
//////        woodSlider.value = wood / maxSkillWood;
//////        stoneSlider.value = stone / maxSkillStone;
//////        ironSlider.value = iron / maxSkillIron;
//////        leatherSlider.value = leather / maxSkillLeather;
//////        soilSlider.value = soil / maxSkillSoil;
//////        sandSlider.value = sand / maxSkillSand;
//////        boneSlider.value = bone / maxSkillBone;
//////        glassSlider.value = glass / maxSkillGlass;
//////        clayingSlider.value = claying / maxSkillClaying;
//////        cookingSlider.value = cooking / maxSkillCooking;
//////        fishingSlider.value = fishing / maxSkillFishing;
//////        huntingSlider.value = hunting / maxSkillHunting;
//////        runningSlider.value = running / maxSkillRunning;
//////        jumpingSlider.value = jumping / maxSkillJumping;
//////        sneakingSlider.value = sneaking / maxSkillSneaking;
//////        swimmingSlider.value = swimming / maxSkillSwimming;
//////        armedSlider.value = armed / maxSkillArmed;
//////        fistSlider.value = fist / maxSkillFist;
//////        rangeSlider.value = range / maxSkillRange;
//////        sheildSlider.value = sheild / maxSkillSheild;
//////        avoidSlider.value = avoid / maxSkillAvoid;
//////        blockSlider.value = block / maxSkillBlock;

//////        Uppdatera texter
//////        woodText.text = "Wood: " + wood + " / " + maxSkillWood;
//////        stoneText.text = "Stone: " + stone + " / " + maxSkillStone;
//////        ironText.text = "Iron: " + iron + " / " + maxSkillIron;
//////        leatherText.text = "Leather: " + leather + " / ";
//////    }
//////    public void ShowMessage(string message)
//////    {
//////        messageText.text = message;
//////        StartCoroutine(ClearMessageAfterDelay(2.0f)); // Meddelandet försvinner efter 2 sekunder
//////    }
//////    private IEnumerator ClearMessageAfterDelay(float delay)
//////    {
//////        yield return new WaitForSeconds(delay);
//////        messageText.text = "";
//////    }
//////}