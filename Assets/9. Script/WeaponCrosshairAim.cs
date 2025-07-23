using UnityEngine;

public class WeaponCrosshairAim : MonoBehaviour
{
    [Header("Sikto-inst�llningar")]
    public bool enableCrosshairAiming = true;  // Aktivera/inaktivera crosshair-sikte
    public float rotationSpeed = 10f;          // Hur snabbt vapnet roterar
    public Vector3 aimOffset = Vector3.zero;   // Extra offset f�r vapnets sikte

    [Header("Referenser")]
    public CrosshairManager crosshairManager;  // Referens till crosshair
    public Camera playerCamera;                // Spelarens kamera

    private void Start()
    {
        // Hitta crosshair manager automatiskt om den inte �r tilldelad
        if (crosshairManager == null)
            crosshairManager = FindObjectOfType<CrosshairManager>();

        // Hitta kameran automatiskt
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!enableCrosshairAiming) return;

        // Rotera vapnet mot crosshair-m�let
        RotateTowardsCrosshair();
    }

    private void RotateTowardsCrosshair()
    {
        Vector3 targetPosition = GetCrosshairWorldPosition();

        // Ber�kna riktningen fr�n vapnet till crosshair-m�let
        Vector3 direction = targetPosition - transform.position;

        // Ta bort Y-komponenten om du bara vill rotera horisontellt
        // direction.y = 0;  // <-- Kommentera bort denna rad om du vill sikta upp/ner ocks�

        // Kontrollera att riktningen inte �r noll
        if (direction.magnitude > 0.1f)
        {
            // Skapa rotation mot m�let
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(aimOffset);

            // Mjuk rotation mot m�let
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetCrosshairWorldPosition()
    {
        // F�rs�k anv�nda CrosshairManager f�rst
        if (crosshairManager != null)
        {
            return crosshairManager.GetAimWorldPosition();
        }

        // Fallback: Raycast fr�n kameran genom mitten av sk�rmen
        if (playerCamera != null)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            // Skjut en ray och se vad den tr�ffar
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return hit.point;
            }
            else
            {
                // Om den inte tr�ffar n�got, anv�nd en punkt l�ngt bort
                return ray.origin + ray.direction * 100f;
            }
        }

        // Sista utv�g: sikta fram�t
        return transform.position + transform.forward * 10f;
    }

    // Metod f�r att visa debug-linjer i Scene view
    private void OnDrawGizmos()
    {
        if (!enableCrosshairAiming) return;

        Vector3 targetPos = GetCrosshairWorldPosition();

        // Rita en linje fr�n vapnet till crosshair-m�let
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPos);

        // Rita en kula vid crosshair-m�let
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPos, 0.1f);
    }
}