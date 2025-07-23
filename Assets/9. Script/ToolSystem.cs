using UnityEngine;
using System.Collections;
using InventoryAndCrafting;

public enum ToolType
{
    None,
    Axe,
    Bottle,
    Drill,
    FishingRod,
    Hammer,
    Knife,
    Hoe,
    Pickaxe,
    Shovel,
    Syringe,
    StickPoint,
    Wrench,
}

public class ToolSystem : MonoBehaviour
{
    [Header("Tool Settings")]
    public ToolType toolType = ToolType.None;
    public float efficiency = 1.0f;
    public float durability = 100f;
    public float maxDurability = 100f;
    public float useAmount = 1.0f;  // Hur mycket durability f�rbrukas per anv�ndning

    [Header("Position & Rotation Settings")]
    public Vector3 toolPositionOffset = Vector3.zero;
    public Vector3 toolRotationOffset = Vector3.zero;

    [Header("Grip Settings")]
    public bool usesTwoHands = false;
    public Vector3 leftHandPosition = new Vector3(0, 0, -0.3f);

    [Header("Animation Settings")]
    public string animationTrigger = ""; // Detta kommer att s�ttas baserat p� verktygstypen
    public string animationBoolParam = ""; // F�r animationer som anv�nder boolean ist�llet f�r trigger

    [Header("Resource Collection")]
    public float harvestAmount = 1.0f;
    public string[] compatibleTags;  // Taggar f�r resurser som detta verktyg kan samla

    [Header("Debug Settings")]
    public bool debugMode = true;

    [Header("References")]
    private Transform playerTransform;
    private Animator playerAnimator;
    private PlayerSkills playerSkills;
    private IKControlLeft leftHandIK;
    private PlayerAnimationController animationController;

    private bool isEquipped = false;
    private bool isAnimationPlaying = false;
    private bool isToolUsable = true;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        // St�ll in animation trigger baserat p� verktygstyp
        SetAnimationTriggerForToolType();
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;

