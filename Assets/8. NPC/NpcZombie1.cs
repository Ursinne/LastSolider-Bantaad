using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NpcZombie1 : MonoBehaviour
{
    [Header("Zombie Settings")]
    public float detectionRange = 10f;      // R�ckvidd d�r zombien uppt�cker spelaren
    public float attackRange = 1.5f;        // R�ckvidd d�r zombien kan attackera spelaren
    public float zombieSpeed = 2f;          // Zombiens r�relsehastighet
    public float attackCooldown = 2f;       // Tid mellan attacker

    [Header("Health Settings")]
    public float maxHealth = 100f;          // Zombiens maximala h�lsa
    public float currentHealth;             // Zombiens nuvarande h�lsa
    public float attackDamage = 10f;        // Skada som zombien g�r vid attack

    [Header("Death Settings")]
    public float deathAnimationTime = 3f;   // Hur l�ng tid d�dsanimationen spelas innan objekt f�rst�rs
    public GameObject[] dropItems;           // M�jliga items som zombien kan sl�ppa n�r den d�r
    [Range(0, 1)] public float dropChance = 0.3f; // Chans att zombien sl�pper n�got item

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
        // Initialisera h�lsa
        currentHealth = maxHealth;

        // Hitta n�dv�ndiga komponenter
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
        // Om zombien �r d�d, avbryt uppdateringen
        if (isDead || player == null) return;

        if (isPlayerDead)
        {
            GoToDeadPlayer();
            return;
        }

        // Ber�kna avst�nd till spelaren
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Kontrollera om spelaren �r inom detektionsr�ckvidden
        playerDetected = distanceToPlayer <= detectionRange;

        if (playerDetected)
        {
            // Jaga spelaren
            ChasePlayer();

            // Kontrollera om zombien �r tillr�ckligt n�ra f�r att attackera
            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Zombien ser inte spelaren, st� stilla eller vandra slumpm�ssigt
            IdleOrWander();
        }

        // Uppdatera animationer
        UpdateAnimations();
    }

    private void GoToDeadPlayer()
    {
        if (isDead) return;

        // Ber�kna avst�nd till den d�da spelaren
        float distanceToBody = Vector3.Distance(transform.position, deadPlayerPosition);

        if (distanceToBody > bitingDistance)
        {
            // G� mot kroppen
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

            // Uppdatera animationer f�r att visa att zombien g�r
            if (animator != null)
            {
                animator.SetFloat("VelocityY", 1);
                animator.SetFloat("VelocityX", 0);
            }
        }
        else
        {
            // Bitanimation n�r zombien n�r kroppen
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
        // V�nd zombien mot spelaren
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Ignorera h�jdskillnad f�r rotation

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

        // Flytta zombien mot spelaren
        if (controller != null)
        {
            Vector3 movement = direction * zombieSpeed * Time.deltaTime;

            // L�gg till gravitation om det beh�vs
            if (!controller.isGrounded)
            {
                movement.y = Physics.gravity.y * Time.deltaTime;
            }

            controller.Move(movement);
        }
    }

    private void AttackPlayer()
    {
        // Registrera tid f�r attacken
        lastAttackTime = Time.time;

        // Spela attackanimation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // F�rs�k skada spelaren
        if (player != null)
        {
            // Kontrollera om spelaren har ett h�lsosystem och orsaka skada
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Zombie orsakade {attackDamage} skada p� spelaren.");
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

        // Zombien springer mot den d�da spelaren
        zombieSpeed *= 1.5f; // Snabbare f�r att n� kroppen
    }

    private void IdleOrWander()
    {
        // Zombien st�r stilla eller utf�r enkel vandringsbeteende
        // H�r kan du implementera slumpm�ssig r�relse om du vill
        if (animator != null)
        {
            animator.SetFloat("VelocityY", 0);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Uppdatera animationsparametrar baserat p� tillst�nd
        if (playerDetected && !isDead)
        {
            // Zombien r�r sig mot spelaren
            animator.SetFloat("VelocityY", 1); // Fram�t r�relse
            animator.SetFloat("VelocityX", 0); // Ingen sidr�relser
        }
        else
        {
            // Zombien �r i idle-l�ge
            animator.SetFloat("VelocityY", 0);
            animator.SetFloat("VelocityX", 0);
        }
    }

    // Metod f�r att hantera n�r zombien tar skada
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Zombie tog {damageAmount} skada. �terst�ende h�lsa: {currentHealth}");

        // Aktivera en skadad-animation om det finns
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Kontrollera om zombien har d�tt
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Inaktivera CharacterController f�r att stoppa r�relse
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Aktivera d�dsanimation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsDead", true);
        }

        // Slumpm�ssigt sl�pp items vid d�d
        DropItems();

        // Ta bort eventuella colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // F�rst�r zombie-objektet efter en f�rdr�jning
        StartCoroutine(DestroyAfterDelay());
    }

    private void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;

        // Slumpa om zombien ska sl�ppa n�got
        if (Random.value <= dropChance)
        {
            // Slumpa vilket item som ska sl�ppas
            int randomIndex = Random.Range(0, dropItems.Length);
            GameObject itemToSpawn = dropItems[randomIndex];

            if (itemToSpawn != null)
            {
                // Skapa item med en liten offset fr�n zombiens position
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
                Instantiate(itemToSpawn, spawnPos, Quaternion.identity);
                Debug.Log($"Zombien sl�ppte: {itemToSpawn.name}");
            }
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // V�nta p� att d�dsanimationen ska spelas klart
        yield return new WaitForSeconds(deathAnimationTime);

        // F�rst�r zombie-objektet
        Destroy(gameObject);
    }

    // Visualisera detektions- och attackomr�det i Unity-editorn
    void OnDrawGizmosSelected()
    {
        // Rita detektionsomr�det
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rita attackomr�det
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}