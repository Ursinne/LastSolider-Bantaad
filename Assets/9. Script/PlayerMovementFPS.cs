using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Components")]
    public Camera playerCamera;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement Keys")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode aimKey = KeyCode.Mouse1;
    public KeyCode shootKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode interactKey = KeyCode.E;

    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float aimingSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool canMove = true;

    void Start()
    {
        // H�mta n�dv�ndiga komponenter
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (characterController == null)
            Debug.LogError("CharacterController saknas!");

        if (animator == null)
            Debug.LogError("Animator saknas p� child-objektet!");

        // L�s muspekaren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool isGrounded = characterController.isGrounded;
        bool isAiming = Input.GetKey(aimKey);

        // Grundl�ggande r�relse
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Ber�kna hastighet baserat p� input och tillst�nd
        bool isSprinting = Input.GetKey(sprintKey);
        float curSpeedX = 0;
        float curSpeedY = 0;

        if (canMove)
        {
            float currentSpeed = isAiming ? aimingSpeed : (isSprinting ? runSpeed : walkSpeed);

            if (Input.GetKey(forwardKey)) curSpeedX += currentSpeed;
            if (Input.GetKey(backwardKey)) curSpeedX -= currentSpeed;
            if (Input.GetKey(rightKey)) curSpeedY += currentSpeed;
            if (Input.GetKey(leftKey)) curSpeedY -= currentSpeed;
        }

        // Ber�kna r�relseriktning
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Hantera hopp
        if (Input.GetKey(jumpKey) && canMove && isGrounded)
        {
            moveDirection.y = jumpPower;
            if (animator) animator.SetBool("isJumping", true);
        }
        else if (isGrounded)
        {
            moveDirection.y = 0;
            if (animator) animator.SetBool("isJumping", false);
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Hantera duckning
        if (Input.GetKey(crouchKey) && canMove)
        {
            characterController.height = crouchHeight;
            if (animator) animator.SetBool("isCrouching", true);
        }
        else
        {
            characterController.height = defaultHeight;
            if (animator) animator.SetBool("isCrouching", false);
        }

        // Uppdatera animationer
        if (animator)
        {
            bool isMoving = curSpeedX != 0 || curSpeedY != 0;
            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isRunning", isMoving && isSprinting);
            animator.SetBool("isAiming", isAiming);



        }

        // Applicera r�relse
        characterController.Move(moveDirection * Time.deltaTime);

        // Hantera kamerarotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    // Hj�lpmetod f�r att aktivera/deaktivera r�relse
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}