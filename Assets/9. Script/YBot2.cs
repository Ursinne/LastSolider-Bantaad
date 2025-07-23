using UnityEngine;
using UnityEngine.AI;

public class YBot2 : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("NPC Settings")]
    public float wanderRadius = 10f;
    public float minWanderTime = 5f;
    public float maxWanderTime = 15f;

    [Header("Rörelse & Gravitation")]
    public float gravity = 20f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    // Interna variabler
    private Vector3 startPosition;
    private float wanderTimer;
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0f;
    private bool isGrounded = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Spara startpositionen
        startPosition = transform.position;

        // Sätt initial timer för första målet
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);

        // Inaktivera NavMeshAgent auto-uppdatering om vi använder CharacterController
        if (agent != null && controller != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = true;
        }
    }

    void Update()
    {
        // Räkna ner timer
        wanderTimer -= Time.deltaTime;

        // Om timern når noll, välj ett nytt mål
        if (wanderTimer <= 0)
        {
            ChooseNewDestination();
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }

        // Kontrollera om NPC står på marken
        CheckGrounded();

        // Flytta NPC
        HandleMovement();

        // Uppdatera animator
        UpdateAnimator();
    }

    void CheckGrounded()
    {
        // Använd en sfär under karaktären för att kontrollera markkontakt
        isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * controller.height / 2 * 0.9f,
            groundCheckDistance,
            groundLayer
        );

        // Visa debug-info om du behöver
        Debug.DrawRay(transform.position, Vector3.down * controller.height / 2 * 0.9f, isGrounded ? Color.green : Color.red);
    }

    void ChooseNewDestination()
    {
        if (agent != null)
        {
            // Beräkna en slumpmässig position inom wanderRadius
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0; // Håll på samma höjdnivå

            Vector3 finalPosition = startPosition + randomDirection;

            // Säkerställ att positionen är på NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(finalPosition, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                isMoving = true;

                // Visa debug-info
                Debug.DrawLine(transform.position, hit.position, Color.blue, 1.0f);
            }
        }
    }

    void HandleMovement()
    {
        if (agent == null || controller == null) return;

        // Beräkna förflyttning baserat på NavMeshAgent
        Vector3 desiredMove = agent.desiredVelocity;

        // Hantera gravitation
        if (isGrounded)
        {
            // Återställ vertikal hastighet när på marken
            verticalVelocity = -0.5f; // Håll kontakt med marken
        }
        else
        {
            // Tillämpa gravitation
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Skapa slutgiltig rörelsevektor
        moveDirection = desiredMove;
        moveDirection.y = verticalVelocity;

        // Flytta controllern
        controller.Move(moveDirection * Time.deltaTime);

        // Uppdatera agentens position för att matcha controllern
        agent.nextPosition = transform.position;

        // Kontrollera om NPC:n har nått sitt mål
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isMoving = false;
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // Använd agentens faktiska hastighet för animation
            float speed = agent.velocity.magnitude / agent.speed;

            animator.SetFloat("VelocityY", speed);
            animator.SetFloat("VelocityX", 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Rita groundCheckDistance för debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.down * controller.height / 2 * 0.9f,
            groundCheckDistance
        );

        // Visa wanderRadius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, wanderRadius);
    }
}