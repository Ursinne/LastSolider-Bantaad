using UnityEngine;

public class FPSViewModel : MonoBehaviour
{
    [Header("Viewmodel Setup")]
    public GameObject handsPrefab;  // Prefab med händer/vapen
    public Transform viewmodelParent;  // Tom GameObject för viewmodel

    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float swaySpeed = 2f;
    public float swayClampX = 0.1f;
    public float swayClampY = 0.1f;

    [Header("Bob Settings")]
    public bool enableBob = true;
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    private Vector3 originalPosition;
    private float bobTimer;
    private Vector3 swayPos;
    private PlayerMovementFPS playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovementFPS>();

        // Hitta eller skapa viewmodel parent
        if (viewmodelParent == null)
        {
            Camera playerCam = GetComponentInChildren<Camera>();
            if (playerCam == null) playerCam = Camera.main;

            GameObject vmParent = new GameObject("ViewmodelParent");
            vmParent.transform.SetParent(playerCam.transform);
            vmParent.transform.localPosition = Vector3.zero;
            vmParent.transform.localRotation = Quaternion.identity;
            viewmodelParent = vmParent.transform;
        }

        // Spawna händer om prefab finns
        if (handsPrefab != null)
        {
            GameObject hands = Instantiate(handsPrefab, viewmodelParent);
            hands.transform.localPosition = Vector3.zero;
            hands.transform.localRotation = Quaternion.identity;

            // Sätt alla objekt i viewmodel till rätt layer
            SetLayerRecursively(hands, LayerMask.NameToLayer("Viewmodel"));
        }

        originalPosition = viewmodelParent.localPosition;

        // Konfigurera kameran för viewmodel
        SetupViewmodelCamera();
    }

    void Update()
    {
        HandleSway();
        HandleBob();

        // Applicera effekter
        Vector3 finalPosition = originalPosition + swayPos;
        viewmodelParent.localPosition = finalPosition;
    }

    void HandleSway()
    {
        // Mouse sway
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Beräkna sway
        swayPos.x = Mathf.Lerp(swayPos.x,
            Mathf.Clamp(-mouseX * swayAmount, -swayClampX, swayClampX),
            Time.deltaTime * swaySpeed);

        swayPos.y = Mathf.Lerp(swayPos.y,
            Mathf.Clamp(-mouseY * swayAmount, -swayClampY, swayClampY),
            Time.deltaTime * swaySpeed);
    }

    void HandleBob()
    {
        if (!enableBob) return;

        // Kontrollera om spelaren rör sig
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                       Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            swayPos.x += Mathf.Cos(bobTimer) * bobAmount;
            swayPos.y += Mathf.Sin(bobTimer * 2) * bobAmount;
        }
        else
        {
            bobTimer = 0;
        }
    }

    void SetupViewmodelCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Skapa viewmodel kamera
        GameObject vmCamObj = new GameObject("ViewmodelCamera");
        vmCamObj.transform.SetParent(mainCam.transform);
        vmCamObj.transform.localPosition = Vector3.zero;
        vmCamObj.transform.localRotation = Quaternion.identity;

        Camera vmCam = vmCamObj.AddComponent<Camera>();
        vmCam.cullingMask = 1 << LayerMask.NameToLayer("Viewmodel");
        vmCam.fieldOfView = mainCam.fieldOfView;
        vmCam.nearClipPlane = 0.01f;
        vmCam.farClipPlane = 1f;
        vmCam.depth = mainCam.depth + 1;
        vmCam.clearFlags = CameraClearFlags.Depth;

        // Exkludera viewmodel från main camera
        mainCam.cullingMask = ~(1 << LayerMask.NameToLayer("Viewmodel"));
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}