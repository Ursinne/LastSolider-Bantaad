using UnityEngine;
using InventoryAndCrafting;

public class PickupWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public ItemData weaponItemData; // Referens till vapnets ItemData
    public WeaponType weaponType = WeaponType.None;

    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Audio & VFX")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            // Hitta spelaren
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            // Kontrollera avst�nd
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < pickupRange)
            {
                Pickup(player);
            }
        }
    }

    private void Pickup(GameObject player)
    {
        // Kontrollera att vi har allt vi beh�ver
        if (weaponItemData == null)
        {
            Debug.LogError("Inget weapon ItemData tilldelat!");
            return;
        }

        Debug.Log($"F�rs�ker plocka upp {weaponItemData.itemName}");

        // L�gg till i spelarens inventory
        bool added = InventoryManager.Instance.AddItem(weaponItemData);

        if (added)
        {
            // Spela ljud om det finns
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Skapa visuell effekt om det finns
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Visa notifikation via NotificationManager
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowItemNotification(weaponItemData, 1);
            }

            // Ta bort vapnet fr�n scenen
            Destroy(gameObject);
        }
    }

    // Visualisera r�ckvidden
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}