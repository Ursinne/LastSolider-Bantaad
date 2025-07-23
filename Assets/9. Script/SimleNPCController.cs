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

        // Kontrollera att CharacterController är aktiv
        if (controller != null && controller.enabled)
        {
            isControllerActive = true;
        }
        else if (controller != null && !controller.enabled)
        {
            Debug.LogWarning("CharacterController på " + gameObject.name + " är inaktiverad. Aktiverar den.");
            controller.enabled = true;
            isControllerActive = true;
        }
        else
        {
            Debug.LogError("Kunde inte hitta CharacterController på " + gameObject.name);
        }

        ChooseNewDirection();
        directionChangeTimer = directionChangeInterval;
    }

    void Update()
    {
        // Verifiera att controllern fortfarande är aktiv
        if (controller != null && !controller.enabled)
        {
            isControllerActive = false;
        }

        // Timer för att ändra riktning regelbundet
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0)
        {
            ChooseNewDirection();
            directionChangeTimer = Random.Range(directionChangeInterval * 0.7f, directionChangeInterval * 1.3f);
        }

        // Kontrollera om det finns ett hinder framför
        if (IsObstacleAhead())
        {
            // Byt riktning direkt när ett hinder upptäcks
            ChooseNewDirection();
            directionChangeTimer = directionChangeInterval;
        }

        // Hantera gravitation
        if (isControllerActive && controller.isGrounded)
        {
            verticalVelocity = -0.1f; // Liten nedåtkraft för att hålla mot marken
        }
        else
        {
            verticalVelocity -= 9.8f * Time.deltaTime;
        }

        // Beräkna rörelse
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 motion = moveDirection * speed;
        motion.y = verticalVelocity;

        // Tillämpa rörelse endast om controllern är aktiv
        if (isControllerActive && controller != null)
        {
            try
            {
                controller.Move(motion * Time.deltaTime);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Fel vid CharacterController.Move på " + gameObject.name + ": " + e.Message);
                isControllerActive = false;
            }
        }
        else
        {
            // Fallback när CharacterController inte är tillgänglig: använd transform direkt
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
        // 20% chans att stå stilla
        if (Random.value < 0.2f)
        {
            moveDirection = Vector3.zero;
            isRunning = false;
            return;
        }

        // Om det finns ett hinder direkt framför, undvik detta genom att ta en annan riktning
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, obstacleCheckDistance, obstacleLayers))
        {
            // Om vi träffar ett hinder, ta en riktning bort från hindret
            Vector3 avoidDirection = Vector3.Reflect(transform.forward, hit.normal);
            avoidDirection.y = 0;
            avoidDirection.Normalize();

            moveDirection = avoidDirection;
        }
        else
        {
            // Välj slumpmässig riktning om det inte finns hinder
            float randomAngle = Random.Range(0, 360);
            moveDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        }

        // Slumpmässig chans att springa
        isRunning = Random.value < 0.3f;
    }

    bool IsObstacleAhead()
    {
        if (moveDirection == Vector3.zero)
            return false;

        // Gör tre raycast framåt för att få bättre täckning (mitten, höger, vänster)
        Vector3 start = transform.position + Vector3.up * 0.5f;

        // Mittenstrålen
        if (Physics.Raycast(start, moveDirection, obstacleCheckDistance, obstacleLayers))
            return true;

        // Höger stråle
        Vector3 rightDirection = Quaternion.Euler(0, 30, 0) * moveDirection;
        if (Physics.Raycast(start, rightDirection, obstacleCheckDistance, obstacleLayers))
            return true;

        // Vänster stråle
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

    // När vi kolliderar med något, byt riktning direkt
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Ignorera kollisioner med marken
        if (hit.normal.y > 0.5f)
            return;

        // Beräkna en ny riktning bort från kollisionspunkten
        Vector3 reflectedDirection = Vector3.Reflect(moveDirection, hit.normal);
        reflectedDirection.y = 0;
        reflectedDirection.Normalize();

        moveDirection = reflectedDirection;
        directionChangeTimer = directionChangeInterval;
    }

    // Aktivera/inaktivera NPC-rörelsen
    public void EnableMovement(bool enable)
    {
        if (controller != null)
        {
            controller.enabled = enable;
            isControllerActive = enable;
        }
    }

    // För att visualisera i editorn
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
            // Visa framåtriktningen i editorn
            Gizmos.DrawRay(start, transform.forward * obstacleCheckDistance);
        }
    }
}