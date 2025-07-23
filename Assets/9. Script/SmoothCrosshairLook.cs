using UnityEngine;
using Invector.vCharacterController;

/// <summary>
/// Mjuk rotation mot crosshair utan konflikter med Invector-systemet
/// </summary>
public class SmoothCrosshairLook : MonoBehaviour
{
    [Header("Inst�llningar")]
    [Tooltip("Aktivera bara n�r spelaren siktar")]
    public bool onlyWhenAiming = true;

    [Tooltip("Hur mjuk rotationen �r (l�gre = mjukare)")]
    [Range(0.1f, 10f)]
    public float smoothness = 2f;

    [Tooltip("Maximal distans f�r siktet")]
    public float maxDistance = 100f;

    [Header("Input")]
    [Tooltip("Knapp f�r att sikta (vanligtvis h�ger musknapp)")]
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
        // Bara k�ra detta n�r vi siktar (om onlyWhenAiming �r true)
        bool isAiming = !onlyWhenAiming || Input.GetKey(aimKey);

        if (isAiming)
        {
            RotateTowardsCrosshair();
        }

        // Logga n�r vi b�rjar/slutar sikta
        if (isAiming != wasAiming)
        {
            Debug.Log(isAiming ? "B�rjar sikta" : "Slutar sikta");
            wasAiming = isAiming;
        }
    }

    void RotateTowardsCrosshair()
    {
        if (playerCamera == null) return;

        // Skapa ray fr�n mitten av sk�rmen
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);

        Vector3 targetPoint;

        // F�rs�k tr�ffa n�got
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Ingen tr�ff - sikta l�ngt bort
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        // Ber�kna riktning (bara horisontellt)
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0; // Ta bort vertikal komponent

        // Kontrollera att vi har en giltig riktning
        if (direction.sqrMagnitude > 0.01f)
        {
            // Ber�kna m�lrotation
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
    /// S�tt om rotationen ska vara aktiv eller inte
    /// </summary>
    public void SetActive(bool active)
    {
        enabled = active;
    }

    /// <summary>
    /// �ndra mjukhet under k�rning
    /// </summary>
    public void SetSmoothness(float newSmoothness)
    {
        smoothness = Mathf.Clamp(newSmoothness, 0.1f, 10f);
    }
}