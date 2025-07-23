using Invector;
using UnityEngine;

public class vThirdPersonCamera : MonoBehaviour
{
    #region inspector properties    

    [Header("Kamera Mål och Inställningar")]
    public Transform target; // Vilket objekt kameran ska följa (HERO)

    [Header("Kamera Beteende")]
    [Tooltip("Hur snabbt kameran roterar mellan olika lägen")]
    public float smoothCameraRotation = 12f;
    [Tooltip("Vilka lager som kan blockera kameravyn")]
    public LayerMask cullingLayer = 1 << 0;
    [Tooltip("Låser kameran bakom karaktären för debugging")]
    public bool lockCamera;

    [Header("First/Third Person Inställningar")]
    public bool isFirstPerson = false; // Om kameran är i first person läge
    public KeyCode toggleViewKey = KeyCode.V; // Knapp för att växla vy
    public float firstPersonHeight = 1.6f; // Höjd för first person kamera
    public float transitionSpeed = 5f; // Hastighet för övergång mellan lägen

    // Sparar original-värden för att kunna växla tillbaka
    private float originalDefaultDistance;
    private float originalHeight;

    [Header("Kamera Position")]
    public float rightOffset = 0f; // Sidoförskjutning från karaktären
    public float defaultDistance = 2.5f; // Standard avstånd i third person
    public float height = 1.4f; // Kamerahöjd
    public float smoothFollow = 10f; // Hur mjukt kameran följer

    [Header("Mus Känslighet")]
    public float xMouseSensitivity = 3f; // Horisontal muskänslighet
    public float yMouseSensitivity = 3f; // Vertikal muskänslighet
    public float yMinLimit = -40f; // Minsta vertikala vinkel
    public float yMaxLimit = 80f; // Största vertikala vinkel

    #endregion

    #region hide properties - Interna variabler som inte visas i Inspector

    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot; // Extra offset för spelarens pivot
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget; // Nuvarande mål som kameran följer
    [HideInInspector]
    public Vector2 movementSpeed; // Aktuell rörelsehastighet för musen

    // Privata variabler för kamerahantering
    private Transform targetLookAt; // Osynligt objekt som kameran tittar på
    private Vector3 currentTargetPos; // Nuvarande position av målet
    private Vector3 lookPoint; // Punkt som kameran tittar mot
    private Vector3 current_cPos; // Nuvarande kameraposition
    private Vector3 desired_cPos; // Önskad kameraposition
    private Camera _camera; // Referens till Camera-komponenten
    private float distance = 5f; // Aktuellt avstånd till målet
    private float mouseY = 0f; // Vertikal musrotation
    private float mouseX = 0f; // Horisontal musrotation
    private float currentHeight; // Aktuell kamerahöjd
    private float cullingDistance; // Avstånd för kollisionsdetektering

    // Inställningar för kollisionsdetektering
    private float checkHeightRadius = 0.4f; // Radie för höjdkontroll
    private float clipPlaneMargin = 0f; // Marginal för klippning
    private float forward = -1f; // Riktning framåt (negativ = bakom karaktären)
    private float xMinLimit = -360f; // Min horisontal rotation
    private float xMaxLimit = 360f; // Max horisontal rotation
    private float cullingHeight = 0.2f; // Höjd för kollisionskontroll
    private float cullingMinDist = 0.1f; // Minsta avstånd vid kollision

    #endregion

    void Start()
    {
        Init(); // Initialisera kameran när spelet startar
    }

    public void Init()
    {
        // Spara original-värden så vi kan växla tillbaka från first person
        originalDefaultDistance = defaultDistance;
        originalHeight = height;

        // Börja alltid i third person läge
        isFirstPerson = false;

        if (target == null)
            return;

        // Hämta Camera-komponenten
        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

        // Skapa osynligt objekt som kameran kan rotera runt
        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy; // Dölj i hierarkin
        targetLookAt.rotation = currentTarget.rotation;

        // Sätt initial rotation baserat på karaktärens rotation
        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        // Sätt initial avstånd och höjd
        distance = defaultDistance;
        currentHeight = height;
    }

    void Update()
    {
        //// Kolla om spelaren trycker på V för att växla kameraläge
        //if (Input.GetKeyDown(toggleViewKey))
        //{
        //    ToggleCameraMode();
        //}
    }

    void FixedUpdate()
    {
        // Uppdatera kameraposition varje physics frame
        if (target == null || targetLookAt == null) return;
        CameraMovement();
    }

    public void ToggleCameraMode()
    {
        isFirstPerson = !isFirstPerson;
        Debug.Log($"Toggle till: {(isFirstPerson ? "First Person" : "Third Person")}");

        if (isFirstPerson)
        {
            // ADS/FIRST PERSON - som PUBG när man siktar
            defaultDistance = 0.1f;  // Mycket nära för ADS-känsla
            height = firstPersonHeight; // 1.6 (ögonhöjd)
            rightOffset = 0.02f; // Minimal offset för ADS

            xMouseSensitivity = 1.5f; // Lägre känslighet för precision
            yMouseSensitivity = 1.5f;

            // Dölj huvudet i first person
            HidePlayerHead(true);
        }
        else
        {
            // THIRD PERSON - MYCKET mer över axel för tydlig skillnad
            defaultDistance = 1.5f;      // Ännu närmare
            height = 1.7f;               // Högre upp
            rightOffset = 1.2f;          // MYCKET mer åt höger

            xMouseSensitivity = 3f;
            yMouseSensitivity = 3f;

            HidePlayerHead(false);
        }
    }

