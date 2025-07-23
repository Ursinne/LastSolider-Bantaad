using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    public CinemachineCamera cinemachineCamera;
    public float mouseSensitivity = 5f; // Ökad känslighet
    public float maxLookAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        // Lås muspekaren
        Cursor.lockState = CursorLockMode.Locked;

        // Säkerställ att kameran är rätt refererad
        if (cinemachineCamera == null)
        {
            Debug.LogError("Cinemachine Camera är inte tilldelad!");
        }
    }

    void Update()
    {
        // Hämta musrörelse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Debug-information
        Debug.Log($"Mouse Input - X: {mouseX}, Y: {mouseY}");

        // Rotera horisontellt (hela spelaren)
        transform.Rotate(Vector3.up * mouseX);

        // Rotera vertikalt (bara kameran)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (cinemachineCamera != null)
        {
            // Skapa en lokal rotation för kameran
            Quaternion targetRotation = Quaternion.Euler(xRotation, 0, 0);
            cinemachineCamera.transform.localRotation = targetRotation;

            // Extra debug för kamerarotation
            Debug.Log($"Camera Local Rotation: {xRotation}");
        }
        else
        {
            Debug.LogWarning("Cinemachine Camera saknas fortfarande!");
        }
    }
}