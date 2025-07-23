using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NpcZombie1 : MonoBehaviour
{
    [Header("Zombie Settings")]
    public float detectionRange = 10f;      // Räckvidd där zombien upptäcker spelaren
    public float attackRange = 1.5f;        // Räckvidd där zombien kan attackera spelaren
    public float zombieSpeed = 2f;          // Zombiens rörelsehastighet
    public float attackCooldown = 2f;       // Tid mellan attacker

    [Header("Health Settings")]
    public float maxHealth = 100f;          // Zombiens maximala hälsa
    public float currentHealth;             // Zombiens nuvarande hälsa
    public float attackDamage = 10f;        // Skada som zombien gör vid attack

    [Header("Death Settings")]
    public float deathAnimationTime = 3f;   // Hur lång tid dödsanimationen spelas innan objekt förstörs
    public GameObject[] dropItems;           // Möjliga items som zombien kan släppa när den dör
    [Range(0, 1)] public float dropChance = 0.3f; // Chans att zombien släpper något item

    [Header("References")]
    private CharacterController controller;  // Referens till CharacterController-komponenten
    private Animator animator;               // Referens till Animator-komponenten
    private Transform player;                // Referens till spelarens transform

    // Status-variabler
    private bool playerDetected = false;
    private float lastAttackTime;
    private Vector3 targetPosition;
    private bool isDead = false;

    public bool isPlayerDead = false;
    public float bitingDistance = 1.0f;
    private Vector3 deadPlayerPosition;

    void Start()
    {
        // Initialisera hälsa
        currentHealth = maxHealth;

        // Hitta nödvändiga komponenter
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Hitta spelaren med tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Ingen spelare med taggen 'Player' hittades!");
        }
    }

    void Update()
    {
        // Om zombien är död, avbryt uppdateringen
        if (isDead || player == null) return;

        if (isPlayerDead)
        {
            GoToDeadPlayer();
            return;
        }

        // Beräkna avstånd till spelaren
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Kontrollera om spelaren är inom detektionsräckvidden
        playerDetected = distanceToPlayer <= detectionRange;

        if (playerDetected)
        {
            // Jaga spelaren
            ChasePlayer();

            // Kontrollera om zombien är tillräckligt nära för att attackera
            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Zombien ser inte spelaren, stå stilla eller vandra slumpmässigt
            IdleOrWander();
        }

        // Uppdatera animationer
        UpdateAnimations();
    }

    private void GoToDeadPlayer()
    {
        if (isDead) return;

        // Beräkna avstånd till den döda spelaren
        float distanceToBody = Vector3.Distance(transform.position, deadPlayerPosition);

        if (distanceToBody > bitingDistance)
        {
            // Gå mot kroppen
            Vector3 direction = (deadPlayerPosition - transform.position).normalized;
            direction.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            if (controller != null)
            {
                Vector3 movement = direction * zombieSpeed * Time.deltaTime;
                if (!controller.isGrounded)
                {
                    movement.y = Physics.gravity.y * Time.deltaTime;
                }
                controller.Move(movement);
            }

            // Uppdatera animationer för att visa att zombien går
            if (animator != null)
            {
                animator.SetFloat("VelocityY", 1);
                animator.SetFloat("VelocityX", 0);
            }
        }
        else
        {
            // Bitanimation när zombien når kroppen
            if (animator != null)
            {
                animator.SetFloat("VelocityY", 0);
                animator.SetFloat("VelocityX", 0);
                animator.SetBool("IsBiting", true);
            }
        }
    }

    private void ChasePlayer()
    {
        // Vänd zombien mot spelaren
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Ignorera höjdskillnad för rotation

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

        // Flytta zombien mot spelaren
        if (controller != null)
        {
            Vector3 movement = direction * zombieSpeed * Time.deltaTime;

            // Lägg till gravitation om det behövs
            if (!controller.isGrounded)
            {
                movement.y = Physics.gravity.y * Time.deltaTime;
            }

            controller.Move(movement);
        }
    }

    private void AttackPlayer()
    {
        // Registrera tid för attacken
        lastAttackTime = Time.time;

        // Spela attackanimation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Försök skada spelaren
        if (player != null)
        {
            // Kontrollera om spelaren har ett hälsosystem och orsaka skada
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Zombie orsakade {attackDamage} skada på spelaren.");
            }
            else
            {
                // Fallback om inget PlayerHealth-skript finns
                Debug.LogWarning("Spelaren har inget PlayerHealth-skript!");
            }
        }
    }

    public void PlayerDied(Vector3 playerPos)
    {
        isPlayerDead = true;
        deadPlayerPosition = playerPos;

        // Zombien springer mot den döda spelaren
        zombieSpeed *= 1.5f; // Snabbare för att nå kroppen
    }

    private void IdleOrWander()
    {
        // Zombien står stilla eller utför enkel vandringsbeteende
        // Här kan du implementera slumpmässig rörelse om du vill
        if (animator != null)
        {
            animator.SetFloat("VelocityY", 0);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Uppdatera animationsparametrar baserat på tillstånd
        if (playerDetected && !isDead)
        {
            // Zombien rör sig mot spelaren
            animator.SetFloat("VelocityY", 1); // Framåt rörelse
            animator.SetFloat("VelocityX", 0); // Ingen sidrörelser
        }
        else
        {
            // Zombien är i idle-läge
            animator.SetFloat("VelocityY", 0);
            animator.SetFloat("VelocityX", 0);
        }
    }

    // Metod för att hantera när zombien tar skada
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Zombie tog {damageAmount} skada. Återstående hälsa: {currentHealth}");

        // Aktivera en skadad-animation om det finns
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Kontrollera om zombien har dött
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Inaktivera CharacterController för att stoppa rörelse
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Aktivera dödsanimation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsDead", true);
        }

        // Slumpmässigt släpp items vid död
        DropItems();

        // Ta bort eventuella colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Förstör zombie-objektet efter en fördröjning
        StartCoroutine(DestroyAfterDelay());
    }

    private void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;

        // Slumpa om zombien ska släppa något
        if (Random.value <= dropChance)
        {
            // Slumpa vilket item som ska släppas
            int randomIndex = Random.Range(0, dropItems.Length);
            GameObject itemToSpawn = dropItems[randomIndex];

            if (itemToSpawn != null)
            {
                // Skapa item med en liten offset från zombiens position
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
                Instantiate(itemToSpawn, spawnPos, Quaternion.identity);
                Debug.Log($"Zombien släppte: {itemToSpawn.name}");
            }
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Vänta på att dödsanimationen ska spelas klart
        yield return new WaitForSeconds(deathAnimationTime);

        // Förstör zombie-objektet
        Destroy(gameObject);
    }

    // Visualisera detektions- och attackområdet i Unity-editorn
    void OnDrawGizmosSelected()
    {
        // Rita detektionsområdet
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rita attackområdet
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}