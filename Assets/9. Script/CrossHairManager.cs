using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Förbättrad CrosshairManager med FPS-funktionalitet
/// Behåller alla ursprungliga funktioner + lägger till exakt siktesfunktion
/// </summary>
public class CrosshairManager : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Image crosshairImage;             // Referens till UI Image-komponenten för siktet
    public Sprite defaultCrosshair;          // Standardsikte
    public Sprite aimingCrosshair;           // Sikte när man siktar
    public Sprite shootingCrosshair;         // Sikte vid skjutning
    public Sprite hitCrosshair;              // Sikte när man träffar ett mål

    [Header("Dynamic Crosshair")]
    public bool useDynamicCrosshair = true;  // Om siktet ska ändra storlek baserat på rörelse/precision
    public float minSize = 20f;              // Minsta storlek på siktet (när spelaren står still)
    public float maxSize = 60f;              // Största storlek på siktet (när spelaren rör sig snabbt)
    public float movementMultiplier = 2f;    // Hur mycket spelarens rörelse påverkar siktet
    public float shootingExpansion = 20f;    // Hur mycket siktet expanderar när man skjuter
    public float sizeSmoothing = 5f;         // Hur snabbt siktet återgår till normal storlek

    [Header("Hit Indicator")]
    public bool showHitIndicator = true;     // Om träffindikatorn ska visas
    public Color hitColor = Color.red;       // Färg för träffindikatorn
    public float hitColorDuration = 0.2f;    // Hur länge träffindikatorn visas
    public float hitDisplayTime = 0.1f;      // Hur länge träff-siktet visas

    [Header("FPS Aiming Settings")]
    public LayerMask aimLayers = -1;         // Vilka lager crosshair kan träffa
    public float maxAimRange = 100f;         // Max räckvidd för sikte
    public bool showDebugRay = true;         // Visa debug-ray i scene view

    [Header("References")]
    public WeaponSystem weaponSystem;        // Referens till vapensystemet
    public GameObject player;                // Referens till spelaren
    public Transform aimTarget;              // Referens till Aimball/målobjektet

    // Privata variabler
    private RectTransform crosshairRect;
    private Color defaultColor;
    private Vector2 defaultSize;
    private Vector2 targetSize;
    private CharacterController characterController;
    private float lastShotTime;
    private bool isAiming = false;
    private Camera mainCamera;
    private float hitTimer = 0f;

    // Nya variabler för FPS-funktionalitet
    private Vector3 currentAimPosition;      // Aktuell sikteposition i världen

    void Start()
    {
        InitializeComponents();
        InitializeCrosshair();
    }

    /// <summary>
    /// Initialiserar alla komponenter och referenser
    /// </summary>
    void InitializeComponents()
    {
        // Hitta referenser om de inte redan är tilldelade
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();

        if (weaponSystem == null && player != null)
            weaponSystem = player.GetComponentInChildren<WeaponSystem>();

        // Sök efter Aimball i Invector-komponenten om den inte är tilldelad
        if (aimTarget == null && player != null)
        {
            var thirdPersonInput = player.GetComponent<Invector.vCharacterController.vThirdPersonInput>();
            if (thirdPersonInput != null && thirdPersonInput.Aimball != null)
            {
                aimTarget = thirdPersonInput.Aimball;
                Debug.Log("Hittade Aimball från Invector-systemet");
            }
        }

        // Hämta komponenter
        crosshairRect = crosshairImage.GetComponent<RectTransform>();
        characterController = player?.GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Initialiserar crosshair-inställningar
    /// </summary>
    void InitializeCrosshair()
    {
        // Spara standardvärden
        defaultColor = crosshairImage.color;
        defaultSize = crosshairRect.sizeDelta;
        targetSize = defaultSize;

        // Sätt initialvärden
        if (defaultCrosshair != null)
            crosshairImage.sprite = defaultCrosshair;
    }

    void Update()
    {
        // Kontrollera aiming-status från input eller vapensystem
        CheckAimingStatus();

        // Uppdatera siktet baserat på rörelser om dynamiskt sikte är aktiverat
        if (useDynamicCrosshair)
            UpdateCrosshairSize();

        // Smootht uppdatera sikte-storlek
        crosshairRect.sizeDelta = Vector2.Lerp(crosshairRect.sizeDelta, targetSize, Time.deltaTime * sizeSmoothing);

        // Utför raycast från siktet för att hitta potentiella mål
        PerformCrosshairRaycast();

        // Hantera träff-timer
        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                ShowNormal();
            }
        }
    }

    /// <summary>
    /// Kontrollerar aiming-status och input
    /// </summary>
    private void CheckAimingStatus()
    {
        // Kontrollera om spelaren siktar (höger musknapp)
        bool wasAiming = isAiming;
        isAiming = Input.GetMouseButton(1);

        // Ändra sikte beroende på om spelaren siktar eller inte
        if (isAiming != wasAiming)
        {
            if (hitTimer <= 0) // Bara ändra om vi inte visar träffsikte
            {
                crosshairImage.sprite = isAiming ? aimingCrosshair : defaultCrosshair;
            }

            // Om spelaren slutar sikta, återställ storlek
            if (!isAiming)
                targetSize = defaultSize;
        }

        // Ändra sikte vid skjutning
        if (Input.GetMouseButtonDown(0))
        {
            if (shootingCrosshair != null)
                StartCoroutine(ShowShootingCrosshair());

            // Expandera siktet vid skjutning
            targetSize = new Vector2(defaultSize.x + shootingExpansion, defaultSize.y + shootingExpansion);
            lastShotTime = Time.time;
        }
    }

    /// <summary>
    /// Uppdaterar crosshair-storlek baserat på rörelse
    /// </summary>
    private void UpdateCrosshairSize()
    {
        if (characterController == null)
            return;

        // Basera siktesstorlek på hur mycket spelaren rör sig
        float movementFactor = characterController.velocity.magnitude * movementMultiplier;

        // Om spelaren siktar, minska effekten av rörelse
        if (isAiming)
            movementFactor *= 0.5f;

        // Beräkna målstorlek baserat på rörelse, men använd inte mindre än minSize
        float sizeOffset = Mathf.Clamp(movementFactor, 0, maxSize - minSize);
        targetSize = new Vector2(minSize + sizeOffset, minSize + sizeOffset);

        // Om spelaren nyligen skjutit, expandera siktet
        if (Time.time - lastShotTime < 0.2f)
        {
            float expansionFactor = 1 - ((Time.time - lastShotTime) / 0.2f);
            targetSize += new Vector2(shootingExpansion * expansionFactor, shootingExpansion * expansionFactor);
        }
    }

    private void PerformCrosshairRaycast()
    {
        if (mainCamera == null)
            return;

        // PUBG-stil: ALLTID från mitten av skärmen - oavsett kameraläge
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        // Använd maxAimRange från nya inställningar
        float maxRange = maxAimRange;
        if (weaponSystem != null && weaponSystem.range > 0)
        {
            maxRange = weaponSystem.range;
        }

        // Utför raycast med de nya layer-inställningarna
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, aimLayers))
        {
            // LÄGG TILL DENNA DEBUG-RAD:
            //Debug.Log($"Crosshair siktar på: {hit.point}, Avstånd: {hit.distance}");

            // Spara träffpunkten för andra scripts att använda
            currentAimPosition = hit.point;

            // Uppdatera aimTarget om den finns
            if (aimTarget != null)
            {
                aimTarget.position = hit.point;
            }

            // Kontrollera om vi träffar en fiende för visuell feedback
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Hitbox"))
            {
                if (showHitIndicator)
                {
                    StartCoroutine(ShowHitIndicator());
                }
            }

            // Debug-visualisering
            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            }
        }
        else
        {
            // Om vi inte träffar något, sätt målpunkt långt bort
            currentAimPosition = ray.origin + ray.direction * maxRange;

            if (aimTarget != null)
            {
                aimTarget.position = currentAimPosition;
            }

            // Debug-visualisering för miss
            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.yellow, 0.1f);
            }
        }
    }
    public Vector3 GetAimWorldPosition()
    {
        return currentAimPosition;
    }

    /// <summary>
    /// Returnerar riktningen från en position mot crosshair
    /// </summary>
    public Vector3 GetAimDirection(Vector3 fromPosition)
    {
        return (currentAimPosition - fromPosition).normalized;
    }

    /// <summary>
    /// Kontrollerar om crosshair siktar på något träffbart
    /// </summary>
    public bool IsAimingAtTarget()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        return Physics.Raycast(ray, maxAimRange, aimLayers);
    }

    /// <summary>
    /// Returnerar information om vad crosshair siktar på
    /// </summary>
    public RaycastHit GetAimTarget()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        Physics.Raycast(ray, out RaycastHit hit, maxAimRange, aimLayers);
        return hit;
    }

    // ===== BEFINTLIGA METODER (oförändrade) =====

    private IEnumerator ShowShootingCrosshair()
    {
        Sprite originalSprite = crosshairImage.sprite;

        if (hitTimer <= 0)
        {
            crosshairImage.sprite = shootingCrosshair;
        }

        yield return new WaitForSeconds(0.1f);

        if (hitTimer <= 0)
        {
            if (isAiming)
                crosshairImage.sprite = aimingCrosshair;
            else
                crosshairImage.sprite = originalSprite;
        }
    }

    private IEnumerator ShowHitIndicator()
    {
        Color originalColor = crosshairImage.color;
        crosshairImage.color = hitColor;

        yield return new WaitForSeconds(hitColorDuration);

        if (hitTimer <= 0)
        {
            crosshairImage.color = originalColor;
        }
    }

    public void ShowHit()
    {
        if (hitCrosshair != null)
        {
            crosshairImage.sprite = hitCrosshair;
            crosshairImage.color = hitColor;
            hitTimer = hitDisplayTime;
        }
        else
        {
            StartCoroutine(ShowHitIndicator());
        }
    }

    public void ShowNormal()
    {
        crosshairImage.color = defaultColor;

        if (isAiming && aimingCrosshair != null)
            crosshairImage.sprite = aimingCrosshair;
        else if (defaultCrosshair != null)
            crosshairImage.sprite = defaultCrosshair;

        hitTimer = 0f;
    }

    public void ShowCrosshair(bool show)
    {
        crosshairImage.enabled = show;
    }

    public void SetCrosshairSprite(Sprite newCrosshair)
    {
        if (newCrosshair != null)
            defaultCrosshair = newCrosshair;

        if (!isAiming && hitTimer <= 0)
            crosshairImage.sprite = defaultCrosshair;
    }
}