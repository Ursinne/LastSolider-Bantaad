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
        // S�kerst�ll att h�lsan inte g�r under 0
        currentHealth = Mathf.Max(0, currentHealth);

        // Utl�s h�ndelsen att h�lsan har �ndrats
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

        // Kontrollera om djuret har d�tt
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        // S�kerst�ll att h�lsan inte �verstiger maxh�lsan
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Utl�s h�ndelsen att h�lsan har �ndrats
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // Utl�s d�dsh�ndelsen
        OnDeath?.Invoke();

        // St�ng av alla r�relserelaterade animationer
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);

            // Trigga d�dsanimation
            animator.SetTrigger("die");
        }
    }

    // Hj�lpmetod f�r att kontrollera om en parameter finns i Animator
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