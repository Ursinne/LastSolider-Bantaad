using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    public CinemachineCamera cinemachineCamera;
    public float mouseSensitivity = 5f; // �kad k�nslighet
    public float maxLookAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        // L�s muspekaren
        Cursor.lockState = CursorLockMode.Locked;

        // S�kerst�ll att kameran �r r�tt refererad
        if (cinemachineCamera == null)
        {
            Debug.LogError("Cinemachine Camera �r inte tilldelad!");
        }
    }

    void Update()
    {
        // H�mta musr�relse
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
            // Skapa en lokal rotation f�r kameran
            Quaternion targetRotation = Quaternion.Euler(xRotation, 0, 0);
            cinemachineCamera.transform.localRotation = targetRotation;

            // Extra debug f�r kamerarotation
            Debug.Log($"Camera Local Rotation: {xRotation}");
        }
        else
        {
            Debug.LogWarning("Cinemachine Camera saknas fortfarande!");
        }
    }
}