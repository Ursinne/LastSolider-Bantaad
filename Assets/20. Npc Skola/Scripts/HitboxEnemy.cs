using UnityEngine;

public class HitboxEnemy : MonoBehaviour
{
    public enum HitboxType
    {
        Head,
        Body,
        Limbs
    }

    public HitboxType hitboxType;
    public EnemyHealth enemyHealth;

    // Skademodifierare för olika kroppsdelar
    private float GetDamageMultiplier()
    {
        switch (hitboxType)
        {
            case HitboxType.Head:
                return 2.0f;                    // Kritisk träff - dubbel skada
            case HitboxType.Body:
                return 1.0f;                    // Normal skada
            case HitboxType.Limbs:
                return 0.5f;                    // Reducerad skada
            default:
                return 1.0f;
        }
    }

    private void Start()
    {
        // Om enemyHealthScript inte är tilldelad, försök hitta den automatiskt
        if (enemyHealth == null)
        {
            // Kolla först på detta objekt
            enemyHealth = GetComponent<EnemyHealth>();

            // Om inte hittad, kolla på föräldern
            if (enemyHealth == null)
            {
                enemyHealth = GetComponentInParent<EnemyHealth>();

                if (enemyHealth == null)
                {
                    Debug.LogWarning("Ingen EnemyHealth hittad för " + gameObject.name);
                }
            }
        }
    }

    public void OnRaycastHit(float baseDamageAmount)
    {
        // Kontrollera att healthScript inte är null före användning
        if (enemyHealth != null)
        {
            // Beräkna skada baserat på träffad kroppsdel
            float finalDamage = baseDamageAmount * GetDamageMultiplier();

            enemyHealth.TakeDamage(finalDamage);

            // Valfri debug information
            Debug.Log($"Träffade {hitboxType} - Skada: {finalDamage}");
        }
        else
        {
            Debug.LogWarning("HitboxScript på " + gameObject.name + " har ingen tilldelad EnemyHealth");
        }
    }
}
