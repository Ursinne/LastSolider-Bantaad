using UnityEngine;
using Invector.vCharacterController;

public class WeaponAimHandler : MonoBehaviour
{
    [Header("Aiming Settings")]
    public Transform aimTarget;            // Referens till Aimball f�r att visualisera m�lpunkten
    public Vector3 rotationOffset = Vector3.zero; // Offset f�r vapnets rotation

    [Header("References")]
    public Camera mainCamera;              // Kameran som anv�nds f�r siktet
    public WeaponSystem weaponSystem;      // Referens till vapensystemet
    public CrosshairManager crosshairManager; // Referens till crosshair manager
    public Transform weaponTransform;      // Transform f�r sj�lva vapnet som ska rotera

    // Privata variabler
    private vThirdPersonInput thirdPersonInput;
    private bool initialized = false;
    private LayerMask targetLayers;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Hitta n�dv�ndiga komponenter om de inte �r tilldelade
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (crosshairManager == null)
            crosshairManager = FindObjectOfType<CrosshairManager>();

        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();

        // Om vapnet inte �r tilldelat, anv�nd detta objekt
        if (weaponTransform == null)
            weaponTransform = transform;

        // H�mta Invector-komponenter
        if (transform.root != null)
        {
            thirdPersonInput = transform.root.GetComponent<vThirdPersonInput>();

            // Anv�nd Aimball fr�n Invector om den finns
            if (thirdPersonInput != null && thirdPersonInput.Aimball != null)
            {
                aimTarget = thirdPersonInput.Aimball;
                Debug.Log("Anv�nder Aimball fr�n Invector som m�lpunkt");
            }
            else if (aimTarget == null)
            {
                // Skapa en Aimball om den inte finns
                GameObject aimObj = new GameObject("Aimball");
                aimTarget = aimObj.transform;
                Debug.Log("Skapade ny Aimball eftersom Invector-Aimball inte hittades");
            }
        }

        // Anv�nd targetLayers fr�n vapensystemet om det finns
        if (weaponSystem != null)
        {
            targetLayers = weaponSystem.targetLayers;
            Debug.Log("Anv�nder targetLayers fr�n vapensystemet");
        }
        else
        {
            // Standardlager om inget �r definierat
            targetLayers = LayerMask.GetMask("Default", "Enemy");
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            InitializeComponents();

        // Uppdatera m�lpunkt f�r siktet
        UpdateAimPoint();

        // Rotera vapnet mot m�lpunkten
        RotateWeaponTowardsTarget();

        // Anropa vapensystemets UseWeapon n�r anv�ndaren skjuter
        if (Input.GetButtonDown("Fire1") && weaponSystem != null)
        {
            if (IsRangedWeapon(weaponSystem.weaponType) && aimTarget != null)
            {
                weaponSystem.UseWeapon(aimTarget.position); // Skicka med m�lpositionen!
            }
        }
    }

    private void RotateWeaponTowardsTarget()
    {
        if (weaponTransform != null && aimTarget != null)
        {
            // Ber�kna riktningen fr�n vapnet mot m�lpunkten
            Vector3 targetDirection = aimTarget.position - weaponTransform.position;

            // Skapa en rotation mot m�let med offset
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(rotationOffset);

            // Applicera rotation med smooth interpolation
            weaponTransform.rotation = Quaternion.Slerp(
                weaponTransform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );

            Debug.DrawRay(weaponTransform.position, targetDirection.normalized * 2f, Color.blue);
        }
    }

    private void UpdateAimPoint()
    {
        if (mainCamera == null || aimTarget == null)
            return;

        // R�kna ut mitten av sk�rmen (d�r crosshair �r)
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Skapa en ray fr�n kameran genom crosshair
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        // Ber�kna maximal r�ckvidd baserat p� vapentyp
        float maxRange = 100f;
        if (weaponSystem != null)
            maxRange = weaponSystem.range;

        // Skjut iv�g rayen och se var den tr�ffar
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetLayers))
        {
            // Uppdatera aimTarget positionen
            aimTarget.position = hit.point;

            // Uppdatera crosshair om en fiende tr�ffades
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Hitbox"))
            {
                if (crosshairManager != null)
                {
                    crosshairManager.ShowHit();
                }
            }
        }
        else
        {
            // Om rayen inte tr�ffade n�got, s�tt ett m�l l�ngt bort i rayens riktning
            aimTarget.position = ray.origin + ray.direction * maxRange;
        }

        // Rita en Debug-linje f�r att visualisera siktet
        Debug.DrawLine(ray.origin, aimTarget.position, Color.red);
    }

    // Hj�lpmetod f�r att kontrollera om det �r ett avst�ndsvapen
    private bool IsRangedWeapon(WeaponType type)
    {
        return type == WeaponType.Bow ||
               type == WeaponType.Crossbow ||
               type == WeaponType.Rifle ||
               type == WeaponType.Shotgun ||
               type == WeaponType.Pistol;
    }
}