using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxWillpower = 100f;

    [Header("Current Values")]
    public float health;
    public float stamina;
    public float willpower;
    public bool isDead = false;  // Ny flagga för att veta om spelaren är död

    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider willpowerSlider;

    [Header("Regeneration Settings")]
    public float staminaRegenRate = 10f;
    public float willpowerRegenRate = 5f;

    [Header("Sprint Settings")]
    public float staminaDepletionRate = 10f;

    [Header("Death Settings")]
    public float respawnTime = 5f;  // Tid innan spelaren respawnar (om det används)
    public Transform respawnPoint;  // Optional respawn point

    private Player playerScript;
    private YBotMovement movementScript;
    private Animator animator;

    public static PlayerHealth Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Initiera startvärden
        ResetToFullHealth();

        // Hitta referenser till andra komponenter
        playerScript = GetComponent<Player>();
        movementScript = GetComponent<YBotMovement>();
        animator = GetComponent<Animator>();

        if (playerScript != null)
        {
            // Synka värden mellan scripten
            playerScript.maxWillpower = maxWillpower;
            playerScript.willpower = willpower;
            playerScript.willpowerRegenRate = willpowerRegenRate;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            Debug.Log("Animator hittades på barn-objekt: " + (animator != null));
        }
    }

    private void Update()
    {
        if (isDead) return;  // Gör inget om spelaren är död

        UpdateSliders();
        HandleStamina();
        SyncWillpowerWithPlayer();
    }

    private void SyncWillpowerWithPlayer()
    {
        if (playerScript != null)
        {
            // Synkronisera värden från Player-scriptet
            willpower = playerScript.willpower;
        }
        else
        {
            // Om Player-scriptet inte finns, hantera regeneration här
            willpower = Mathf.Min(willpower + willpowerRegenRate * Time.deltaTime, maxWillpower);
        }
    }

    private void UpdateSliders()
    {
        if (healthSlider != null) healthSlider.value = health / maxHealth;
        if (staminaSlider != null) staminaSlider.value = stamina / maxStamina;
        if (willpowerSlider != null) willpowerSlider.value = willpower / maxWillpower;
    }

    private void HandleStamina()
    {
        // Minska stamina när spelaren springer
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            stamina -= staminaDepletionRate * Time.deltaTime;
        }
        // Återställ stamina när inte sprint är aktivt
        else if (stamina < maxStamina)
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Min(stamina, maxStamina);
        }

        // Meddelande när stamina är slut
        if (stamina <= 0)
        {
            Debug.Log("Stamina är slut, du kan inte springa mer. Vänta lite för att springa igen.");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log($"TakeDamage kallad! Skadebelopp: {damageAmount}");
        Debug.Log($"Nuvarande hälsa före skada: {health}");

        if (isDead)
        {
            Debug.Log("Kan inte ta skada - spelaren är redan död");
            return;
        }

        health -= damageAmount;
        health = Mathf.Max(health, 0);

        Debug.Log($"Nuvarande hälsa efter skada: {health}");

        // Triggra skadad-animation om det finns
        if (animator != null && !isDead)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (health <= 0 && !isDead)
        {
            Debug.Log("Spelaren ska dö!");
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        health += healAmount;
        health = Mathf.Min(health, maxHealth);
    }

    public void UseWillpower(float willpowerAmount)
    {
        if (isDead) return;

        willpower -= willpowerAmount;
        willpower = Mathf.Max(willpower, 0);

        // Synka med Player-scriptet om det finns
        if (playerScript != null)
        {
            playerScript.willpower = willpower;
        }
    }

    private void ResetToFullHealth()
    {
        health = maxHealth;
        stamina = maxStamina;
        willpower = maxWillpower;
        isDead = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Spelaren har dött!");

        // Inaktivera spelarens rörelseskript
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Spela dödsanimation om den finns
        if (animator != null)
        {
            animator.SetTrigger("isDead");
            //animator.SetBool("isDead", true);
            Debug.Log("Animator-referens: " + (animator != null ? "OK" : "NULL"));
        }
        //if (animator != null)
        //{
        //    animator.SetTrigger("Die");
        //    Debug.Log("Die-triggern har aktiverats");

        //    // Meddela alla zombies att spelaren är död
        //    NotifyZombiesOfPlayerDeath();
        //}

        // Alternativ till att förstöra spelaren direkt:
        // Om du vill ha respawn eller game over-skärm istället
        // Invoke("GameOver", 3f); eller Invoke("Respawn", respawnTime);
    }

    private void NotifyZombiesOfPlayerDeath()
    {
        // Hitta alla zombies i scenen och meddela dem att spelaren är död
        NpcZombie1[] zombies = FindObjectsOfType<NpcZombie1>();
        foreach (var zombie in zombies)
        {
            zombie.PlayerDied(transform.position);
        }
    }

    // Om du vill ha respawn funktion
    private void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            // Default respawn om ingen punkt är specificerad
            transform.position = new Vector3(0, 1, 0);
        }

        ResetToFullHealth();

        // Återaktivera spelarens rörelseskript
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        // Återställ animationer
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerHealth : MonoBehaviour
//{
//    [Header("Max Values")]
//    public float maxHealth = 100f;
//    public float maxStamina = 100f;
//    public float maxWillpower = 100f; // Ändrat från maxMana till maxWillpower

