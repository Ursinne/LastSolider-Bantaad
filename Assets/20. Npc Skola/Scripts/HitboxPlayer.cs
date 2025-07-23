
using UnityEngine;

public class HitboxPlayer : MonoBehaviour
{
    public enum HitboxType
    {
        Head,
        Body,
        Limbs
    }

    public HitboxType hitboxType;
    public PlayerHealth playerHealth;

    // Skademodifierare för olika kroppsdelar
    private float GetDamageMultiplier()
    {
        switch (hitboxType)
        {
            case HitboxType.Head:
                return 2.0f; // Kritisk träff - dubbel skada
            case HitboxType.Body:
                return 1.0f; // Normal skada
            case HitboxType.Limbs:
                return 0.5f; // Reducerad skada
            default:
                return 1.0f;
        }
    }

    private void Start()
    {
        // Om playerHealthScript inte är tilldelad, försök hitta den automatiskt
        if (playerHealth == null)
        {
            // Kolla först på detta objekt
            playerHealth = GetComponent<PlayerHealth>();

            // Om inte hittad, kolla på föräldern
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();

                if (playerHealth == null)
                {
                    Debug.LogWarning("Ingen PlayerHealth hittad för " + gameObject.name);
                }
            }
        }
    }

    public void OnRaycastHit(float baseDamageAmount)
    {
        Debug.Log($"OnRaycastHit kallad på {gameObject.name}!");
        Debug.Log($"Bas skada: {baseDamageAmount}");
        Debug.Log($"Träffad kroppsdel: {hitboxType}");
        Debug.Log($"PlayerHealth referens: {playerHealth != null}");

        if (playerHealth != null)
        {
            float finalDamage = baseDamageAmount * GetDamageMultiplier();

            Debug.Log($"Slutlig skada efter multiplikator: {finalDamage}");

            playerHealth.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogError("KRITISK: Ingen PlayerHealth hittad på " + gameObject.name);
        }
    }
}