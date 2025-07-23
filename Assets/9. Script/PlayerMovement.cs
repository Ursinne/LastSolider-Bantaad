using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Kamerakomponenter")]
    public CinemachineCamera thirdPersonAimCamera;
    public Transform cameraLookTarget;

    [Header("R�relsetangenter")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode aimKey = KeyCode.Mouse1;
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("R�relseinst�llningar")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 20f;
    public float lookSpeed = 2f;              // Horisontell musr�relsek�nslighet
    public float lookSpeedY = 10f;            // Vertikal musr�relsek�nslighet
    public float maxLookAngle = 90f;          // Maximal vertikal kameravinkel

    [Header("Skjutinst�llningar")]
    public Transform bulletPrefab;
    public Transform bulletSpawnPoint;
    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public float weaponDamage = 10f;
    public Transform aimTarget;

    // Privata komponenter och variabler
    private CharacterController characterController;
    private Animator animator;
    private Camera mainCameraRef;
    private Vector3 moveDirection = Vector3.zero;
    private bool canMove = true;
    private float verticalVelocity = 0f;
    private RaycastHit hitInfo;
    private float verticalRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeComponents();
        ConfigureCursor();
        SetupAimTarget();
        ConfigureCinemachineCamera();
        SetupCameraLookTarget();

        EnableCinemachineComposer();
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float manualX = Input.GetAxis("Mouse X");
        float manualY = Input.GetAxis("Mouse Y");

        Debug.Log($"GetAxisRaw - X: {mouseX}, Y: {mouseY}");
        Debug.Log($"GetAxis - X: {manualX}, Y: {manualY}");

        HandleGroundedState();
        HandleMovement();
        HandleCameraRotation();
        HandleAimingAndShooting();
    }

    private void LateUpdate()
    {
        if (Input.GetKey(aimKey))
        {
            RotatePlayerTowardsCamera();
        }
    }

    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        mainCameraRef = Camera.main;

        if (characterController == null)
            Debug.LogError("CharacterController saknas!");

        if (animator == null)
            Debug.LogError("Animator saknas p� child-objektet!");

        if (mainCameraRef == null)
            Debug.LogError("Huvudkamera hittades inte!");
    }

    private void ConfigureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupAimTarget()
    {
        if (aimTarget == null)
        {
            GameObject aimTargetObj = new GameObject("AimTarget");
            aimTarget = aimTargetObj.transform;
            aimTarget.parent = transform;
            aimTarget.localPosition = Vector3.forward * 5f;
        }
    }

    private void ConfigureCinemachineCamera()
    {
        var rotComp = thirdPersonAimCamera?.GetComponent<CinemachineRotationComposer>();
        if (rotComp != null) rotComp.enabled = false;
    }

    private void SetupCameraLookTarget()
    {
        if (cameraLookTarget == null)
        {
            GameObject lookObj = new GameObject("CameraLookTarget");
            cameraLookTarget = lookObj.transform;
            cameraLookTarget.parent = transform;
            cameraLookTarget.localPosition = new Vector3(0, 1.5f, 0); // Justera h�jden efter behov
        }

        var cam = thirdPersonAimCamera?.GetComponent<CinemachineCamera>();
        if (cam != null)
        {
            cam.LookAt = cameraLookTarget;
        }
    }

    private void HandleGroundedState()
    {
        bool isGrounded = characterController.isGrounded;
        //animator.SetBool("isGrounded", isGrounded);
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float inputHorizontal = 0;
        float inputVertical = 0;

        if (canMove)
        {
            if (Input.GetKey(forwardKey)) inputVertical += 1.0f;
            if (Input.GetKey(backwardKey)) inputVertical -= 1.0f;
            if (Input.GetKey(rightKey)) inputHorizontal += 1.0f;
            if (Input.GetKey(leftKey)) inputHorizontal -= 1.0f;
        }

        animator.SetFloat("InputHorizontal", inputHorizontal);
        animator.SetFloat("InputVertical", inputVertical);

        float inputMagnitude = new Vector2(inputHorizontal, inputVertical).sqrMagnitude;
        animator.SetFloat("InputMagnitude", inputMagnitude);

        bool isSprinting = Input.GetKey(sprintKey) && inputVertical > 0;
        //animator.SetBool("isSprinting", isSprinting);

        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        Vector3 horizontalMove = (forward * inputVertical * currentSpeed) + (right * inputHorizontal * currentSpeed);

        bool isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            verticalVelocity = -0.5f;
            if (Input.GetKey(jumpKey) && canMove)
            {
                verticalVelocity = jumpPower;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        moveDirection = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);

        characterController.Move(moveDirection * Time.deltaTime);
    }

    public Camera aimCamera; // Dra in den nya kameran h�r i inspektorn

    private void HandleCameraRotation()
    {
        if (canMove && aimCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

            // Inverterar rotationen genom att subtrahera ist�llet f�r att addera
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            Debug.Log($"Applying Vertical Rotation: {verticalRotation}");

            aimCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void EnableCinemachineComposer()
    {
        var composer = thirdPersonAimCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineComposer;
        if (composer != null)
        {
            composer.enabled = true;
        }
    }

    private void UpdateAiming()
    {
        Transform targetObj = transform.Find("Lock Target");

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out hitInfo, 1000f))
        {
            aimTarget.position = hitInfo.point;
            Debug.DrawLine(bulletSpawnPoint.position, hitInfo.point, Color.red, 0.1f);
        }
        else
        {
            Vector3 targetPoint = ray.origin + ray.direction * 100f;
            aimTarget.position = targetPoint;
            Debug.DrawRay(bulletSpawnPoint.position, ray.direction * 100f, Color.blue, 0.1f);
        }
    }

    private void HandleAimingAndShooting()
    {
        bool isAiming = Input.GetKey(aimKey);
        animator.SetBool("isAiming", isAiming);

        if (isAiming)
        {
            UpdateAiming();
            if (Input.GetKeyDown(shootKey))
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null)
        {
            Debug.LogError("bulletPrefab eller bulletSpawnPoint �r inte tilldelade!");
            return;
        }

        Transform bulletTransform = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        PlayerBullet bulletScript = bulletTransform.GetComponent<PlayerBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetupTargetPoint(aimTarget.position);
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (hitInfo.collider != null && hitInfo.collider.tag != "NotShootable")
        {
            if (hitEffect != null)
            {
                hitEffect.transform.position = hitInfo.point;
                hitEffect.transform.forward = hitInfo.normal;
                hitEffect.Emit(1);
            }
        }

        animator.SetBool("isShooting", true);

        Invoke("ResetShootingAnimation", 0.2f);
    }

    private void ResetShootingAnimation()
    {
        animator.SetBool("isShooting", false);
    }

    private void RotatePlayerTowardsCamera()
    {
        Vector3 cameraForward = mainCameraRef.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(cameraForward),
            Time.deltaTime * 10f);
    }
}

