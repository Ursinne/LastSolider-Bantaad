using UnityEngine;
using System.Collections.Generic;
using InventoryAndCrafting;

public class EquippableItemSystem : MonoBehaviour
{
    [Header("References")]
    public Transform handHoldPoint; // Position där verktyget visas i spelarens hand

    [Header("Prefab References")]
    public List<EquippablePrefabMapping> equippablePrefabs = new List<EquippablePrefabMapping>();

    private GameObject currentEquippedObject; // Referens till verktyget som hålls just nu
    private ItemData currentEquippedItem; // Referens till ItemData för nuvarande verktyg

    // Singleton-instans
    private static EquippableItemSystem _instance;
    public static EquippableItemSystem Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Lyssna på händelser från hotbar eller inventory
        if (InventoryManager.Instance != null)
        {
            // Om du har en event för när ett föremål väljs i hotbar, prenumerera på det här
            // InventoryManager.Instance.onItemSelected += OnItemSelected;
        }
    }

    // Denna metod anropas när ett föremål väljs i hotbar
    public void EquipItem(ItemData itemToEquip)
    {
        // Avutrusta nuvarande föremål om något är utrustat
        if (currentEquippedObject != null)
        {
            Destroy(currentEquippedObject);
            currentEquippedObject = null;
            currentEquippedItem = null;
        }

        // Kontrollera om föremålet är utrustat eller inte
        if (itemToEquip == null)
        {
            return; // Inget att utrusta
        }

        // Hitta motsvarande prefab för detta ItemData
        GameObject prefabToInstantiate = FindPrefabForItem(itemToEquip);

        if (prefabToInstantiate != null && handHoldPoint != null)
        {
            // Skapa föremålet i handen
            currentEquippedObject = Instantiate(prefabToInstantiate, handHoldPoint.position, handHoldPoint.rotation, handHoldPoint);
            currentEquippedItem = itemToEquip;

            // Justera position, rotation och skala
            currentEquippedObject.transform.localPosition = Vector3.zero;
            currentEquippedObject.transform.localRotation = Quaternion.identity;

            // Aktivera alla renderers
            Renderer[] renderers = currentEquippedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }

            Debug.Log($"Utrustade {itemToEquip.itemName} i spelarens hand");
        }
        else
        {
            Debug.LogWarning($"Kunde inte hitta prefab för {itemToEquip.itemName} eller handHoldPoint är null");
        }
    }

    // Hitta prefab som matchar ItemData
    private GameObject FindPrefabForItem(ItemData item)
    {
        foreach (var mapping in equippablePrefabs)
        {
            if (mapping.itemData == item)
            {
                return mapping.prefab;
            }
        }

        Debug.LogWarning($"Ingen prefab hittades för {item.itemName}");
        return null;
    }

    // Använd det utrustade föremålet
    public void UseEquippedItem()
    {
        if (currentEquippedObject != null)
        {
            // Implementera användningslogik här baserat på föremålstyp

            // Exempel: Spela animation
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("UseItem");
            }

            Debug.Log($"Använde {currentEquippedItem.itemName}");
        }
    }
}

// Klass för att mappa ItemData till prefab
[System.Serializable]
public class EquippablePrefabMapping
{
    public ItemData itemData;
    public GameObject prefab;
}