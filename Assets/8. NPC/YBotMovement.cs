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
            Debug.LogError("NavMeshAgent saknas på " + gameObject.name);
            return;
        }

        agent.speed = walkSpeed;

        // Starta i idle-tillstånd
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

        // Dags att byta tillstånd?
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
                // Byt till walking och välj en ny destination
                isWalking = true;
                timer = walkTime;
                FindRandomDestination();
                agent.isStopped = false;
            }

            // Uppdatera animationer för nytt tillstånd
            UpdateAnimations();
        }

        // Uppdatera animationer baserat på faktisk hastighet
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("VelocityY", speed);
        }
    }

    void FindRandomDestination()
    {
        // Hitta en slumpmässig position inom wanderRadius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0; // Håll på samma höjd

        Vector3 finalPosition = transform.position + randomDirection;

        // Säkerställ att positionen är på NavMesh
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

            // Om din animator använder VelocityY istället för isWalking
            if (!isWalking)
            {
                animator.SetFloat("VelocityY", 0f);
                animator.SetFloat("VelocityX", 0f);
            }
        }
    }

    private void Aim()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);  // Räkna ut vart mitten av skärmen är
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);                       // Skapa en ray från mitten av skärmen
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))                         // Skjut iväg rayen 1000m mot skärmens mitt.
        {
            aimball.position = hit.point;                                           // Sätt aimballens position till där rayen träffade.
            Debug.Log("Träffade objekt: " + hit.collider.gameObject.name);
        }
    }
}