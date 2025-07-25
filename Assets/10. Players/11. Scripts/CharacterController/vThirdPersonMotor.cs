﻿using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonMotor : MonoBehaviour
    {
        #region Inspector Variables

        [Header("- Movement")]

        [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        public bool useRootMotion = false;
        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        public bool rotateByWorld = false;
        [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
        public bool useContinuousSprint = true;
        [Tooltip("Check this to sprint always in free movement")]
        public bool sprintOnlyFree = true;
        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree,
        }
        public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;

        public vMovementSpeed freeSpeed, strafeSpeed;

        [Header("- Airborne")]

        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        public bool jumpWithRigidbodyForce = false;
        [Tooltip("Rotate or not while airborne")]
        public bool jumpAndRotate = true;
        [Tooltip("How much time the character will be jumping")]
        public float jumpTimer = 0.3f;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        public float jumpHeight = 4f;

        [Tooltip("Speed that the character will move while airborne")]
        public float airSpeed = 5f;
        [Tooltip("Smoothness of the direction while airborne")]
        public float airSmooth = 6f;
        [Tooltip("Apply extra gravity when the character is not grounded")]
        public float extraGravity = -10f;
        [HideInInspector]
        public float limitFallVelocity = -15f;

        [Header("- Ground")]
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        public float groundMinDistance = 0.25f;
        public float groundMaxDistance = 0.5f;
        [Tooltip("Max angle to walk")]
        [Range(30, 80)] public float slopeLimit = 75f;
        #endregion

        #region Components

        internal Animator animator;
        internal Rigidbody _rigidbody;                                                      // access the Rigidbody component
        internal PhysicsMaterial frictionPhysics, maxFrictionPhysics, slippyPhysics;         // create PhysicMaterial for the Rigidbody
        internal CapsuleCollider _capsuleCollider;                                          // access CapsuleCollider information

        #endregion

        #region Internal Variables

        // movement bools
        internal bool isJumping;
        internal bool isStrafing
        {
            get
            {
                return _isStrafing;
            }
            set
            {
                _isStrafing = value;
            }
        }
        internal bool isGrounded { get; set; }
        internal bool isSprinting { get; set; }
        public bool stopMove { get; protected set; }

        internal float inputMagnitude;                      // sets the inputMagnitude to update the animations in the animator controller
        internal float verticalSpeed;                       // set the verticalSpeed based on the verticalInput
        internal float horizontalSpeed;                     // set the horizontalSpeed based on the horizontalInput       
        internal float moveSpeed;                           // set the current moveSpeed for the MoveCharacter method
        internal float verticalVelocity;                    // set the vertical velocity of the rigidbody
        internal float colliderRadius, colliderHeight;      // storage capsule collider extra information        
        internal float heightReached;                       // max height that character reached in air;
        internal float jumpCounter;                         // used to count the routine to reset the jump
        internal float groundDistance;                      // used to know the distance from the ground
        internal RaycastHit groundHit;                      // raycast to hit the ground 
        internal bool lockMovement = false;                 // lock the movement of the controller (not the animation)
        internal bool lockRotation = false;                 // lock the rotation of the controller (not the animation)        
        internal bool _isStrafing;                          // internally used to set the strafe movement                
        internal Transform rotateTarget;                    // used as a generic reference for the camera.transform
        internal Vector3 input;                             // generate raw input for the controller
        internal Vector3 colliderCenter;                    // storage the center of the capsule collider info                
        internal Vector3 inputSmooth;                       // generate smooth input based on the inputSmooth value       
        internal Vector3 moveDirection;                     // used to know the direction you're moving 

        #endregion

        public void Init()
        {
            // Dessa ska nu hitta komponenter på samma GameObject (Player)
            animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // slides the character through walls and edges
            frictionPhysics = new PhysicsMaterial();
            frictionPhysics.name = "frictionPhysics";
            frictionPhysics.staticFriction = .25f;
            frictionPhysics.dynamicFriction = .25f;
            frictionPhysics.frictionCombine = PhysicsMaterialCombine.Multiply;

            // prevents the collider from slipping on ramps
            maxFrictionPhysics = new PhysicsMaterial();
            maxFrictionPhysics.name = "maxFrictionPhysics";
            maxFrictionPhysics.staticFriction = 1f;
            maxFrictionPhysics.dynamicFriction = 1f;
            maxFrictionPhysics.frictionCombine = PhysicsMaterialCombine.Maximum;

            // air physics 
            slippyPhysics = new PhysicsMaterial();
            slippyPhysics.name = "slippyPhysics";
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;
            slippyPhysics.frictionCombine = PhysicsMaterialCombine.Minimum;

            // rigidbody info
            //_rigidbody = GetComponent<Rigidbody>();
            //_rigidbody = GetComponentInChildren<Rigidbody>();
            _rigidbody = GetComponent<Rigidbody>();

            // capsule collider info
            //_capsuleCollider = GetComponent<CapsuleCollider>();
            animator = GetComponentInChildren<Animator>();
            _capsuleCollider = GetComponentInChildren<CapsuleCollider>();

            // save your collider preferences 
            colliderCenter = _capsuleCollider.center;
            colliderRadius = _capsuleCollider.radius;
            colliderHeight = _capsuleCollider.height;

            isGrounded = true;
        }

        public virtual void UpdateMotor()
        {
            CheckGround();
            CheckSlopeLimit();
            ControlJumpBehaviour();
            AirControl();
        }

        #region Locomotion

        public virtual void SetControllerMoveSpeed(vMovementSpeed speed)
        {
            if (speed.walkByDefault)
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.deltaTime);
            else
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.deltaTime);
        }

        public virtual void MoveCharacter(Vector3 _direction)
        {
            // Safety check för NaN och Infinity värden
            if (float.IsNaN(_direction.x) || float.IsNaN(_direction.y) || float.IsNaN(_direction.z) ||
                float.IsInfinity(_direction.x) || float.IsInfinity(_direction.y) || float.IsInfinity(_direction.z))
            {
                Debug.LogWarning("Invalid direction detected, resetting to zero");
                _direction = Vector3.zero;
            }

            // Beräkna input smooth (med extra säkerhet)
            Vector3 safeInput = input;
            if (float.IsNaN(input.x) || float.IsNaN(input.z))
            {
                Debug.LogWarning("Invalid input detected, resetting to zero");
                safeInput = Vector3.zero;
            }

            inputSmooth = Vector3.Lerp(inputSmooth, safeInput, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

            // Kontrollera att vi är på marken och inte hoppar
            if (!isGrounded || isJumping) return;

            // Nollställ Y-komponenten (vi rör oss bara horisontellt)
            _direction.y = 0;

            // Begränsa input-värdena
            _direction.x = Mathf.Clamp(_direction.x, -1f, 1f);
            _direction.z = Mathf.Clamp(_direction.z, -1f, 1f);

            // Normalisera om magnitud är större än 1
            if (_direction.magnitude > 1f)
                _direction.Normalize();

            // Beräkna målposition
            Vector3 basePosition = useRootMotion ? animator.rootPosition : _rigidbody.position;
            Vector3 targetPosition = basePosition + _direction * (stopMove ? 0 : moveSpeed) * Time.deltaTime;

            // Säkerhetskontroll för målposition
            if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.z))
            {
                Debug.LogWarning("Invalid target position, skipping movement");
                return;
            }

            // Beräkna målhastighet
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            // Säkerhetskontroll för hastighet
            if (float.IsNaN(targetVelocity.x) || float.IsNaN(targetVelocity.z))
            {
                Debug.LogWarning("Invalid target velocity, skipping movement");
                return;
            }

            // Behåll Y-hastigheten för att inte störa hopp/fall
            targetVelocity.y = _rigidbody.linearVelocity.y;

            // Applicera den nya hastigheten
            _rigidbody.linearVelocity = targetVelocity;
        }

        // Förbättrad UpdateMoveDirection för att undvika NaN
        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            // Kontrollera input för NaN-värden
            if (float.IsNaN(input.x) || float.IsNaN(input.z))
            {
                Debug.LogWarning("NaN input detected, resetting to zero");
                input = Vector3.zero;
            }

            if (input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                // Hämta höger-riktning från referensobjektet
                var right = referenceTransform.right;
                right.y = 0;

                // Säkerhetskontroll för right-vektor
                if (float.IsNaN(right.x) || float.IsNaN(right.z))
                {
                    Debug.LogWarning("Invalid right vector from reference transform");
                    right = Vector3.right;
                }

                // Beräkna framåt-riktning
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;

                // Säkerhetskontroll för forward-vektor
                if (float.IsNaN(forward.x) || float.IsNaN(forward.z))
                {
                    Debug.LogWarning("Invalid forward vector calculated");
                    forward = Vector3.forward;
                }

                // Beräkna rörelseriktning
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }

            // Slutkontroll av moveDirection
            if (float.IsNaN(moveDirection.x) || float.IsNaN(moveDirection.z))
            {
                Debug.LogWarning("NaN moveDirection detected, resetting to zero");
                moveDirection = Vector3.zero;
            }
        }

        public virtual void CheckSlopeLimit()
        {
            if (input.sqrMagnitude < 0.1) return;

            RaycastHit hitinfo;
            var hitAngle = 0f;

            if (Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), transform.position + moveDirection.normalized * (_capsuleCollider.radius + 0.2f), out hitinfo, groundLayer))
            {
                hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                var targetPoint = hitinfo.point + moveDirection.normalized * _capsuleCollider.radius;
                if ((hitAngle > slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), targetPoint, out hitinfo, groundLayer))
                {
                    hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                    if (hitAngle > slopeLimit && hitAngle < 85f)
                    {
                        stopMove = true;
                        return;
                    }
                }
            }
            stopMove = false;
        }

        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public virtual void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
        }

        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (!jumpAndRotate && !isGrounded) return;
            direction.y = 0f;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed * Time.deltaTime, .1f);
            Quaternion _newRotation = Quaternion.LookRotation(desiredForward);
            transform.rotation = _newRotation;
        }

        #endregion

        #region Jump Methods

        protected virtual void ControlJumpBehaviour()
        {
            if (!isJumping) return;

            jumpCounter -= Time.deltaTime;
            if (jumpCounter <= 0)
            {
                jumpCounter = 0;
                isJumping = false;
            }
            // apply extra force to the jump height   
            var vel = _rigidbody.linearVelocity;
            vel.y = jumpHeight;
            _rigidbody.linearVelocity = vel;
        }

        public virtual void AirControl()
        {
            if ((isGrounded && !isJumping)) return;
            if (transform.position.y > heightReached) heightReached = transform.position.y;
            inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.deltaTime);

            if (jumpWithRigidbodyForce && !isGrounded)
            {
                _rigidbody.AddForce(moveDirection * airSpeed * Time.deltaTime, ForceMode.VelocityChange);
                return;
            }

            moveDirection.y = 0;
            moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
            moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);

            Vector3 targetPosition = _rigidbody.position + (moveDirection * airSpeed) * Time.deltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            targetVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, targetVelocity, airSmooth * Time.deltaTime);
        }

        protected virtual bool jumpFwdCondition
        {
            get
            {
                Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
                Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
            }
        }

        #endregion

        #region Ground Check                

        protected virtual void CheckGround()
        {
            CheckGroundDistance();
            ControlMaterialPhysics();

            if (groundDistance <= groundMinDistance)
            {
                isGrounded = true;
                if (!isJumping && groundDistance > 0.05f)
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);

                heightReached = transform.position.y;
            }
            else
            {
                if (groundDistance >= groundMaxDistance)
                {
                    // set IsGrounded to false 
                    isGrounded = false;
                    // check vertical velocity
                    verticalVelocity = _rigidbody.linearVelocity.y;
                    // apply extra gravity when falling
                    if (!isJumping)
                    {
                        _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                    }
                }
                else if (!isJumping)
                {
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
                }
            }
        }

        protected virtual void ControlMaterialPhysics()
        {
            // change the physics material to very slip when not grounded
            _capsuleCollider.material = (isGrounded && GroundAngle() <= slopeLimit + 1) ? frictionPhysics : slippyPhysics;

            if (isGrounded && input == Vector3.zero)
                _capsuleCollider.material = maxFrictionPhysics;
            else if (isGrounded && input != Vector3.zero)
                _capsuleCollider.material = frictionPhysics;
            else
                _capsuleCollider.material = slippyPhysics;
        }

        protected virtual void CheckGroundDistance()
        {
            if (_capsuleCollider != null)
            {
                // radius of the SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                var dist = 10f;
                // ray for RayCast
                Ray ray2 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
                // raycast for check the ground distance
                if (Physics.Raycast(ray2, out groundHit, (colliderHeight / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
                    dist = transform.position.y - groundHit.point.y;
                // sphere cast around the base of the capsule to check the ground distance
                if (dist >= groundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                    Ray ray = new Ray(pos, -Vector3.up);
                    if (Physics.SphereCast(ray, radius, out groundHit, _capsuleCollider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
                    {
                        Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                        float newDist = transform.position.y - groundHit.point.y;
                        if (dist > newDist) dist = newDist;
                    }
                }
                groundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }

        public virtual float GroundAngleFromDirection()
        {
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, groundHit.normal) - 90;
            return movementAngle;
        }

        #endregion

        [System.Serializable]
        public class vMovementSpeed
        {
            [Range(1f, 20f)]
            public float movementSmooth = 6f;
            [Range(0f, 1f)]
            public float animationSmooth = 0.2f;
            [Tooltip("Rotation speed of the character")]
            public float rotationSpeed = 16f;
            [Tooltip("Character will limit the movement to walk instead of running")]
            public bool walkByDefault = false;
            [Tooltip("Rotate with the Camera forward when standing idle")]
            public bool rotateWithCamera = false;
            [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
            public float walkSpeed = 2f;
            [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
            public float runningSpeed = 4f;
            [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
            public float sprintSpeed = 6f;
        }
    }
}