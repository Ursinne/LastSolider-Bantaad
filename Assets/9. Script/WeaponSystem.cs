using UnityEngine;
using System.Collections;
using InventoryAndCrafting;

public enum WeaponType
{
    None,
    Sword,
    Dagger,
    Axe,
    Spear,
    Bow,
    Crossbow,
    Rifle,
    Shotgun,
    Pistol,
    Explosive
}

public class WeaponSystem : MonoBehaviour
{

    [Header("Crosshair Integration")]
    public bool useExactCrosshairAiming = true;
    public bool rotateTowardsCrosshair = true;  // <-- NY INSTÄLLNING
    public float rotationSpeed = 10f;           // <-- NY INSTÄLLNING

    [Header("Weapon Settings")]
    public WeaponType weaponType = WeaponType.None;
    public float damage = 10f;
    public float range = 2f;
    public float attackSpeed = 1f;
    public float durability = 100f;
    public float maxDurability = 100f;
    public float useAmount = 1.0f;  // Hur mycket durability f�rbrukas per anv�ndning

    [Header("Position & Rotation Settings")]
    public Vector3 weaponPositionOffset = Vector3.zero;
    public Vector3 weaponRotationOffset = Vector3.zero;

    [Header("Grip Settings")]
    public bool usesTwoHands = false;
    public Vector3 leftHandPosition = new Vector3(0, 0, -0.3f);

    [Header("Animation Settings")]
    public string animationTrigger = ""; // Detta kommer att s�ttas baserat p� vapentypen
    public string animationBoolParam = ""; // F�r animationer som anv�nder boolean ist�llet f�r trigger

    [Header("Attack Settings")]
    public float attackRadius = 1.0f;
    public LayerMask targetLayers;  // Lager f�r fiender och andra objekt som kan skadas

    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 30f;
    public int ammoCount = 30;
    public int maxAmmo = 30;

    [Header("Debug Settings")]
    public bool debugMode = true;

    [Header("References")]
    private Transform playerTransform;
    private Animator playerAnimator;
    private PlayerSkills playerSkills;
    private IKControlLeft leftHandIK;
    private PlayerAnimationController animationController;

    private bool isEquipped = false;
    private bool isAttacking = false;
    private bool isWeaponUsable = true;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        // St�ll in animation trigger baserat p� vapentyp
        SetAnimationTriggerForWeaponType();
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

        // St�ll in vapnet baserat p� dess typ
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
            Debug.Log($"Vapen initialiserat: {weaponType}, Animation: {animationTrigger}, Bool Param: {animationBoolParam}");
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

