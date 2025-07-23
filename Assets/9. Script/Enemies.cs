using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyType = "Generic"; // Den h�r anv�nds till quest-uppdatering
    public float maxHealth = 100f;
    public float currentHealth;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float detectionRange = 10f;
    public float attackCooldown = 2f;

    [Header("Rewards")]
    public int experienceReward = 10;
    public int goldReward = 5;
    public GameObject[] itemDrops; // Prefabs f�r objekt som kan sl�ppas
    [Range(0f, 1f)] public float dropChance = 0.3f; // Chans att sl�ppa n�got

    [Header("References")]
    public Animator animator;

    private bool isDead = false;
    private float lastAttackTime;
    private Transform player;
    private CharacterController characterController;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Om spelaren �r inom detection range, b�rja f�rf�lja
            if (distanceToPlayer <= detectionRange)
            {
                // Rotera mot spelaren
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                     Quaternion.LookRotation(direction),
                                                     5f * Time.deltaTime);

                // R�r sig mot spelaren om inte inom attack range
                if (distanceToPlayer > attackRange)
                {
                    // S�tt animationen f�r att springa
                    if (animator != null)
                        animator.SetBool("isRunning", true);

                    // R�r sig mot spelaren
                    if (characterController != null)
                    {
                        Vector3 moveDirection = direction * 3f * Time.deltaTime;
                        characterController.Move(moveDirection);
                    }
                    else
                    {
                        transform.position += direction * 3f * Time.deltaTime;
                    }
                }
                else
                {
                    // Inom attack range, stanna och attackera
                    if (animator != null)
                        animator.SetBool("isRunning", false);

                    // Attackera om cooldown �r �ver
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        Attack();
                    }
                }
            }
            else
            {
                // Utanf�r detection range, �terg� till vanligt tillst�nd
                if (animator != null)
                    animator.SetBool("isRunning", false);
            }
        }
    }

    private void Attack()
    {
        lastAttackTime = Time.time;

        // Spela attack animation
        if (animator != null)
            animator.SetTrigger("attack");

        // F�rs�k skada spelaren
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"Enemy attacked player for {attackDamage} damage");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Spela skada animation
        if (animator != null)
            animator.SetTrigger("takeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Spela d�ds-animation
        if (animator != null)
        {
            animator.SetTrigger("die");
            animator.SetBool("isDead", true);
        }

        // Ge bel�ningar till spelaren
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.GainXP(experienceReward);
                playerScript.GainGold(goldReward);
            }
        }

        // Droppa items om det slumpas fram
        if (Random.value <= dropChance && itemDrops.Length > 0)
        {
            int randomItemIndex = Random.Range(0, itemDrops.Length);
            if (itemDrops[randomItemIndex] != null)
            {
                Instantiate(itemDrops[randomItemIndex],
                           transform.position + Vector3.up * 0.5f,
                           Quaternion.identity);
            }
        }

        // Uppdatera quest-framsteg
        QuestManager.Instance?.UpdateQuestProgress("kill", enemyType, 1);

        // Inaktivera alla colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        if (characterController != null)
            characterController.enabled = false;

        // F�rst�r objektet efter en stund
        Destroy(gameObject, 5f);
    }

    // Visualisera detektions- och attackradie i editorn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}