using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyHealth : MonoBehaviour
{
    private float maxHealth = 100f;                 // Maximal hälsa.       
    public float currentHealth;                    // Nuvarande hälsonivå.
    public bool isDead;                             // Bool som är sann om spelaren är död.
    void Start()
    {
        isDead = false;                             // Se till att spelaren inte är död vid spelets start.
        currentHealth = maxHealth;                  // Resetta healthvärdet.
    }
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;              // dra bort skadevärdet från hälsovärdet.
        if (currentHealth <= 0)
        {
            isDead = true;                          // Sätt spelaren som död.
        }
    }
}
