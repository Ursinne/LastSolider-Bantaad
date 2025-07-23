using UnityEngine;
using InventoryAndCrafting;

public class PickupItem2: MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;

    // S�tt en st�rre trigger-radie
    private void OnValidate()
    {
        // S�kerst�ll att objektet har en trigger collider
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
        // Kontrollera om det �r en spelare och om F-tangenten trycks
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F-tangent tryckt: f�rs�ker plocka upp " + (itemData != null ? itemData.itemName : "ok�nt f�rem�l"));

            if (itemData == null)
            {
                Debug.LogError("ItemData saknas p� " + gameObject.name);
                return;
            }

            // Leta efter InventoryManager
            if (InventoryManager.Instance != null)
            {
                Debug.Log("InventoryManager hittad, f�rs�ker l�gga till f�rem�l");

                // F�rs�k l�gga till f�rem�let i inventariet
                bool added = InventoryManager.Instance.AddItem(itemData, amount);

                if (added)
                {
                    Debug.Log("F�rem�l tillagt i inventariet: " + itemData.itemName);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("Kunde inte l�gga till f�rem�l, inventariet kan vara fullt");
                }
            }
            else
            {
                Debug.LogError("InventoryManager.Instance �r null! Kontrollera att den finns i scenen.");
            }
        }
    }

    // F�r att visualisera i editorn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}