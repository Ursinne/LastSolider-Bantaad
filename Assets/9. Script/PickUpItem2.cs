using UnityEngine;
using InventoryAndCrafting;

public class PickupItem2: MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;

    // Sätt en större trigger-radie
    private void OnValidate()
    {
        // Säkerställ att objektet har en trigger collider
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 2f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Kontrollera om det är en spelare och om F-tangenten trycks
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F-tangent tryckt: försöker plocka upp " + (itemData != null ? itemData.itemName : "okänt föremål"));

            if (itemData == null)
            {
                Debug.LogError("ItemData saknas på " + gameObject.name);
                return;
            }

            // Leta efter InventoryManager
            if (InventoryManager.Instance != null)
            {
                Debug.Log("InventoryManager hittad, försöker lägga till föremål");

                // Försök lägga till föremålet i inventariet
                bool added = InventoryManager.Instance.AddItem(itemData, amount);

                if (added)
                {
                    Debug.Log("Föremål tillagt i inventariet: " + itemData.itemName);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("Kunde inte lägga till föremål, inventariet kan vara fullt");
                }
            }
            else
            {
                Debug.LogError("InventoryManager.Instance är null! Kontrollera att den finns i scenen.");
            }
        }
    }

    // För att visualisera i editorn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}