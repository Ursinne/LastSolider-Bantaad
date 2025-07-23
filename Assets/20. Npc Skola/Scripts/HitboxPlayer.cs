
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

    // Skademodifierare f�r olika kroppsdelar
    private float GetDamageMultiplier()
    {
        switch (hitboxType)
        {
            case HitboxType.Head:
                return 2.0f; // Kritisk tr�ff - dubbel skada
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
        // Om playerHealthScript inte �r tilldelad, f�rs�k hitta den automatiskt
        if (playerHealth == null)
        {
            // Kolla f�rst p� detta objekt
            playerHealth = GetComponent<PlayerHealth>();

            // Om inte hittad, kolla p� f�r�ldern
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();

                if (playerHealth == null)
                {
                    Debug.LogWarning("Ingen PlayerHealth hittad f�r " + gameObject.name);
                }
            }
        }
    }

    public void OnRaycastHit(float baseDamageAmount)
    {
        Debug.Log($"OnRaycastHit kallad p� {gameObject.name}!");
        Debug.Log($"Bas skada: {baseDamageAmount}");
        Debug.Log($"Tr�ffad kroppsdel: {hitboxType}");
        Debug.Log($"PlayerHealth referens: {playerHealth != null}");

        if (playerHealth != null)
        {
            float finalDamage = baseDamageAmount * GetDamageMultiplier();

            Debug.Log($"Slutlig skada efter multiplikator: {finalDamage}");

            playerHealth.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogError("KRITISK: Ingen PlayerHealth hittad p� " + gameObject.name);
        }
    }
}