//    [Header("Current Values")]
//    public float health;
//    public float stamina;
//    public float willpower; // Ändrat från mana till willpower

//    [Header("UI Sliders")]
//    public Slider healthSlider;
//    public Slider staminaSlider;
//    public Slider willpowerSlider; // Ändrat från manaSlider till willpowerSlider

//    [Header("Regeneration Settings")]
//    public float staminaRegenRate = 10f;
//    public float willpowerRegenRate = 5f; // Ändrat från manaRegenRate till willpowerRegenRate

//    [Header("Sprint Settings")]
//    public float staminaDepletionRate = 10f;

//    private Player playerScript; // Referens till spelarens Player-script

//    private void Start()
//    {
//        // Initiera startvärden
//        ResetToFullHealth();

//        // Hitta Player-scriptet på samma objekt
//        playerScript = GetComponent<Player>();
//        if (playerScript != null)
//        {
//            // Synka värden mellan scripten
//            playerScript.maxWillpower = maxWillpower;
//            playerScript.willpower = willpower;
//            playerScript.willpowerRegenRate = willpowerRegenRate;
//        }
//    }

//    private void Update()
//    {
//        UpdateSliders();
//        HandleStamina();
//        SyncWillpowerWithPlayer();
//    }

//    private void SyncWillpowerWithPlayer()
//    {
//        if (playerScript != null)
//        {
//            // Synkronisera värden från Player-scriptet
//            willpower = playerScript.willpower;
//        }
//        else
//        {
//            // Om Player-scriptet inte finns, hantera regeneration här
//            willpower = Mathf.Min(willpower + willpowerRegenRate * Time.deltaTime, maxWillpower);
//        }
//    }

//    private void UpdateSliders()
//    {
//        if (healthSlider != null) healthSlider.value = health / maxHealth;
//        if (staminaSlider != null) staminaSlider.value = stamina / maxStamina;
//        if (willpowerSlider != null) willpowerSlider.value = willpower / maxWillpower;
//    }

//    private void HandleStamina()
//    {
//        // Minska stamina när spelaren springer
//        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
//        {
//            stamina -= staminaDepletionRate * Time.deltaTime;
//        }
//        // Återställ stamina när inte sprint är aktivt
//        else if (stamina < maxStamina)
//        {
//            stamina += staminaRegenRate * Time.deltaTime;
//            stamina = Mathf.Min(stamina, maxStamina);
//        }

//        // Meddelande när stamina är slut
//        if (stamina <= 0)
//        {
//            Debug.Log("Stamina är slut, du kan inte springa mer. Vänta lite för att springa igen.");
//        }
//    }

//    public void TakeDamage(float damageAmount)
//    {
//        health -= damageAmount;
//        health = Mathf.Max(health, 0);

//        if (health <= 0)
//        {
//            Die();
//        }
//    }

//    public void Heal(float healAmount)
//    {
//        health += healAmount;
//        health = Mathf.Min(health, maxHealth);
//    }

//    public void UseWillpower(float willpowerAmount) // Ändrat från UseMana till UseWillpower
//    {
//        willpower -= willpowerAmount;
//        willpower = Mathf.Max(willpower, 0);

//        // Synka med Player-scriptet om det finns
//        if (playerScript != null)
//        {
//            playerScript.willpower = willpower;
//        }
//    }

//    private void ResetToFullHealth()
//    {
//        health = maxHealth;
//        stamina = maxStamina;
//        willpower = maxWillpower;
//    }

//    private void Die()
//    {
//        Debug.Log("Spelaren har dött!");
//        // Lägg till dödshantering här, t.ex. Game Over skärm
//        Destroy(gameObject);
//    }
//}