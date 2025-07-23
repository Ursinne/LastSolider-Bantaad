using UnityEngine;
using Invector.vCharacterController;

public class WeaponAimHandler : MonoBehaviour
{
    [Header("Aiming Settings")]
    public Transform aimTarget;            // Referens till Aimball för att visualisera målpunkten
    public Vector3 rotationOffset = Vector3.zero; // Offset för vapnets rotation

    [Header("References")]
    public Camera mainCamera;              // Kameran som används för siktet
    public WeaponSystem weaponSystem;      // Referens till vapensystemet
    public CrosshairManager crosshairManager; // Referens till crosshair manager
    public Transform weaponTransform;      // Transform för själva vapnet som ska rotera

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
        // Hitta nödvändiga komponenter om de inte är tilldelade
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (crosshairManager == null)
            crosshairManager = FindObjectOfType<CrosshairManager>();

        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();

        // Om vapnet inte är tilldelat, använd detta objekt
        if (weaponTransform == null)
            weaponTransform = transform;

        // Hämta Invector-komponenter
        if (transform.root != null)
        {
            thirdPersonInput = transform.root.GetComponent<vThirdPersonInput>();

            // Använd Aimball från Invector om den finns
            if (thirdPersonInput != null && thirdPersonInput.Aimball != null)
            {
                aimTarget = thirdPersonInput.Aimball;
                Debug.Log("Använder Aimball från Invector som målpunkt");
            }
            else if (aimTarget == null)
            {
                // Skapa en Aimball om den inte finns
                GameObject aimObj = new GameObject("Aimball");
                aimTarget = aimObj.transform;
                Debug.Log("Skapade ny Aimball eftersom Invector-Aimball inte hittades");
            }
        }

        // Använd targetLayers från vapensystemet om det finns
        if (weaponSystem != null)
        {
            targetLayers = weaponSystem.targetLayers;
            Debug.Log("Använder targetLayers från vapensystemet");
        }
        else
        {
            // Standardlager om inget är definierat
            targetLayers = LayerMask.GetMask("Default", "Enemy");
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            InitializeComponents();

        // Uppdatera målpunkt för siktet
        UpdateAimPoint();

        // Rotera vapnet mot målpunkten
        RotateWeaponTowardsTarget();

        // Anropa vapensystemets UseWeapon när användaren skjuter
        if (Input.GetButtonDown("Fire1") && weaponSystem != null)
        {
            if (IsRangedWeapon(weaponSystem.weaponType) && aimTarget != null)
            {
                weaponSystem.UseWeapon(aimTarget.position); // Skicka med målpositionen!
            }
        }
    }

    private void RotateWeaponTowardsTarget()
    {
        if (weaponTransform != null && aimTarget != null)
        {
            // Beräkna riktningen från vapnet mot målpunkten
            Vector3 targetDirection = aimTarget.position - weaponTransform.position;

            // Skapa en rotation mot målet med offset
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

        // Räkna ut mitten av skärmen (där crosshair är)
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Skapa en ray från kameran genom crosshair
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        // Beräkna maximal räckvidd baserat på vapentyp
        float maxRange = 100f;
        if (weaponSystem != null)
            maxRange = weaponSystem.range;

        // Skjut iväg rayen och se var den träffar
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetLayers))
        {
            // Uppdatera aimTarget positionen
            aimTarget.position = hit.point;

            // Uppdatera crosshair om en fiende träffades
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
            // Om rayen inte träffade något, sätt ett mål långt bort i rayens riktning
            aimTarget.position = ray.origin + ray.direction * maxRange;
        }

        // Rita en Debug-linje för att visualisera siktet
        Debug.DrawLine(ray.origin, aimTarget.position, Color.red);
    }

    // Hjälpmetod för att kontrollera om det är ett avståndsvapen
    private bool IsRangedWeapon(WeaponType type)
    {
        return type == WeaponType.Bow ||
               type == WeaponType.Crossbow ||
               type == WeaponType.Rifle ||
               type == WeaponType.Shotgun ||
               type == WeaponType.Pistol;
    }
}