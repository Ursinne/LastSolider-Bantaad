using UnityEngine;

public class AnimalHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;

    public delegate void DeathHandler();
    public event DeathHandler OnDeath;

    private SimpleAnimalAI animalAI;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animalAI = GetComponent<SimpleAnimalAI>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        // Säkerställ att hälsan inte går under 0
        currentHealth = Mathf.Max(0, currentHealth);

        // Utlös händelsen att hälsan har ändrats
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Animera skada
        if (animator != null && CheckAnimatorHasParameter("isDamaged"))
        {
            animator.SetTrigger("isDamaged");
        }

        // Skicka skadan till AI:n
        if (animalAI != null)
        {
            animalAI.TakeDamage(amount);
        }

        // Kontrollera om djuret har dött
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        // Säkerställ att hälsan inte överstiger maxhälsan
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Utlös händelsen att hälsan har ändrats
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // Utlös dödshändelsen
        OnDeath?.Invoke();

        // Stäng av alla rörelserelaterade animationer
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);

            // Trigga dödsanimation
            animator.SetTrigger("die");
        }
    }

    // Hjälpmetod för att kontrollera om en parameter finns i Animator
    private bool CheckAnimatorHasParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}