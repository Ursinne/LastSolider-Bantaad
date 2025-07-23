using UnityEngine;

public class WeaponCrosshairAim : MonoBehaviour
{
    [Header("Sikto-inställningar")]
    public bool enableCrosshairAiming = true;  // Aktivera/inaktivera crosshair-sikte
    public float rotationSpeed = 10f;          // Hur snabbt vapnet roterar
    public Vector3 aimOffset = Vector3.zero;   // Extra offset för vapnets sikte

    [Header("Referenser")]
    public CrosshairManager crosshairManager;  // Referens till crosshair
    public Camera playerCamera;                // Spelarens kamera

    private void Start()
    {
        // Hitta crosshair manager automatiskt om den inte är tilldelad
        if (crosshairManager == null)
            crosshairManager = FindObjectOfType<CrosshairManager>();

        // Hitta kameran automatiskt
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!enableCrosshairAiming) return;

        // Rotera vapnet mot crosshair-målet
        RotateTowardsCrosshair();
    }

    private void RotateTowardsCrosshair()
    {
        Vector3 targetPosition = GetCrosshairWorldPosition();

        // Beräkna riktningen från vapnet till crosshair-målet
        Vector3 direction = targetPosition - transform.position;

        // Ta bort Y-komponenten om du bara vill rotera horisontellt
        // direction.y = 0;  // <-- Kommentera bort denna rad om du vill sikta upp/ner också

        // Kontrollera att riktningen inte är noll
        if (direction.magnitude > 0.1f)
        {
            // Skapa rotation mot målet
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(aimOffset);

            // Mjuk rotation mot målet
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetCrosshairWorldPosition()
    {
        // Försök använda CrosshairManager först
        if (crosshairManager != null)
        {
            return crosshairManager.GetAimWorldPosition();
        }

        // Fallback: Raycast från kameran genom mitten av skärmen
        if (playerCamera != null)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            // Skjut en ray och se vad den träffar
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return hit.point;
            }
            else
            {
                // Om den inte träffar något, använd en punkt långt bort
                return ray.origin + ray.direction * 100f;
            }
        }

        // Sista utväg: sikta framåt
        return transform.position + transform.forward * 10f;
    }

    // Metod för att visa debug-linjer i Scene view
    private void OnDrawGizmos()
    {
        if (!enableCrosshairAiming) return;

        Vector3 targetPos = GetCrosshairWorldPosition();

        // Rita en linje från vapnet till crosshair-målet
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPos);

        // Rita en kula vid crosshair-målet
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPos, 0.1f);
    }
}