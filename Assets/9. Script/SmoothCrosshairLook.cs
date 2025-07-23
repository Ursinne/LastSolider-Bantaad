using UnityEngine;
using Invector.vCharacterController;

/// <summary>
/// Mjuk rotation mot crosshair utan konflikter med Invector-systemet
/// </summary>
public class SmoothCrosshairLook : MonoBehaviour
{
    [Header("Inställningar")]
    [Tooltip("Aktivera bara när spelaren siktar")]
    public bool onlyWhenAiming = true;

    [Tooltip("Hur mjuk rotationen är (lägre = mjukare)")]
    [Range(0.1f, 10f)]
    public float smoothness = 2f;

    [Tooltip("Maximal distans för siktet")]
    public float maxDistance = 100f;

    [Header("Input")]
    [Tooltip("Knapp för att sikta (vanligtvis höger musknapp)")]
    public KeyCode aimKey = KeyCode.Mouse1;

    // Referenser
    private Camera playerCamera;
    private vThirdPersonController thirdPersonController;
    private bool wasAiming = false;

    void Start()
    {
        // Hitta referenser
        playerCamera = Camera.main;
        thirdPersonController = GetComponent<vThirdPersonController>();

        if (playerCamera == null)
        {
            Debug.LogError("Ingen huvudkamera hittades!");
            enabled = false;
        }

        if (thirdPersonController == null)
        {
            Debug.LogWarning("vThirdPersonController inte hittad - rotationen kanske inte fungerar optimalt");
        }
    }

    void LateUpdate()
    {
        // Bara köra detta när vi siktar (om onlyWhenAiming är true)
        bool isAiming = !onlyWhenAiming || Input.GetKey(aimKey);

        if (isAiming)
        {
            RotateTowardsCrosshair();
        }

        // Logga när vi börjar/slutar sikta
        if (isAiming != wasAiming)
        {
            Debug.Log(isAiming ? "Börjar sikta" : "Slutar sikta");
            wasAiming = isAiming;
        }
    }

    void RotateTowardsCrosshair()
    {
        if (playerCamera == null) return;

        // Skapa ray från mitten av skärmen
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);

        Vector3 targetPoint;

        // Försök träffa något
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Ingen träff - sikta långt bort
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        // Beräkna riktning (bara horisontellt)
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0; // Ta bort vertikal komponent

        // Kontrollera att vi har en giltig riktning
        if (direction.sqrMagnitude > 0.01f)
        {
            // Beräkna målrotation
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Applicera mjuk rotation
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                smoothness * Time.deltaTime
            );
        }

        // Debug - visa vart vi siktar
        Debug.DrawLine(transform.position + Vector3.up, targetPoint, Color.green, 0.1f);
    }

    /// <summary>
    /// Sätt om rotationen ska vara aktiv eller inte
    /// </summary>
    public void SetActive(bool active)
    {
        enabled = active;
    }

    /// <summary>
    /// Ändra mjukhet under körning
    /// </summary>
    public void SetSmoothness(float newSmoothness)
    {
        smoothness = Mathf.Clamp(newSmoothness, 0.1f, 10f);
    }
}