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

    // Skademodifierare f�r olika kroppsdelar
    private float GetDamageMultiplier()
    {
        switch (hitboxType)
        {
            case HitboxType.Head:
                return 2.0f;                    // Kritisk tr�ff - dubbel skada
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
        // Om enemyHealthScript inte �r tilldelad, f�rs�k hitta den automatiskt
        if (enemyHealth == null)
        {
            // Kolla f�rst p� detta objekt
            enemyHealth = GetComponent<EnemyHealth>();

            // Om inte hittad, kolla p� f�r�ldern
            if (enemyHealth == null)
            {
                enemyHealth = GetComponentInParent<EnemyHealth>();

                if (enemyHealth == null)
                {
                    Debug.LogWarning("Ingen EnemyHealth hittad f�r " + gameObject.name);
                }
            }
        }
    }

    public void OnRaycastHit(float baseDamageAmount)
    {
        // Kontrollera att healthScript inte �r null f�re anv�ndning
        if (enemyHealth != null)
        {
            // Ber�kna skada baserat p� tr�ffad kroppsdel
            float finalDamage = baseDamageAmount * GetDamageMultiplier();

            enemyHealth.TakeDamage(finalDamage);

            // Valfri debug information
            Debug.Log($"Tr�ffade {hitboxType} - Skada: {finalDamage}");
        }
        else
        {
            Debug.LogWarning("HitboxScript p� " + gameObject.name + " har ingen tilldelad EnemyHealth");
        }
    }
}
