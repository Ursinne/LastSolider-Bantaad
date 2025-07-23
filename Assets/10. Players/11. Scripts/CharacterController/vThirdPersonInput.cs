using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       

        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera Input")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        [Header("Camera Mode")]
        public KeyCode toggleCameraMode = KeyCode.V;

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        public Transform Aimball;

        private CrosshairManager crosshairManager;

        #endregion

        protected virtual void Start()
        {
            InitilizeController();
            InitializeTpCamera();
            crosshairManager = FindObjectOfType<CrosshairManager>();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();
            cc.ControlLocomotionType();
            cc.ControlRotationType();
        }

        protected virtual void Update()
        {
            InputHandle();
            cc.UpdateAnimator();

            // TESTA: Tvinga spelaren att titta mot crosshair
            if (Input.GetKey(KeyCode.LeftControl)) // Håll Ctrl för test
            {
                RotatePlayerToCrosshair();
            }
        }

        private void RotatePlayerToCrosshair()
        {
            if (cameraMain == null) return;

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = cameraMain.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0; // Bara horisontell rotation

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                }
            }
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion();
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform); // Player (this) - RÄTT!
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
            Aim();
            CameraToggleInput();
        }

        protected virtual void CameraToggleInput()
        {
            if (Input.GetKeyDown(toggleCameraMode))
            {
                Debug.Log("V-knapp tryckt!");

                if (tpCamera != null)
                {
                    Debug.Log($"Kamera hittades: {tpCamera.name}, växlar läge...");
                    Debug.Log($"Före toggle - isFirstPerson: {tpCamera.isFirstPerson}");
                    tpCamera.ToggleCameraMode();
                    Debug.Log($"Efter toggle - isFirstPerson: {tpCamera.isFirstPerson}");
                }
                else
                {
                    Debug.Log("ERROR: tpCamera är null! Kollar om vi kan hitta den...");
                    vThirdPersonCamera foundCamera = FindObjectOfType<vThirdPersonCamera>();
                    if (foundCamera != null)
                    {
                        Debug.Log($"Hittade kamera i scenen: {foundCamera.name}");
                        tpCamera = foundCamera;
                    }
                    else
                    {
                        Debug.Log("Ingen vThirdPersonCamera hittades i scenen!");
                    }
                }
            }
        }

        private void Aim()
        {
            if (Aimball == null)
            {
                Debug.LogWarning("Aimball referens saknas!");
                return;
            }

            if (cameraMain == null)
            {
                cameraMain = Camera.main;
                if (cameraMain == null)
                {
                    Debug.LogError("Ingen huvudkamera hittades!");
                    return;
                }
            }

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = cameraMain.ScreenPointToRay(screenCenter);
            RaycastHit hit;
            float maxDistance = 1000f;
            LayerMask layerMask = Physics.DefaultRaycastLayers;

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
            {
                Aimball.position = hit.point;

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log($"Siktet träffade objekt: {hit.collider.gameObject.name} på avstånd {hit.distance}m");
                }
            }
            else
            {
                Aimball.position = ray.GetPoint(maxDistance);
            }

            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
        }

        public virtual void MoveInput()
        {
            cc.input.x = Input.GetAxis(horizontalInput);
            cc.input.z = Input.GetAxis(verticallInput);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);
        }

        protected virtual void StrafeInput()
        {
            if (Input.GetKeyDown(strafeInput))
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (Input.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        protected virtual void JumpInput()
        {
            if (Input.GetKeyDown(jumpInput) && JumpConditions())
                cc.Jump();
        }

        #endregion       
    }
}