//using UnityEngine;
//using Unity.Cinemachine;

//[RequireComponent(typeof(CharacterController))]
//public class PlayerMovement : MonoBehaviour
//{
//    [Header("Kamerakomponenter")]
//    public CinemachineCamera thirdPersonAimCamera;
//    public Transform cameraLookTarget;

//    [Header("R�relsetangenter")]
//    public KeyCode forwardKey = KeyCode.W;
//    public KeyCode backwardKey = KeyCode.S;
//    public KeyCode leftKey = KeyCode.A;
//    public KeyCode rightKey = KeyCode.D;
//    public KeyCode jumpKey = KeyCode.Space;
//    public KeyCode sprintKey = KeyCode.LeftShift;
//    public KeyCode crouchKey = KeyCode.LeftControl;
//    public KeyCode aimKey = KeyCode.Mouse1;
//    public KeyCode shootKey = KeyCode.Mouse0;

//    [Header("R�relseinst�llningar")]
//    public float walkSpeed = 6f;
//    public float runSpeed = 12f;
//    public float jumpPower = 7f;
//    public float gravity = 20f; 
//    public float lookSpeed = 2f;              // Horisontell musr�relsek�nslighet
//    public float lookSpeedY = 10f;            // Vertikal musr�relsek�nslighet
//    public float maxLookAngle = 90f;          // Maximal vertikal kameravinkel

