using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// F�rb�ttrad CrosshairManager med FPS-funktionalitet
/// Beh�ller alla ursprungliga funktioner + l�gger till exakt siktesfunktion
/// </summary>
public class CrosshairManager : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Image crosshairImage;             // Referens till UI Image-komponenten f�r siktet
    public Sprite defaultCrosshair;          // Standardsikte
    public Sprite aimingCrosshair;           // Sikte n�r man siktar
    public Sprite shootingCrosshair;         // Sikte vid skjutning
    public Sprite hitCrosshair;              // Sikte n�r man tr�ffar ett m�l

    [Header("Dynamic Crosshair")]
    public bool useDynamicCrosshair = true;  // Om siktet ska �ndra storlek baserat p� r�relse/precision
    public float minSize = 20f;              // Minsta storlek p� siktet (n�r spelaren st�r still)
    public float maxSize = 60f;              // St�rsta storlek p� siktet (n�r spelaren r�r sig snabbt)
    public float movementMultiplier = 2f;    // Hur mycket spelarens r�relse p�verkar siktet
    public float shootingExpansion = 20f;    // Hur mycket siktet expanderar n�r man skjuter
    public float sizeSmoothing = 5f;         // Hur snabbt siktet �terg�r till normal storlek

    [Header("Hit Indicator")]
    public bool showHitIndicator = true;     // Om tr�ffindikatorn ska visas
    public Color hitColor = Color.red;       // F�rg f�r tr�ffindikatorn
    public float hitColorDuration = 0.2f;    // Hur l�nge tr�ffindikatorn visas
    public float hitDisplayTime = 0.1f;      // Hur l�nge tr�ff-siktet visas

    [Header("FPS Aiming Settings")]
    public LayerMask aimLayers = -1;         // Vilka lager crosshair kan tr�ffa
    public float maxAimRange = 100f;         // Max r�ckvidd f�r sikte
    public bool showDebugRay = true;         // Visa debug-ray i scene view

    [Header("References")]
    public WeaponSystem weaponSystem;        // Referens till vapensystemet
    public GameObject player;                // Referens till spelaren
    public Transform aimTarget;              // Referens till Aimball/m�lobjektet

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

    // Nya variabler f�r FPS-funktionalitet
    private Vector3 currentAimPosition;      // Aktuell sikteposition i v�rlden

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
        // Hitta referenser om de inte redan �r tilldelade
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();

        if (weaponSystem == null && player != null)
            weaponSystem = player.GetComponentInChildren<WeaponSystem>();

        // S�k efter Aimball i Invector-komponenten om den inte �r tilldelad
        if (aimTarget == null && player != null)
        {
            var thirdPersonInput = player.GetComponent<Invector.vCharacterController.vThirdPersonInput>();
            if (thirdPersonInput != null && thirdPersonInput.Aimball != null)
            {
                aimTarget = thirdPersonInput.Aimball;
                Debug.Log("Hittade Aimball fr�n Invector-systemet");
            }
        }

        // H�mta komponenter
        crosshairRect = crosshairImage.GetComponent<RectTransform>();
        characterController = player?.GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Initialiserar crosshair-inst�llningar
    /// </summary>
    void InitializeCrosshair()
    {
        // Spara standardv�rden
        defaultColor = crosshairImage.color;
        defaultSize = crosshairRect.sizeDelta;
        targetSize = defaultSize;

        // S�tt initialv�rden
        if (defaultCrosshair != null)
            crosshairImage.sprite = defaultCrosshair;
    }

    void Update()
    {
        // Kontrollera aiming-status fr�n input eller vapensystem
        CheckAimingStatus();

        // Uppdatera siktet baserat p� r�relser om dynamiskt sikte �r aktiverat
        if (useDynamicCrosshair)
            UpdateCrosshairSize();

        // Smootht uppdatera sikte-storlek
        crosshairRect.sizeDelta = Vector2.Lerp(crosshairRect.sizeDelta, targetSize, Time.deltaTime * sizeSmoothing);

        // Utf�r raycast fr�n siktet f�r att hitta potentiella m�l
        PerformCrosshairRaycast();

        // Hantera tr�ff-timer
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
        // Kontrollera om spelaren siktar (h�ger musknapp)
        bool wasAiming = isAiming;
        isAiming = Input.GetMouseButton(1);

        // �ndra sikte beroende p� om spelaren siktar eller inte
        if (isAiming != wasAiming)
        {
            if (hitTimer <= 0) // Bara �ndra om vi inte visar tr�ffsikte
            {
                crosshairImage.sprite = isAiming ? aimingCrosshair : defaultCrosshair;
            }

            // Om spelaren slutar sikta, �terst�ll storlek
            if (!isAiming)
                targetSize = defaultSize;
        }

        // �ndra sikte vid skjutning
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
    /// Uppdaterar crosshair-storlek baserat p� r�relse
    /// </summary>
    private void UpdateCrosshairSize()
    {
        if (characterController == null)
            return;

        // Basera siktesstorlek p� hur mycket spelaren r�r sig
        float movementFactor = characterController.velocity.magnitude * movementMultiplier;

        // Om spelaren siktar, minska effekten av r�relse
        if (isAiming)
            movementFactor *= 0.5f;

        // Ber�kna m�lstorlek baserat p� r�relse, men anv�nd inte mindre �n minSize
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

        // PUBG-stil: ALLTID fr�n mitten av sk�rmen - oavsett kameral�ge
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        // Anv�nd maxAimRange fr�n nya inst�llningar
        float maxRange = maxAimRange;
        if (weaponSystem != null && weaponSystem.range > 0)
        {
            maxRange = weaponSystem.range;
        }

        // Utf�r raycast med de nya layer-inst�llningarna
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, aimLayers))
        {
            // L�GG TILL DENNA DEBUG-RAD:
            //Debug.Log($"Crosshair siktar p�: {hit.point}, Avst�nd: {hit.distance}");

            // Spara tr�ffpunkten f�r andra scripts att anv�nda
            currentAimPosition = hit.point;

            // Uppdatera aimTarget om den finns
            if (aimTarget != null)
            {
                aimTarget.position = hit.point;
            }

            // Kontrollera om vi tr�ffar en fiende f�r visuell feedback
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
            // Om vi inte tr�ffar n�got, s�tt m�lpunkt l�ngt bort
            currentAimPosition = ray.origin + ray.direction * maxRange;

            if (aimTarget != null)
            {
                aimTarget.position = currentAimPosition;
            }

            // Debug-visualisering f�r miss
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
    /// Returnerar riktningen fr�n en position mot crosshair
    /// </summary>
    public Vector3 GetAimDirection(Vector3 fromPosition)
    {
        return (currentAimPosition - fromPosition).normalized;
    }

    /// <summary>
    /// Kontrollerar om crosshair siktar p� n�got tr�ffbart
    /// </summary>
    public bool IsAimingAtTarget()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        return Physics.Raycast(ray, maxAimRange, aimLayers);
    }

    /// <summary>
    /// Returnerar information om vad crosshair siktar p�
    /// </summary>
    public RaycastHit GetAimTarget()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        Physics.Raycast(ray, out RaycastHit hit, maxAimRange, aimLayers);
        return hit;
    }

    // ===== BEFINTLIGA METODER (of�r�ndrade) =====

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