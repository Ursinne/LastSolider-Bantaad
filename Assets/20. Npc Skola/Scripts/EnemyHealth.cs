using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyHealth : MonoBehaviour
{
    private float maxHealth = 100f;                 // Maximal h�lsa.       
    public float currentHealth;                    // Nuvarande h�lsoniv�.
    public bool isDead;                             // Bool som �r sann om spelaren �r d�d.
    void Start()
    {
        isDead = false;                             // Se till att spelaren inte �r d�d vid spelets start.
        currentHealth = maxHealth;                  // Resetta healthv�rdet.
    }
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;              // dra bort skadev�rdet fr�n h�lsov�rdet.
        if (currentHealth <= 0)
        {
            isDead = true;                          // S�tt spelaren som d�d.
        }
    }
}