//    [Header("Skjutinst�llningar")]
//    public Transform bulletPrefab;
//    public Transform bulletSpawnPoint;
//    public ParticleSystem muzzleFlash;
//    public ParticleSystem hitEffect;
//    public float weaponDamage = 10f;
//    public Transform aimTarget;

//    // Privata komponenter och variabler
//    private CharacterController characterController;
//    private Animator animator;
//    private Camera mainCameraRef;
//    private Vector3 moveDirection = Vector3.zero;
//    private bool canMove = true;
//    private float verticalVelocity = 0f;
//    private RaycastHit hitInfo;

//    // Ny variabel f�r kamerarotation
//    private float verticalRotation = 0f;

//    void Start()
//    {

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//        // Initiera komponenter
//        InitializeComponents();

//        // Konfigurera muspekare
//        ConfigureCursor();

//        // Skapa aim-m�l om det inte finns
//        SetupAimTarget();

//        // Konfigurera Cinemachine-kamera
//        ConfigureCinemachineCamera();

//        if (cameraLookTarget == null)
//        {
//            GameObject lookObj = new GameObject("CameraLookTarget");
//            cameraLookTarget = lookObj.transform;
//            cameraLookTarget.parent = transform;
//            cameraLookTarget.localPosition = new Vector3(0, 1.5f, 0); // Justera h�jden efter behov
//        }

//        // S�tt detta som Look Target f�r Cinemachine-kameran
//        var cam = thirdPersonAimCamera?.GetComponent<CinemachineCamera>();
//        if (cam != null)
//        {
//            cam.LookAt = cameraLookTarget;
//        }
//    }

//    void InitializeComponents()
//    {
//        characterController = GetComponent<CharacterController>();
//        animator = GetComponentInChildren<Animator>();
//        mainCameraRef = Camera.main;

//        if (characterController == null)
//            Debug.LogError("CharacterController saknas!");

//        if (animator == null)
//            Debug.LogError("Animator saknas p� child-objektet!");

//        if (mainCameraRef == null)
//            Debug.LogError("Huvudkamera hittades inte!");
//    }

//    void ConfigureCursor()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    void SetupAimTarget()
//    {
//        if (aimTarget == null)
//        {
//            GameObject aimTargetObj = new GameObject("AimTarget");
//            aimTarget = aimTargetObj.transform;
//            aimTarget.parent = transform;
//            aimTarget.localPosition = Vector3.forward * 5f;
//        }
//    }

//    void ConfigureCinemachineCamera()
//    {
//        var rotComp = thirdPersonAimCamera?.GetComponent<CinemachineRotationComposer>();
//        if (rotComp != null) rotComp.enabled = false;
//    }

//    void Update()
//    {

//        // Alternativ metod f�r att h�mta musen
//        float mouseX = Input.GetAxisRaw("Mouse X");
//        float mouseY = Input.GetAxisRaw("Mouse Y");

//        Debug.Log($"GetAxisRaw - X: {mouseX}, Y: {mouseY}");

//        // Om ovanst�ende inte fungerar, testa helt manuell input
//        float manualX = Input.GetAxis("Mouse X");
//        float manualY = Input.GetAxis("Mouse Y");

//        Debug.Log($"GetAxis - X: {manualX}, Y: {manualY}");
//        HandleGroundedState();
//        HandleMovement();
//        HandleCameraRotation();
//        HandleAimingAndShooting();