            // Kontrollera om vi kan hitta alternativa triggers f�r sv�rdet/attacken
            if (weaponType == WeaponType.Sword)
            {
                if (param.name == "Attack" || param.name == "SwordAttack" || param.name == "Slash")
                {
                    animationTrigger = param.name;
                    foundTrigger = true;
                    Debug.Log($"Hittade alternativ sv�rdsanimation: {param.name}");
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
        // Uppdatera vapnets position och rotation om det är utrustat
        if (isEquipped)
        {
            UpdateTransform();



            // Uppdatera cooldown-timer
            if (!isWeaponUsable)
            {
                if (cooldownTimer > 0)
                {
                    cooldownTimer -= Time.deltaTime;
                }
                else
                {
                    isWeaponUsable = true;
                }
            }
        }

        // Resten av input-hantering samma som innan...
        if (isEquipped && Input.GetButtonDown("Fire1"))
        {
            WeaponAimHandler aimHandler = GetComponent<WeaponAimHandler>();

            if (IsRangedWeapon() && aimHandler != null && aimHandler.aimTarget != null)
            {
                UseWeapon(aimHandler.aimTarget.position);
            }
            else
            {
                UseWeapon();
            }
        }
    }

    private bool IsRangedWeapon()
    {
        return weaponType == WeaponType.Bow ||
               weaponType == WeaponType.Crossbow ||
               weaponType == WeaponType.Rifle ||
               weaponType == WeaponType.Shotgun ||
               weaponType == WeaponType.Pistol;
    }

    private bool PerformWeaponAction(Vector3? targetPoint = null)
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Dagger:
            case WeaponType.Axe:
            case WeaponType.Spear:
                return PerformMeleeAttack();
            case WeaponType.Bow:
            case WeaponType.Crossbow:
            case WeaponType.Rifle:
            case WeaponType.Shotgun:
            case WeaponType.Pistol:
                return PerformRangedAttack(targetPoint);
            case WeaponType.Explosive:
                return ThrowExplosive();
            default:
                return false;
        }
    }

    private void UpdateTransform()
    {
        // S�tt lokal position och rotation
        transform.localPosition = weaponPositionOffset;
        transform.localRotation = Quaternion.Euler(weaponRotationOffset);
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
            Debug.Log($"Konfigurerade IK f�r tv�handsgrepp p� {weaponType}");

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
        // S�tt standardv�rden baserat p� vapentyp
        switch (weaponType)
        {
            case WeaponType.Sword:
                damage = 15f;
                range = 2.5f;
                attackSpeed = 1.2f;
                attackRadius = 1.5f;
                break;
            case WeaponType.Dagger:
                damage = 8f;
                range = 1.0f;
                attackSpeed = 1.8f;
                attackRadius = 0.8f;
                break;
            case WeaponType.Axe:
                damage = 18f;
                range = 2.0f;
                attackSpeed = 0.8f;
                attackRadius = 1.3f;
                usesTwoHands = true;
                break;
            case WeaponType.Spear:
                damage = 12f;
                range = 3.0f;
                attackSpeed = 1.0f;
                attackRadius = 2.0f;
                usesTwoHands = true;
                break;
            case WeaponType.Bow:
                damage = 10f;
                range = 20f;
                attackSpeed = 0.7f;
                usesTwoHands = true;
                break;
            case WeaponType.Crossbow:
                damage = 15f;
                range = 25f;
                attackSpeed = 0.5f;
                usesTwoHands = true;
                break;
            case WeaponType.Rifle:
                damage = 25f;
                range = 50f;
                attackSpeed = 5f; // Öka detta värde för snabbare skottlossning
                usesTwoHands = true;
                break;
            case WeaponType.Shotgun:
                damage = 20f;
                range = 15f;
                attackSpeed = 0.4f;
                usesTwoHands = true;
                break;
            case WeaponType.Pistol:
                damage = 12f;
                range = 30f;
                attackSpeed = 0.8f;
                break;
        }

        // Konfigurera v�nsterhandens position f�r tv�handsverktyg
        if (usesTwoHands)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle:
                case WeaponType.Shotgun:
                    leftHandPosition = new Vector3(0, -0.05f, -0.2f);
                    break;
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    leftHandPosition = new Vector3(0, -0.1f, -0.3f);
                    break;
                case WeaponType.Axe:
                case WeaponType.Spear:
                    leftHandPosition = new Vector3(0, -0.2f, -0.3f);
                    break;
            }
        }
    }

    public void SetAnimationTriggerForWeaponType()
    {
        // S�tt animation trigger baserat p� vapentypen
        switch (weaponType)
        {
            case WeaponType.Sword:
                animationTrigger = "Attack";
                animationBoolParam = "isMeleeing";
                break;
            case WeaponType.Dagger:
                animationTrigger = "Stab";
                animationBoolParam = "isStabbing";
                break;
            case WeaponType.Axe:
                animationTrigger = "AxeAttack";
                animationBoolParam = "isMeleeing";
                break;
            case WeaponType.Spear:
                animationTrigger = "SpearThrust";
                animationBoolParam = "isThrusting";
                break;
            case WeaponType.Bow:
                animationTrigger = "ShootBow";
                animationBoolParam = "isShooting";
                break;
            case WeaponType.Crossbow:
                animationTrigger = "ShootCrossbow";
                animationBoolParam = "isShooting";
                break;
            case WeaponType.Rifle:
            case WeaponType.Shotgun:
            case WeaponType.Pistol:
                animationTrigger = "Shoot";
                animationBoolParam = "isShooting";
                break;
            case WeaponType.Explosive:
                animationTrigger = "Throw";
                animationBoolParam = "isThrowing";
                break;
            default:
                animationTrigger = "Attack";
                animationBoolParam = "isAttacking";
                break;
        }
    }

    public bool UseWeapon(Vector3? targetPoint = null)
    {
        if (!isWeaponUsable)
        {
            if (debugMode)
                Debug.Log($"{weaponType} kan inte anv�ndas just nu (nedkylning p�g�r)");
            return false;
        }

        if (durability <= 0)
        {
            Debug.Log($"{weaponType} �r trasigt och beh�ver repareras!");
            return false;
        }

        // S�tt vapnet p� cooldown och starta timer
        isWeaponUsable = false;
        cooldownTimer = 1.0f / attackSpeed; // Anv�nd attackSpeed f�r att ber�kna cooldown

        // Spela animation 
        bool animationStarted = PlayWeaponAnimation();

        // Minska vapnets h�llbarhet
        durability -= useAmount;
        durability = Mathf.Max(0, durability);

        // �ka f�rdighet baserat p� vapentyp
        IncrementSkill();

        // Utf�r specifik vapenlogik baserat p� typ
        bool hitSomething = PerformWeaponAction();

        return hitSomething || animationStarted;
    }

    private bool PlayWeaponAnimation()
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
            string[] commonTriggers = { "Attack", "Melee", "Slash", "Swing", "Shoot" };
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
        if (debugMode) Debug.LogWarning("Kunde inte spela n�gon animation f�r vapnet!");
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

        // �ka f�rdighet baserat p� vapentyp
        switch (weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Dagger:
            case WeaponType.Spear:
                playerSkills.GainOldSkillExp("armed", 5f);
                break;
            case WeaponType.Bow:
            case WeaponType.Crossbow:
            case WeaponType.Rifle:
            case WeaponType.Shotgun:
            case WeaponType.Pistol:
                playerSkills.GainOldSkillExp("range", 5f);
                break;
        }
    }

    private bool PerformWeaponAction()
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Dagger:
            case WeaponType.Axe:
            case WeaponType.Spear:
                return PerformMeleeAttack();
            case WeaponType.Bow:
            case WeaponType.Crossbow:
            case WeaponType.Rifle:
            case WeaponType.Shotgun:
            case WeaponType.Pistol:
                return PerformRangedAttack();
            case WeaponType.Explosive:
                return ThrowExplosive();
            default:
                return false;
        }
    }

    private bool PerformMeleeAttack()
    {
        bool hitSomething = false;

        // Anv�nd en sf�r f�r att hitta fiender inom attackradie
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position + transform.forward * range / 2,
            attackRadius,
            targetLayers
        );

        foreach (var hitCollider in hitColliders)
        {
            // Kontrollera om det tr�ffade objektet �r en fiende
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitSomething = true;

                if (debugMode)
                    Debug.Log($"Tr�ffade fiende: {hitCollider.name} f�r {damage} skada");
            }

            // Kontrollera om det tr�ffade objektet �r en NPC
            NpcZombie1 zombie = hitCollider.GetComponent<NpcZombie1>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                hitSomething = true;

                if (debugMode)
                    Debug.Log($"Tr�ffade zombie: {hitCollider.name} f�r {damage} skada");
            }

            // Kontrollera andra typer av fiender eller tr�ffbara objekt h�r
        }

        return hitSomething;
    }

    private Vector3 GetExactCrosshairTarget()
    {
        CrosshairManager crosshair = FindObjectOfType<CrosshairManager>();
        if (crosshair != null)
        {
            Vector3 target = crosshair.GetAimWorldPosition();
            // Debug.Log($"VAPEN får crosshair-mål: {target}");  // <-- TA BORT DENNA RAD
            return target;
        }

        // Fallback
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 fallbackTarget = ray.origin + ray.direction * range;
            // Debug.Log($"VAPEN använder fallback-mål: {fallbackTarget}");  // <-- TA BORT DENNA
            return fallbackTarget;
        }

        Vector3 weaponTarget = transform.position + transform.forward * range;
        // Debug.Log($"VAPEN använder vapen-forward-mål: {weaponTarget}");  // <-- TA BORT DENNA
        return weaponTarget;
    }

    private bool PerformRangedAttack(Vector3? targetPoint = null)
    {
        if (ammoCount <= 0)
        {
            Debug.Log("Slut på ammunition!");
            return false;
        }

        // Minska ammo
        ammoCount--;

        // Bestäm var projektilen ska startas
        Transform spawnTransform = projectileSpawnPoint != null ?
            projectileSpawnPoint : transform;

        // Använd exakt crosshair-mål
        Vector3 exactTarget = useExactCrosshairAiming ?
            GetExactCrosshairTarget() :
            (targetPoint ?? spawnTransform.position + spawnTransform.forward * range);

        Vector3 direction = (exactTarget - spawnTransform.position).normalized;



        // Om vi har en projektilprefab, skjut den
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                spawnTransform.position,
                Quaternion.LookRotation(direction)
            );

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }

            if (debugMode)
                Debug.Log($"Sköt projektil mot EXAKT crosshair-mål. Ammo kvar: {ammoCount}");

            return true;
        }
        else
        {
            // Raycasting för vapnet om ingen projektil finns
            if (Physics.Raycast(
                spawnTransform.position,
                direction,
                out RaycastHit hit,
                range,
                targetLayers))
            {
                // Träffade något
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    if (debugMode)
                        Debug.Log($"Träffade fiende med EXAKT crosshair-sikte: {hit.collider.name}");
                    return true;
                }

                NpcZombie1 zombie = hit.collider.GetComponent<NpcZombie1>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage);
                    if (debugMode)
                        Debug.Log($"Träffade zombie med EXAKT crosshair-sikte: {hit.collider.name}");
                    return true;
                }
            }
        }

        return false; // <-- DENNA RAD SAKNADES!
    }


    private bool ThrowExplosive()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Inget explosivt projektil tilldelat!");
            return false;
        }

        // Best�m var projektilen ska startas
        Transform spawnTransform = projectileSpawnPoint != null ?
            projectileSpawnPoint : transform;

        // Skapa projektilen
        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnTransform.position,
            spawnTransform.rotation
        );

        // Konfigurera projektilen
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Kasta med en b�ge
            Vector3 throwDirection = spawnTransform.forward + Vector3.up * 0.5f;
            rb.linearVelocity = throwDirection.normalized * projectileSpeed;
        }

        // Om det finns en Explosive-komponent, konfigurera den
        // H�r kan du l�gga till egen explosiv-logik om du beh�ver

        if (debugMode)
            Debug.Log($"Kastade explosiv projektil");

        return true;
    }

    public void ReloadWeapon(int amount)
    {
        // Fyll p� ammunition
        ammoCount = Mathf.Min(ammoCount + amount, maxAmmo);
        Debug.Log($"Laddade om {weaponType}. Ammunition: {ammoCount}/{maxAmmo}");
    }

    public void RepairWeapon(float repairAmount)
    {
        durability += repairAmount;
        durability = Mathf.Min(durability, maxDurability);
        Debug.Log($"{weaponType} reparerat. Ny h�llbarhet: {durability}/{maxDurability}");
    }

    public void OnUnequipped()
    {
        isEquipped = false;

        // �terst�ll IK om det var aktivt
        if (usesTwoHands && leftHandIK != null)
        {
            leftHandIK.ikActive = false;
            leftHandIK.leftHandObj = null;
            Debug.Log("IK-kontroll inaktiverad vid avequipering.");
        }
    }

    // Visualisera r�ckvidden i editorn
    private void OnDrawGizmosSelected()
    {
        // Rita attackradie f�r n�rstridsvapen
        if (weaponType == WeaponType.Sword || weaponType == WeaponType.Dagger ||
            weaponType == WeaponType.Axe || weaponType == WeaponType.Spear)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * range / 2, attackRadius);
        }

        // Rita r�ckvidd f�r avst�ndsvapen
        if (weaponType == WeaponType.Bow || weaponType == WeaponType.Crossbow ||
            weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun ||
            weaponType == WeaponType.Pistol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * range);
        }

        // Visa v�nsterhandens position om det �r ett tv�handsvapen
        if (usesTwoHands)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.TransformDirection(leftHandPosition), 0.05f);
        }

        // Visa vapnets position och rotation
        Gizmos.color = Color.yellow;
        Vector3 positionWithOffset = transform.position + transform.TransformDirection(weaponPositionOffset);
        Gizmos.DrawSphere(positionWithOffset, 0.05f);

        // Visa rotationen genom att dra en linje i den riktningen
        Gizmos.color = Color.green;
        Quaternion rotationWithOffset = transform.rotation * Quaternion.Euler(weaponRotationOffset);
        Vector3 direction = rotationWithOffset * Vector3.forward;
        Gizmos.DrawRay(transform.position, direction * 0.5f);
    }
}