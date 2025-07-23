// ERSÄTT hela din vThirdPersonController.cs med denna version som forcerar rotation:

using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        [Header("PUBG-Style Movement")]
        public bool usePubgMovement = true;
        public float rotationSmoothness = 10f;

        [Header("Crosshair Alignment")]
        public bool alwaysLookAtCrosshair = true;
        public float crosshairRotationSpeed = 15f;
        public LayerMask crosshairLayers = -1;
        public bool showDebugLines = true;

        private Camera playerCamera;

        void Start()
        {
            playerCamera = Camera.main ?? FindObjectOfType<Camera>();
        }

        void Update()
        {
            if (playerCamera == null)
                playerCamera = Camera.main ?? FindObjectOfType<Camera>();
        }

        // FORCERA rotation i LateUpdate - efter alla andra system
        void LateUpdate()
        {
            if (usePubgMovement && alwaysLookAtCrosshair)
            {
                ForceRotationToCrosshair();
            }
        }

        // LÄGG TILL detta i din ForceRotationToCrosshair() metod:

        private void ForceRotationToCrosshair()
        {
            if (playerCamera == null) return;

            Vector3 targetDirection = GetCrosshairWorldDirection();

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // LOGGA FÖRE rotation
                Debug.Log($"FÖRE forcering: {transform.rotation.eulerAngles}");

                // OMEDELBAR ROTATION
                transform.rotation = targetRotation;

                // LOGGA EFTER rotation  
                Debug.Log($"EFTER forcering: {transform.rotation.eulerAngles}");
                Debug.Log($"MÅL rotation: {targetRotation.eulerAngles}");
            }
        }

        // LÄGG TILL detta i GetCrosshairWorldDirection() för att verifiera:

        private Vector3 GetCrosshairWorldDirection()
        {
            if (playerCamera == null) return Vector3.forward;

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray crosshairRay = playerCamera.ScreenPointToRay(screenCenter);

            Vector3 targetPoint;

            if (Physics.Raycast(crosshairRay, out RaycastHit hit, 100f, crosshairLayers))
            {
                targetPoint = hit.point;
                if (showDebugLines)
                    Debug.DrawLine(crosshairRay.origin, hit.point, Color.green);
            }
            else
            {
                targetPoint = crosshairRay.origin + crosshairRay.direction * 100f;
                if (showDebugLines)
                    Debug.DrawRay(crosshairRay.origin, crosshairRay.direction * 100f, Color.yellow);
            }

            Vector3 direction = targetPoint - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (showDebugLines)
            {
                // RÖD = Calculated direction (vart spelaren BORDE titta)
                Debug.DrawRay(transform.position, direction * 10f, Color.red);

                // **NY DEBUG**: BLÅ = Spelarens FAKTISKA forward (från transform)
                Debug.DrawRay(transform.position, transform.forward * 8f, Color.blue);

                // **NY DEBUG**: MAGENTA = Calculated vs Actual skillnad
                Vector3 actualForward = transform.forward;
                actualForward.y = 0;
                actualForward.Normalize();

                float angleDiff = Vector3.Angle(direction, actualForward);
                Debug.Log($"VINKEL-SKILLNAD: {angleDiff} grader mellan beräknad och faktisk riktning");

                if (angleDiff > 5f) // Om skillnaden är större än 5 grader
                {
                    Debug.LogWarning($"STOR SKILLNAD! Calculated: {direction}, Actual: {actualForward}");
                }
            }

            return direction;
        }

        // Resten av metoderna samma som innan
        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;

            if (inputSmooth == Vector3.zero)
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlLocomotionType()
        {
            if (lockMovement) return;

            if (usePubgMovement)
            {
                SetControllerMoveSpeed(freeSpeed);
                SetAnimatorMoveSpeed(freeSpeed);
            }
            else
            {
                if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
                {
                    SetControllerMoveSpeed(freeSpeed);
                    SetAnimatorMoveSpeed(freeSpeed);
                }
                else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
                {
                    isStrafing = true;
                    SetControllerMoveSpeed(strafeSpeed);
                    SetAnimatorMoveSpeed(strafeSpeed);
                }
            }

            if (!useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlRotationType()
        {
            if (lockRotation) return;

            // I PUBG-läge hanteras rotation i LateUpdate istället
            if (!usePubgMovement)
            {
                bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

                if (validInput)
                {
                    inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                    Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                    RotateToDirection(dir);
                }
            }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (usePubgMovement && playerCamera != null)
            {
                UpdatePubgMoveDirection();
            }
            else if (referenceTransform && !rotateByWorld)
            {
                var right = referenceTransform.right;
                right.y = 0;
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        private void UpdatePubgMoveDirection()
        {
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (inputSmooth.z * cameraForward) + (inputSmooth.x * cameraRight);
        }

        public virtual void Sprint(bool value)
        {
            var sprintConditions = (input.sqrMagnitude > 0.1f && isGrounded &&
                !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (input.sqrMagnitude > 0.1f)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                    }
                    else if (!isSprinting)
                    {
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting)
            {
                isSprinting = false;
            }
        }

        public virtual void Strafe()
        {
            if (!usePubgMovement)
            {
                isStrafing = !isStrafing;
            }
        }

        public virtual void Jump()
        {
            jumpCounter = jumpTimer;
            isJumping = true;

            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
        }
    }
}