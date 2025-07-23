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

    [Header("R�relse & Gravitation")]
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

        // S�tt initial timer f�r f�rsta m�let
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);

        // Inaktivera NavMeshAgent auto-uppdatering om vi anv�nder CharacterController
        if (agent != null && controller != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = true;
        }
    }

    void Update()
    {
        // R�kna ner timer
        wanderTimer -= Time.deltaTime;

        // Om timern n�r noll, v�lj ett nytt m�l
        if (wanderTimer <= 0)
        {
            ChooseNewDestination();
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }

        // Kontrollera om NPC st�r p� marken
        CheckGrounded();

        // Flytta NPC
        HandleMovement();

        // Uppdatera animator
        UpdateAnimator();
    }

    void CheckGrounded()
    {
        // Anv�nd en sf�r under karakt�ren f�r att kontrollera markkontakt
        isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * controller.height / 2 * 0.9f,
            groundCheckDistance,
            groundLayer
        );

        // Visa debug-info om du beh�ver
        Debug.DrawRay(transform.position, Vector3.down * controller.height / 2 * 0.9f, isGrounded ? Color.green : Color.red);
    }

    void ChooseNewDestination()
    {
        if (agent != null)
        {
            // Ber�kna en slumpm�ssig position inom wanderRadius
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0; // H�ll p� samma h�jdniv�

            Vector3 finalPosition = startPosition + randomDirection;

            // S�kerst�ll att positionen �r p� NavMesh
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

        // Ber�kna f�rflyttning baserat p� NavMeshAgent
        Vector3 desiredMove = agent.desiredVelocity;

        // Hantera gravitation
        if (isGrounded)
        {
            // �terst�ll vertikal hastighet n�r p� marken
            verticalVelocity = -0.5f; // H�ll kontakt med marken
        }
        else
        {
            // Till�mpa gravitation
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Skapa slutgiltig r�relsevektor
        moveDirection = desiredMove;
        moveDirection.y = verticalVelocity;

        // Flytta controllern
        controller.Move(moveDirection * Time.deltaTime);

        // Uppdatera agentens position f�r att matcha controllern
        agent.nextPosition = transform.position;

        // Kontrollera om NPC:n har n�tt sitt m�l
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isMoving = false;
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // Anv�nd agentens faktiska hastighet f�r animation
            float speed = agent.velocity.magnitude / agent.speed;

            animator.SetFloat("VelocityY", speed);
            animator.SetFloat("VelocityX", 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Rita groundCheckDistance f�r debugging
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