        // Hitta komponenter p� spelaren
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator == null)
                playerAnimator = player.GetComponentInChildren<Animator>();

            playerSkills = player.GetComponent<PlayerSkills>();

            leftHandIK = player.GetComponent<IKControlLeft>();
            if (leftHandIK == null)
                leftHandIK = player.GetComponentInChildren<IKControlLeft>();

            animationController = player.GetComponent<PlayerAnimationController>();
            if (animationController == null)
                animationController = player.GetComponentInChildren<PlayerAnimationController>();
        }

        // St�ll in verktyget baserat p� dess typ
        ConfigureDefaultSettings();

        isEquipped = true;

        // Konfigurera tv�handsgrepp om det beh�vs
        if (usesTwoHands)
        {
            ConfigureTwoHandedGrip();
        }

        // Validera animationsparametrar
        ValidateAnimationParameters();

        if (debugMode)
            Debug.Log($"Verktyg initialiserat: {toolType}, Animation: {animationTrigger}, Bool Param: {animationBoolParam}");
    }

    private void ValidateAnimationParameters()
    {
        if (playerAnimator == null) return;

        bool foundTrigger = false;
        bool foundBool = false;

        // Lista tillg�ngliga parametrar f�r debugging
        string availableParams = "Tillg�ngliga parametrar: ";
        foreach (AnimatorControllerParameter param in playerAnimator.parameters)
        {
            availableParams += param.name + " (" + param.type + "), ";

            // Kontrollera om n�gon av v�ra parametrar finns
            if (param.name == animationTrigger)
                foundTrigger = true;
            if (param.name == animationBoolParam)
                foundBool = true;

            // Kontrollera om vi kan hitta alternativa triggers f�r yxan
            if (toolType == ToolType.Axe)
            {
                if (param.name == "Chopping" || param.name == "isChopping" || param.name == "ChopTree")
                {
                    animationTrigger = param.name;
                    foundTrigger = true;
                    Debug.Log($"Hittade alternativ yxanimation: {param.name}");
                }
            }
        }

        if (debugMode)
        {
            Debug.Log(availableParams);

            if (!foundTrigger && !string.IsNullOrEmpty(animationTrigger))
                Debug.LogWarning($"Trigger '{animationTrigger}' finns inte i Animator!");

            if (!foundBool && !string.IsNullOrEmpty(animationBoolParam))
                Debug.LogWarning($"Boolean '{animationBoolParam}' finns inte i Animator!");
        }
    }

    private void Update()
    {
        // Uppdatera verktygets position och rotation om det �r utrustat
        if (isEquipped)
        {
            UpdateTransform();

            // Uppdatera cooldown-timer
            if (!isToolUsable)
            {
                if (cooldownTimer > 0)
                {
                    cooldownTimer -= Time.deltaTime;
                }
                else
                {
                    isToolUsable = true;
                }
            }
        }
    }

    private void UpdateTransform()
    {
        // S�tt lokal position och rotation
        transform.localPosition = toolPositionOffset;
        transform.localRotation = Quaternion.Euler(toolRotationOffset);
    }

    private void ConfigureTwoHandedGrip()
    {
        if (!usesTwoHands || leftHandIK == null) return;

        // Skapa ett m�l f�r v�nsterhanden
        GameObject leftHandTarget = new GameObject("LeftHandTarget");
        leftHandTarget.transform.SetParent(transform);
        leftHandTarget.transform.localPosition = leftHandPosition;

        // S�tt upp IK f�r v�nster hand
        leftHandIK.leftHandObj = leftHandTarget.transform;
        leftHandIK.ikActive = true;

        if (debugMode)
            Debug.Log($"Konfigurerade IK f�r tv�handsgrepp p� {toolType}");

        // Skapa en visuell indikering f�r v�nsterhandspositionen i debug-l�ge
#if UNITY_EDITOR
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.SetParent(leftHandTarget.transform);
        debugSphere.transform.localPosition = Vector3.zero;
        debugSphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        debugSphere.GetComponent<Collider>().enabled = false;

        // Ta bort denna visuella indikering i spelet
        if (Application.isPlaying)
        {
            Renderer renderer = debugSphere.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
#endif
    }

    public void ConfigureDefaultSettings()
    {
        // Vissa verktyg �r som standard tv�handsverktyg
        switch (toolType)
        {
            case ToolType.Axe:
            case ToolType.Pickaxe:
            case ToolType.Shovel:
            case ToolType.Hoe:
            case ToolType.Hammer:
                usesTwoHands = true;
                break;
            case ToolType.FishingRod:
                if (toolRotationOffset == Vector3.zero)
                    toolRotationOffset = new Vector3(0, 90, 0);
                break;
        }

        // Konfigurera v�nsterhandens position f�r tv�handsverktyg
        if (usesTwoHands)
        {
            switch (toolType)
            {
                case ToolType.Axe:
                    leftHandPosition = new Vector3(0, -0.2f, -0.3f);
                    break;
                case ToolType.Pickaxe:
                    leftHandPosition = new Vector3(0, -0.15f, -0.3f);
                    break;
                case ToolType.Shovel:
                case ToolType.Hoe:
                    leftHandPosition = new Vector3(0, -0.3f, -0.2f);
                    break;
            }
        }
    }

    public void SetAnimationTriggerForToolType()
    {
        // S�tt animation trigger baserat p� verktygstypen
        switch (toolType)
        {
            case ToolType.Axe:
                animationTrigger = "Chopping"; // �ndrat fr�n isCutting till Chopping
                animationBoolParam = "ChopTree"; // Alternativ boolean parameter
                break;
            case ToolType.Pickaxe:
                animationTrigger = "Mining";
                animationBoolParam = "isMining";
                break;
            case ToolType.Shovel:
                animationTrigger = "Digging";
                animationBoolParam = "isDigging";
                break;
            case ToolType.Hoe:
                animationTrigger = "Hoeing";
                animationBoolParam = "isHoeing";
                break;
            case ToolType.FishingRod:
                animationTrigger = "Fishing";
                animationBoolParam = "isFishing";
                break;
            case ToolType.Hammer:
                animationTrigger = "Hammering";
                animationBoolParam = "isHammering";
                break;
            case ToolType.Knife:
                animationTrigger = "Cutting";
                animationBoolParam = "isCutting";
                break;
            default:
                animationTrigger = "UseItem";
                animationBoolParam = "UseItem";
                break;
        }
    }

    public bool UseTool()
    {
        if (!isToolUsable)
        {
            if (debugMode)
                Debug.Log($"{toolType} kan inte anv�ndas just nu (nedkylning p�g�r)");
            return false;
        }

        if (durability <= 0)
        {
            Debug.Log($"{toolType} �r trasigt och beh�ver repareras!");
            return false;
        }

        // S�tt verktyget p� cooldown och starta timer
        isToolUsable = false;
        cooldownTimer = 1.0f; // 1 sekund cooldown

        // Spela animation genom hierarki av metoder
        bool animationStarted = PlayToolAnimation();

        // Minska verktygets h�llbarhet
        durability -= useAmount;
        durability = Mathf.Max(0, durability);

        // �ka f�rdighet baserat p� verktygstyp
        IncrementSkill();

        // Kontrollera efter resurser i n�rheten
        bool harvestedResource = DetectAndHarvestResources();

        return harvestedResource || animationStarted;
    }

    private bool PlayToolAnimation()
    {
        // Prioritet 1: Anv�nd PlayerAnimationController
        if (animationController != null)
        {
            // Prioritet 1A: Anv�nd trigger via TriggerAnimation
            if (!string.IsNullOrEmpty(animationTrigger))
            {
                bool success = animationController.TriggerAnimation(animationTrigger);
                if (success)
                {
                    if (debugMode) Debug.Log($"Animation spelad via animationController.TriggerAnimation({animationTrigger})");
                    return true;
                }
            }

            // Prioritet 1B: Anv�nd bool via PlayAnimation
            if (!string.IsNullOrEmpty(animationBoolParam))
            {
                animationController.PlayAnimation(animationBoolParam, 1.5f);
                if (debugMode) Debug.Log($"Animation spelad via animationController.PlayAnimation({animationBoolParam})");
                return true;
            }
        }

        // Prioritet 2: Anv�nd Animator direkt
        if (playerAnimator != null)
        {
            // Prioritet 2A: Kontrollera om trigger finns
            if (!string.IsNullOrEmpty(animationTrigger) && CheckAnimatorHasParameter(animationTrigger))
            {
                playerAnimator.SetTrigger(animationTrigger);
                if (debugMode) Debug.Log($"Animation spelad direkt via animator.SetTrigger({animationTrigger})");
                return true;
            }

            // Prioritet 2B: Kontrollera om bool finns
            if (!string.IsNullOrEmpty(animationBoolParam) && CheckAnimatorHasParameter(animationBoolParam))
            {
                playerAnimator.SetBool(animationBoolParam, true);
                StartCoroutine(ResetBoolAfterDelay(1.5f));
                if (debugMode) Debug.Log($"Animation spelad direkt via animator.SetBool({animationBoolParam}, true)");
                return true;
            }

            // Prioritet 2C: Testa f�r "Attack" trigger
            if (CheckAnimatorHasParameter("Attack"))
            {
                playerAnimator.SetTrigger("Attack");
                if (debugMode) Debug.Log("Animation spelad direkt via animator.SetTrigger(\"Attack\")");
                return true;
            }

            // Prioritet 2D: Testa f�r andra vanliga triggers som kan finnas
            string[] commonTriggers = { "UseItem", "Use", "Action", "Swing", "Tool" };
            foreach (string trigger in commonTriggers)
            {
                if (CheckAnimatorHasParameter(trigger))
                {
                    playerAnimator.SetTrigger(trigger);
                    if (debugMode) Debug.Log($"Animation spelad direkt via animator.SetTrigger(\"{trigger}\")");
                    return true;
                }
            }
        }

        // Om vi kommer hit lyckades ingen animationsmetod
        if (debugMode) Debug.LogWarning("Kunde inte spela n�gon animation f�r verktyget!");
        return false;
    }

    private IEnumerator ResetBoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerAnimator != null && !string.IsNullOrEmpty(animationBoolParam) &&
            CheckAnimatorHasParameter(animationBoolParam))
        {
            playerAnimator.SetBool(animationBoolParam, false);
            if (debugMode) Debug.Log($"�terst�llde animationBoolParam {animationBoolParam} till false");
        }
    }

    private bool CheckAnimatorHasParameter(string paramName)
    {
        if (playerAnimator == null || string.IsNullOrEmpty(paramName)) return false;

        foreach (AnimatorControllerParameter param in playerAnimator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private void IncrementSkill()
    {
        if (playerSkills == null) return;

        // �ka f�rdighet baserat p� verktygstyp
        switch (toolType)
        {
            case ToolType.Axe:
                playerSkills.GainOldSkillExp("wood", 5f);
                break;
            case ToolType.Pickaxe:
                playerSkills.GainOldSkillExp("stone", 5f);
                break;
            case ToolType.Shovel:
                playerSkills.GainOldSkillExp("soil", 5f);
                break;
            case ToolType.Hoe:
                playerSkills.GainOldSkillExp("agriculture", 5f);
                break;
            case ToolType.FishingRod:
                playerSkills.GainOldSkillExp("fishing", 5f);
                break;
        }
    }

    // Konvertera fr�n lokal ToolType till InventoryAndCrafting.ToolType
    public InventoryAndCrafting.ToolType GetInventoryToolType()
    {
        switch (toolType)
        {
            case ToolType.None: return InventoryAndCrafting.ToolType.None;
            case ToolType.Axe: return InventoryAndCrafting.ToolType.Axe;
            case ToolType.Bottle: return InventoryAndCrafting.ToolType.Bottle;
            case ToolType.Drill: return InventoryAndCrafting.ToolType.Drill;
            case ToolType.FishingRod: return InventoryAndCrafting.ToolType.FishingRod;
            case ToolType.Hammer: return InventoryAndCrafting.ToolType.Hammer;
            case ToolType.Knife: return InventoryAndCrafting.ToolType.Knife;
            case ToolType.Hoe: return InventoryAndCrafting.ToolType.Hoe;
            case ToolType.Pickaxe: return InventoryAndCrafting.ToolType.Pickaxe;
            case ToolType.Shovel: return InventoryAndCrafting.ToolType.Shovel;
            case ToolType.Syringe: return InventoryAndCrafting.ToolType.Syringe;
            case ToolType.StickPoint: return InventoryAndCrafting.ToolType.StickPoint;
            case ToolType.Wrench: return InventoryAndCrafting.ToolType.Wrench;
            default: return InventoryAndCrafting.ToolType.None;
        }
    }

    // Konvertera fr�n InventoryAndCrafting.ToolType till lokal ToolType
    public static ToolType ConvertFromInventoryToolType(InventoryAndCrafting.ToolType inventoryToolType)
    {
        switch (inventoryToolType)
        {
            case InventoryAndCrafting.ToolType.None: return ToolType.None;
            case InventoryAndCrafting.ToolType.Axe: return ToolType.Axe;
            case InventoryAndCrafting.ToolType.Bottle: return ToolType.Bottle;
            case InventoryAndCrafting.ToolType.Drill: return ToolType.Drill;
            case InventoryAndCrafting.ToolType.FishingRod: return ToolType.FishingRod;
            case InventoryAndCrafting.ToolType.Hammer: return ToolType.Hammer;
            case InventoryAndCrafting.ToolType.Knife: return ToolType.Knife;
            case InventoryAndCrafting.ToolType.Hoe: return ToolType.Hoe;
            case InventoryAndCrafting.ToolType.Pickaxe: return ToolType.Pickaxe;
            case InventoryAndCrafting.ToolType.Shovel: return ToolType.Shovel;
            case InventoryAndCrafting.ToolType.Syringe: return ToolType.Syringe;
            case InventoryAndCrafting.ToolType.StickPoint: return ToolType.StickPoint;
            case InventoryAndCrafting.ToolType.Wrench: return ToolType.Wrench;
            case InventoryAndCrafting.ToolType.Sickle: return ToolType.None; // Hantera specialfall
            default: return ToolType.None;
        }
    }

    private bool DetectAndHarvestResources()
    {
        bool didHarvestSomething = false;

        // Anv�nd en sf�r f�r att hitta resurser i n�rheten
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);

        if (hitColliders.Length > 0 && debugMode)
        {
            Debug.Log($"Detekterade {hitColliders.Length} objekt runt verktyget");
        }

        foreach (var hitCollider in hitColliders)
        {
            // Kontrollera om det tr�ffade objektet �r en kompatibel resurs
            if (IsCompatibleResource(hitCollider.tag))
            {
                if (debugMode)
                    Debug.Log($"Hittade kompatibel resurs med tag: {hitCollider.tag}");

                // F�rs�k hitta och interagera med ResourceNode
                ResourceNode resourceNode = hitCollider.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    resourceNode.Harvest(this);
                    didHarvestSomething = true;

                    if (debugMode)
                        Debug.Log($"Lyckades sk�rda fr�n {hitCollider.name}");
                }
            }
        }

        return didHarvestSomething;
    }

    private bool IsCompatibleResource(string tag)
    {
        // Om inga kompatibla tags �r specificerade, acceptera "Resource" taggen
        if (compatibleTags == null || compatibleTags.Length == 0)
        {
            return tag == "Resource";
        }

        // Annars kolla om taggen matchar n�gon av de kompatibla
        foreach (var compatibleTag in compatibleTags)
        {
            if (compatibleTag == tag) return true;
        }

        return false;
    }

    public void RepairTool(float repairAmount)
    {
        durability += repairAmount;
        durability = Mathf.Min(durability, maxDurability);
        Debug.Log($"{toolType} reparerat. Ny h�llbarhet: {durability}/{maxDurability}");
    }

    public void OnUnequipped()
    {
        isEquipped = false;

        // �terst�ll IK om det var aktivt
        if (usesTwoHands && leftHandIK != null)
        {
            leftHandIK.ikActive = false;
            leftHandIK.leftHandObj = null;
            Debug.Log("IK-kontroll inaktiverad vid utrustning.");
        }
    }

    // Visualisera r�ckvidden i editorn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);

        // Visa v�nsterhandens position om det �r ett tv�handsverktyg
        if (usesTwoHands)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.TransformDirection(leftHandPosition), 0.05f);
        }

        // Visa verktygets position och rotation
        Gizmos.color = Color.blue;
        Vector3 positionWithOffset = transform.position + transform.TransformDirection(toolPositionOffset);
        Gizmos.DrawSphere(positionWithOffset, 0.05f);

        // Visa rotationen genom att dra en linje i den riktningen
        Gizmos.color = Color.green;
        Quaternion rotationWithOffset = transform.rotation * Quaternion.Euler(toolRotationOffset);
        Vector3 direction = rotationWithOffset * Vector3.forward;
        Gizmos.DrawRay(transform.position, direction * 0.5f);
    }
}