    // Ny metod för att dölja bara huvudet
    private void HidePlayerHead(bool hideHead)
    {
        if (currentTarget != null)
        {
            // Hitta head-meshen specifikt
            Transform head = FindChildByName(currentTarget, "Head08_Mesh");
            if (head != null)
            {
                Renderer headRenderer = head.GetComponent<Renderer>();
                if (headRenderer != null)
                {
                    headRenderer.enabled = !hideHead;
                }
            }
        }
    }

    // Hjälpmetod för att hitta barn med namn
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == name)
                return child;
        }
        return null;
    }
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    /// <summary>
    /// Sätt huvudmål för kameran (används vid initialisering)
    /// </summary>
    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
        currentTarget = newTarget;
        mouseY = currentTarget.rotation.eulerAngles.x;
        mouseX = currentTarget.rotation.eulerAngles.y;
        Init(); // Återinitialisera med nytt mål
    }

    /// <summary>
    /// Konvertera skärmposition till 3D-ray (används för aiming)
    /// </summary>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    /// <summary>
    /// Hantera kamerarotation baserat på musinput
    /// </summary>
    public void RotateCamera(float x, float y)
    {
        // Lägg till musinput till aktuell rotation
        mouseX += x * xMouseSensitivity;
        mouseY -= y * yMouseSensitivity;

        // Spara movement speed för animationer
        movementSpeed.x = x;
        movementSpeed.y = -y;

        if (!lockCamera)
        {
            // Begränsa vertikal rotation så kameran inte kan rotera helt runt
            mouseY = vExtensions.ClampAngle(mouseY, yMinLimit, yMaxLimit);
            mouseX = vExtensions.ClampAngle(mouseX, xMinLimit, xMaxLimit);
        }
        else
        {
            // Om kamera är låst, följ karaktärens rotation
            mouseY = currentTarget.root.localEulerAngles.x;
            mouseX = currentTarget.root.localEulerAngles.y;
        }
    }

    /// <summary>
    /// Huvudlogik för kamerarörelse och positionering
    /// </summary>
    void CameraMovement()
    {
        if (currentTarget == null)
            return;

        // VIKTIG: Beräkna målvståndet baserat på first/third person läge
        float targetDistance = isFirstPerson ? 0.05f : defaultDistance;
        distance = Mathf.Lerp(distance, targetDistance, smoothFollow * Time.deltaTime);

        // Mjuk övergång för kollisionsavstånd
        cullingDistance = Mathf.Lerp(cullingDistance, distance, Time.deltaTime);

        // Beräkna kamerariktning (bakom karaktären + sidoförskjutning)
        var camDir = (forward * targetLookAt.forward) + (rightOffset * targetLookAt.right);
        camDir = camDir.normalized;

        // Beräkna målposition och kamerapositioner
        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        currentTargetPos = targetPos;
        desired_cPos = targetPos + new Vector3(0, height, 0); // Önskad position
        current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0); // Aktuell position

        RaycastHit hitInfo;

        // Förbered kollisionskontroll med kamerans "near clip plane"
        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = _camera.NearClipPlanePoints(desired_cPos + (camDir * distance), clipPlaneMargin);

        // Kontrollera om höjden blockeras av något objekt (tak, etc.)
        if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer))
        {
            var t = hitInfo.distance - 0.2f;
            t -= height;
            t /= (cullingHeight - height);
            cullingHeight = Mathf.Lerp(height, cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
        }

        // Kontrollera om önskad kameraposition blockeras
        if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, distance + 0.2f, cullingLayer, Color.blue))
        {
            distance = hitInfo.distance - 0.2f; // Minska avstånd om något är i vägen
            if (distance < defaultDistance)
            {
                // Justera höjd om vi kommer för nära
                var t = hitInfo.distance;
                t -= cullingMinDist;
                t /= cullingMinDist;
                currentHeight = Mathf.Lerp(cullingHeight, height, Mathf.Clamp(t, 0.0f, 1.0f));
                current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
            }
        }
        else
        {
            currentHeight = height; // Återställ normal höjd om inget blockerar
        }

        // Slutlig kollisionskontroll med justerad höjd
        if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan))
            distance = Mathf.Clamp(cullingDistance, 0.0f, defaultDistance);

        // Beräkna var kameran ska titta
        var lookPoint = current_cPos + targetLookAt.forward * 2f;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));

        // Uppdatera lookAt-objektets position
        targetLookAt.position = current_cPos;

        // Skapa och applicera rotation
        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);

        // Sätt slutlig kameraposition och rotation
        transform.position = current_cPos + (camDir * (distance));
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);
        transform.rotation = rotation;

        // Återställ movement speed
        movementSpeed = Vector2.zero;
    }

    /// <summary>
    /// Anpassad raycast som använder kamerans near clip plane för kollisionskontroll
    /// Kontrollerar alla fyra hörn av kamerans synfält
    /// </summary>
    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;

        // Kontrollera nedre vänstra hörnet
        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            cullingDistance = hitInfo.distance;
        }

        // Kontrollera nedre högra hörnet
        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        // Kontrollera övre vänstra hörnet
        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        // Kontrollera övre högra hörnet
        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        // Returnera true om vi träffade något och hitInfo är giltigt
        return hitInfo.collider && value;
    }
}