//    }

//    void HandleGroundedState()
//    {
//        bool isGrounded = characterController.isGrounded;
//        //animator.SetBool("isGrounded", isGrounded);
//    }

//    void HandleMovement()
//    {
//        Vector3 forward = transform.TransformDirection(Vector3.forward);
//        Vector3 right = transform.TransformDirection(Vector3.right);

//        float inputHorizontal = 0;
//        float inputVertical = 0;

//        if (canMove)
//        {
//            if (Input.GetKey(forwardKey)) inputVertical += 1.0f;
//            if (Input.GetKey(backwardKey)) inputVertical -= 1.0f;
//            if (Input.GetKey(rightKey)) inputHorizontal += 1.0f;
//            if (Input.GetKey(leftKey)) inputHorizontal -= 1.0f;
//        }

//        // Uppdatera animator-parametrar
//        animator.SetFloat("InputHorizontal", inputHorizontal);
//        animator.SetFloat("InputVertical", inputVertical);

//        float inputMagnitude = new Vector2(inputHorizontal, inputVertical).sqrMagnitude;
//        animator.SetFloat("InputMagnitude", inputMagnitude);

//        // Hantera sprint
//        bool isSprinting = Input.GetKey(sprintKey) && inputVertical > 0;
//        //animator.SetBool("isSprinting", isSprinting);

//        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

//        // Ber�kna horisontell r�relseriktning
//        Vector3 horizontalMove = (forward * inputVertical * currentSpeed) + (right * inputHorizontal * currentSpeed);

//        // Hantera vertikal r�relse (fall/hopp)
//        bool isGrounded = characterController.isGrounded;
//        if (isGrounded)
//        {
//            verticalVelocity = -0.5f;
//            if (Input.GetKey(jumpKey) && canMove)
//            {
//                verticalVelocity = jumpPower;
//            }
//        }
//        else
//        {
//            verticalVelocity -= gravity * Time.deltaTime;
//        }

//        // Kombinera horisontell och vertikal r�relse
//        moveDirection = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);

//        // Applicera r�relse
//        characterController.Move(moveDirection * Time.deltaTime);
//    }

//    void HandleCameraRotation()
//    {
//        if (canMove && thirdPersonAimCamera != null)
//        {
//            // Horisontell rotation (spelaren)
//            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
//            transform.Rotate(Vector3.up * mouseX);

//            // Vertikal rotation (kamera)
//            float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

//            // Inverterad rotation f�r naturlig k�nsla
//            verticalRotation += mouseY;
//            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

//            Debug.Log($"Applying Vertical Rotation: {verticalRotation}");

//            // Rotera Cinemachine-kameran direkt
//            thirdPersonAimCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
//        }
//    }

//    void UpdateAiming()
//    {
//        // Ist�llet f�r att g�ra raycast fr�n kameran, 
//        // anv�nd kamerans riktning och placera AimTarget i den riktningen

//        // Samma m�l som kameran tittar p�
//        Transform targetObj = transform.Find("Lock Target"); // Eller din direkta referens

//        // Skapa en ray fr�n kameran i fram�triktningen
//        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
//        Ray ray = mainCameraRef.ScreenPointToRay(screenCenter);

//        // Utf�r raycast f�r att se vad siktet tr�ffar
//        if (Physics.Raycast(ray, out hitInfo, 1000f))
//        {
//            // Uppdatera siktet till tr�ffpunkten
//            aimTarget.position = hitInfo.point;

//            // Debug-rita en linje 
//            Debug.DrawLine(bulletSpawnPoint.position, hitInfo.point, Color.red, 0.1f);
//        }
//        else
//        {
//            // Om inget tr�ffas, placera siktet l�ngre fram
//            Vector3 targetPoint = ray.origin + ray.direction * 100f;
//            aimTarget.position = targetPoint;

