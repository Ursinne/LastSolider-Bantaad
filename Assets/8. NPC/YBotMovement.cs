using UnityEngine;
using UnityEngine.AI;

public class YBotMovement : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float idleTime = 3f;
    public float walkTime = 5f;
    public float wanderRadius = 10f;

    private float timer;
    private bool isWalking = false;

    public Transform aimball;            //-------------------------------
    void Start()
    {
        // Hitta komponenter
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent saknas p� " + gameObject.name);
            return;
        }

        agent.speed = walkSpeed;

        // Starta i idle-tillst�nd
        isWalking = false;
        timer = idleTime;
        agent.isStopped = true;

        // Uppdatera animationer
        UpdateAnimations();
    }

    void Update()
    {
        Aim();                                 //------------------------------

        timer -= Time.deltaTime;

        // Dags att byta tillst�nd?
        if (timer <= 0)
        {
            if (isWalking)
            {
                // Byt till idle
                isWalking = false;
                timer = idleTime;
                agent.isStopped = true;
            }
            else
            {
                // Byt till walking och v�lj en ny destination
                isWalking = true;
                timer = walkTime;
                FindRandomDestination();
                agent.isStopped = false;
            }

            // Uppdatera animationer f�r nytt tillst�nd
            UpdateAnimations();
        }

        // Uppdatera animationer baserat p� faktisk hastighet
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("VelocityY", speed);
        }
    }

    void FindRandomDestination()
    {
        // Hitta en slumpm�ssig position inom wanderRadius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0; // H�ll p� samma h�jd

        Vector3 finalPosition = transform.position + randomDirection;

        // S�kerst�ll att positionen �r p� NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(finalPosition, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);

            // Om din animator anv�nder VelocityY ist�llet f�r isWalking
            if (!isWalking)
            {
                animator.SetFloat("VelocityY", 0f);
                animator.SetFloat("VelocityX", 0f);
            }
        }
    }

    private void Aim()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);  // R�kna ut vart mitten av sk�rmen �r
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);                       // Skapa en ray fr�n mitten av sk�rmen
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))                         // Skjut iv�g rayen 1000m mot sk�rmens mitt.
        {
            aimball.position = hit.point;                                           // S�tt aimballens position till d�r rayen tr�ffade.
            Debug.Log("Tr�ffade objekt: " + hit.collider.gameObject.name);
        }
    }
}