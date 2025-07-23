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
    public bool isDead = false;  // Ny flagga f�r att veta om spelaren �r d�d

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
    public float respawnTime = 5f;  // Tid innan spelaren respawnar (om det anv�nds)
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
        // Initiera startv�rden
        ResetToFullHealth();

        // Hitta referenser till andra komponenter
        playerScript = GetComponent<Player>();
        movementScript = GetComponent<YBotMovement>();
        animator = GetComponent<Animator>();

        if (playerScript != null)
        {
            // Synka v�rden mellan scripten
            playerScript.maxWillpower = maxWillpower;
            playerScript.willpower = willpower;
            playerScript.willpowerRegenRate = willpowerRegenRate;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            Debug.Log("Animator hittades p� barn-objekt: " + (animator != null));
        }
    }

    private void Update()
    {
        if (isDead) return;  // G�r inget om spelaren �r d�d

        UpdateSliders();
        HandleStamina();
        SyncWillpowerWithPlayer();
    }

    private void SyncWillpowerWithPlayer()
    {
        if (playerScript != null)
        {
            // Synkronisera v�rden fr�n Player-scriptet
            willpower = playerScript.willpower;
        }
        else
        {
            // Om Player-scriptet inte finns, hantera regeneration h�r
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
        // Minska stamina n�r spelaren springer
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            stamina -= staminaDepletionRate * Time.deltaTime;
        }
        // �terst�ll stamina n�r inte sprint �r aktivt
        else if (stamina < maxStamina)
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Min(stamina, maxStamina);
        }

        // Meddelande n�r stamina �r slut
        if (stamina <= 0)
        {
            Debug.Log("Stamina �r slut, du kan inte springa mer. V�nta lite f�r att springa igen.");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log($"TakeDamage kallad! Skadebelopp: {damageAmount}");
        Debug.Log($"Nuvarande h�lsa f�re skada: {health}");

        if (isDead)
        {
            Debug.Log("Kan inte ta skada - spelaren �r redan d�d");
            return;
        }

        health -= damageAmount;
        health = Mathf.Max(health, 0);

        Debug.Log($"Nuvarande h�lsa efter skada: {health}");

        // Triggra skadad-animation om det finns
        if (animator != null && !isDead)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (health <= 0 && !isDead)
        {
            Debug.Log("Spelaren ska d�!");
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
        Debug.Log("Spelaren har d�tt!");

        // Inaktivera spelarens r�relseskript
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Spela d�dsanimation om den finns
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

        //    // Meddela alla zombies att spelaren �r d�d
        //    NotifyZombiesOfPlayerDeath();
        //}

        // Alternativ till att f�rst�ra spelaren direkt:
        // Om du vill ha respawn eller game over-sk�rm ist�llet
        // Invoke("GameOver", 3f); eller Invoke("Respawn", respawnTime);
    }

    private void NotifyZombiesOfPlayerDeath()
    {
        // Hitta alla zombies i scenen och meddela dem att spelaren �r d�d
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
            // Default respawn om ingen punkt �r specificerad
            transform.position = new Vector3(0, 1, 0);
        }

        ResetToFullHealth();

        // �teraktivera spelarens r�relseskript
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        // �terst�ll animationer
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
//    public float maxWillpower = 100f; // �ndrat fr�n maxMana till maxWillpower

//    [Header("Current Values")]
//    public float health;
//    public float stamina;
//    public float willpower; // �ndrat fr�n mana till willpower

//    [Header("UI Sliders")]
//    public Slider healthSlider;
//    public Slider staminaSlider;
//    public Slider willpowerSlider; // �ndrat fr�n manaSlider till willpowerSlider

//    [Header("Regeneration Settings")]
//    public float staminaRegenRate = 10f;
//    public float willpowerRegenRate = 5f; // �ndrat fr�n manaRegenRate till willpowerRegenRate

//    [Header("Sprint Settings")]
//    public float staminaDepletionRate = 10f;

//    private Player playerScript; // Referens till spelarens Player-script

//    private void Start()
//    {
//        // Initiera startv�rden
//        ResetToFullHealth();

//        // Hitta Player-scriptet p� samma objekt
//        playerScript = GetComponent<Player>();
//        if (playerScript != null)
//        {
//            // Synka v�rden mellan scripten
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
//            // Synkronisera v�rden fr�n Player-scriptet
//            willpower = playerScript.willpower;
//        }
//        else
//        {
//            // Om Player-scriptet inte finns, hantera regeneration h�r
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
//        // Minska stamina n�r spelaren springer
//        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
//        {
//            stamina -= staminaDepletionRate * Time.deltaTime;
//        }
//        // �terst�ll stamina n�r inte sprint �r aktivt
//        else if (stamina < maxStamina)
//        {
//            stamina += staminaRegenRate * Time.deltaTime;
//            stamina = Mathf.Min(stamina, maxStamina);
//        }

//        // Meddelande n�r stamina �r slut
//        if (stamina <= 0)
//        {
//            Debug.Log("Stamina �r slut, du kan inte springa mer. V�nta lite f�r att springa igen.");
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

//    public void UseWillpower(float willpowerAmount) // �ndrat fr�n UseMana till UseWillpower
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
//        Debug.Log("Spelaren har d�tt!");
//        // L�gg till d�dshantering h�r, t.ex. Game Over sk�rm
//        Destroy(gameObject);
//    }
//}