//            // Debug-rita en linje
//            Debug.DrawRay(bulletSpawnPoint.position, ray.direction * 100f, Color.blue, 0.1f);
//        }
//    }

//    void HandleAimingAndShooting()
//    {
//        bool isAiming = Input.GetKey(aimKey);
//        animator.SetBool("isAiming", isAiming);

//        if (isAiming)
//        {
//            UpdateAiming();
//            if (Input.GetKeyDown(shootKey))
//            {
//                Shoot();
//            }
//        }
//    }

//    //void UpdateAiming()
//    //{
//    //    // Skapa en ray fr�n kameran mot mitten av sk�rmen (d�r crosshair �r)
//    //    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
//    //    Ray ray = mainCameraRef.ScreenPointToRay(screenCenter);

//    //    // Utf�r raycast f�r att hitta vad vi siktar p�
//    //    if (Physics.Raycast(ray, out hitInfo, 1000f))
//    //    {
//    //        // Placera aimTarget p� tr�ffpunkten
//    //        aimTarget.position = hitInfo.point;

//    //        // Debug-rita en linje f�r att visa siktlinjen
//    //        Debug.DrawLine(bulletSpawnPoint.position, hitInfo.point, Color.red, 0.1f);
//    //    }
//    //    else
//    //    {
//    //        // Om vi inte tr�ffar n�got, placera aimTarget l�ngt fram
//    //        Vector3 targetPoint = ray.origin + ray.direction * 100f;
//    //        aimTarget.position = targetPoint;

//    //        // Debug-rita en linje f�r att visa siktlinjen
//    //        Debug.DrawRay(bulletSpawnPoint.position, ray.direction * 100f, Color.blue, 0.1f);
//    //    }
//    //}

//    void Shoot()
//    {
//        if (bulletPrefab == null || bulletSpawnPoint == null)
//        {
//            Debug.LogError("bulletPrefab eller bulletSpawnPoint �r inte tilldelade!");
//            return;
//        }

//        // Instantiera ett skott vid bulletSpawnPoint
//        Transform bulletTransform = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

//        // H�mta PlayerBullet-komponenten och anropa SetupTargetPoint
//        PlayerBullet bulletScript = bulletTransform.GetComponent<PlayerBullet>();
//        if (bulletScript != null)
//        {
//            bulletScript.SetupTargetPoint(aimTarget.position);
//        }

//        // Aktivera muzzleFlash om den finns
//        if (muzzleFlash != null)
//        {
//            muzzleFlash.Play();
//        }

//        // Skapa hitEffect om raycast tr�ffar n�got
//        if (hitInfo.collider != null && hitInfo.collider.tag != "NotShootable")
//        {
//            if (hitEffect != null)
//            {
//                hitEffect.transform.position = hitInfo.point;
//                hitEffect.transform.forward = hitInfo.normal;
//                hitEffect.Emit(1);
//            }
//        }

//        // Uppdatera animator f�r skjutning om det beh�vs
//        animator.SetBool("isShooting", true);

//        // �terst�ll skjutanimation efter en kort tid
//        Invoke("ResetShootingAnimation", 0.2f);
//    }

//    void ResetShootingAnimation()
//    {
//        animator.SetBool("isShooting", false);
//    }

//    private void LateUpdate()
//    {
//        // Om spelaren siktar
//        if (Input.GetKey(aimKey))
//        {
//            // H�mta kamerans fram�triktning (utan y-komponent f�r att beh�lla spelaren uppr�tt)
//            Vector3 cameraForward = mainCameraRef.transform.forward;
//            cameraForward.y = 0;
//            cameraForward.Normalize();

//            // Rotera spelaren f�r att m�ta siktriktningen
//            transform.rotation = Quaternion.Lerp(
//                transform.rotation,
//                Quaternion.LookRotation(cameraForward),
//                Time.deltaTime * 10f);
//        }
//    }
//}