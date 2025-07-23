using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float obstacleCheckDistance = 1.5f;
    public float directionChangeInterval = 3f;
    public LayerMask obstacleLayers;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float directionChangeTimer;
    private float verticalVelocity;
    private bool isRunning;
    private bool isControllerActive = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // Kontrollera att CharacterController �r aktiv
        if (controller != null && controller.enabled)
        {
            isControllerActive = true;
        }
        else if (controller != null && !controller.enabled)
        {
            Debug.LogWarning("CharacterController p� " + gameObject.name + " �r inaktiverad. Aktiverar den.");
            controller.enabled = true;
            isControllerActive = true;
        }
        else
        {
            Debug.LogError("Kunde inte hitta CharacterController p� " + gameObject.name);
        }

        ChooseNewDirection();
        directionChangeTimer = directionChangeInterval;
    }

    void Update()
    {
        // Verifiera att controllern fortfarande �r aktiv
        if (controller != null && !controller.enabled)
        {
            isControllerActive = false;
        }

        // Timer f�r att �ndra riktning regelbundet
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0)
        {
            ChooseNewDirection();
            directionChangeTimer = Random.Range(directionChangeInterval * 0.7f, directionChangeInterval * 1.3f);
        }

        // Kontrollera om det finns ett hinder framf�r
        if (IsObstacleAhead())
        {
            // Byt riktning direkt n�r ett hinder uppt�cks
            ChooseNewDirection();
            directionChangeTimer = directionChangeInterval;
        }

        // Hantera gravitation
        if (isControllerActive && controller.isGrounded)
        {
            verticalVelocity = -0.1f; // Liten ned�tkraft f�r att h�lla mot marken
        }
        else
        {
            verticalVelocity -= 9.8f * Time.deltaTime;
        }

        // Ber�kna r�relse
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 motion = moveDirection * speed;
        motion.y = verticalVelocity;

        // Till�mpa r�relse endast om controllern �r aktiv
        if (isControllerActive && controller != null)
        {
            try
            {
                controller.Move(motion * Time.deltaTime);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Fel vid CharacterController.Move p� " + gameObject.name + ": " + e.Message);
                isControllerActive = false;
            }
        }
        else
        {
            // Fallback n�r CharacterController inte �r tillg�nglig: anv�nd transform direkt
            transform.position += motion * Time.deltaTime;
        }

        // Uppdatera rotation
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Uppdatera animationer
        UpdateAnimations();
    }

    void ChooseNewDirection()
    {
        // 20% chans att st� stilla
        if (Random.value < 0.2f)
        {
            moveDirection = Vector3.zero;
            isRunning = false;
            return;
        }

        // Om det finns ett hinder direkt framf�r, undvik detta genom att ta en annan riktning
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, obstacleCheckDistance, obstacleLayers))
        {
            // Om vi tr�ffar ett hinder, ta en riktning bort fr�n hindret
            Vector3 avoidDirection = Vector3.Reflect(transform.forward, hit.normal);
            avoidDirection.y = 0;
            avoidDirection.Normalize();

            moveDirection = avoidDirection;
        }
        else
        {
            // V�lj slumpm�ssig riktning om det inte finns hinder
            float randomAngle = Random.Range(0, 360);
            moveDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        }

        // Slumpm�ssig chans att springa
        isRunning = Random.value < 0.3f;
    }

    bool IsObstacleAhead()
    {
        if (moveDirection == Vector3.zero)
            return false;

        // G�r tre raycast fram�t f�r att f� b�ttre t�ckning (mitten, h�ger, v�nster)
        Vector3 start = transform.position + Vector3.up * 0.5f;

        // Mittenstr�len
        if (Physics.Raycast(start, moveDirection, obstacleCheckDistance, obstacleLayers))
            return true;

        // H�ger str�le
        Vector3 rightDirection = Quaternion.Euler(0, 30, 0) * moveDirection;
        if (Physics.Raycast(start, rightDirection, obstacleCheckDistance, obstacleLayers))
            return true;

        // V�nster str�le
        Vector3 leftDirection = Quaternion.Euler(0, -30, 0) * moveDirection;
        if (Physics.Raycast(start, leftDirection, obstacleCheckDistance, obstacleLayers))
            return true;

        return false;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = moveDirection != Vector3.zero;
        animator.SetBool("isWalking", isMoving && !isRunning);
        animator.SetBool("isRunning", isMoving && isRunning);
    }

    // N�r vi kolliderar med n�got, byt riktning direkt
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Ignorera kollisioner med marken
        if (hit.normal.y > 0.5f)
            return;

        // Ber�kna en ny riktning bort fr�n kollisionspunkten
        Vector3 reflectedDirection = Vector3.Reflect(moveDirection, hit.normal);
        reflectedDirection.y = 0;
        reflectedDirection.Normalize();

        moveDirection = reflectedDirection;
        directionChangeTimer = directionChangeInterval;
    }

    // Aktivera/inaktivera NPC-r�relsen
    public void EnableMovement(bool enable)
    {
        if (controller != null)
        {
            controller.enabled = enable;
            isControllerActive = enable;
        }
    }

    // F�r att visualisera i editorn
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 start = transform.position + Vector3.up * 0.5f;

        if (Application.isPlaying)
        {
            // Visa raycast-riktningar
            Gizmos.DrawRay(start, moveDirection * obstacleCheckDistance);
            Gizmos.DrawRay(start, Quaternion.Euler(0, 30, 0) * moveDirection * obstacleCheckDistance);
            Gizmos.DrawRay(start, Quaternion.Euler(0, -30, 0) * moveDirection * obstacleCheckDistance);
        }
        else
        {
            // Visa fram�triktningen i editorn
            Gizmos.DrawRay(start, transform.forward * obstacleCheckDistance);
        }
    }
}