using UnityEngine;
using System.Collections.Generic;
using InventoryAndCrafting;

public class EquippableItemSystem : MonoBehaviour
{
    [Header("References")]
    public Transform handHoldPoint; // Position d�r verktyget visas i spelarens hand

    [Header("Prefab References")]
    public List<EquippablePrefabMapping> equippablePrefabs = new List<EquippablePrefabMapping>();

    private GameObject currentEquippedObject; // Referens till verktyget som h�lls just nu
    private ItemData currentEquippedItem; // Referens till ItemData f�r nuvarande verktyg

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
        // Lyssna p� h�ndelser fr�n hotbar eller inventory
        if (InventoryManager.Instance != null)
        {
            // Om du har en event f�r n�r ett f�rem�l v�ljs i hotbar, prenumerera p� det h�r
            // InventoryManager.Instance.onItemSelected += OnItemSelected;
        }
    }

    // Denna metod anropas n�r ett f�rem�l v�ljs i hotbar
    public void EquipItem(ItemData itemToEquip)
    {
        // Avutrusta nuvarande f�rem�l om n�got �r utrustat
        if (currentEquippedObject != null)
        {
            Destroy(currentEquippedObject);
            currentEquippedObject = null;
            currentEquippedItem = null;
        }

        // Kontrollera om f�rem�let �r utrustat eller inte
        if (itemToEquip == null)
        {
            return; // Inget att utrusta
        }

        // Hitta motsvarande prefab f�r detta ItemData
        GameObject prefabToInstantiate = FindPrefabForItem(itemToEquip);

        if (prefabToInstantiate != null && handHoldPoint != null)
        {
            // Skapa f�rem�let i handen
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
            Debug.LogWarning($"Kunde inte hitta prefab f�r {itemToEquip.itemName} eller handHoldPoint �r null");
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

        Debug.LogWarning($"Ingen prefab hittades f�r {item.itemName}");
        return null;
    }

    // Anv�nd det utrustade f�rem�let
    public void UseEquippedItem()
    {
        if (currentEquippedObject != null)
        {
            // Implementera anv�ndningslogik h�r baserat p� f�rem�lstyp

            // Exempel: Spela animation
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("UseItem");
            }

            Debug.Log($"Anv�nde {currentEquippedItem.itemName}");
        }
    }
}

// Klass f�r att mappa ItemData till prefab
[System.Serializable]
public class EquippablePrefabMapping
{
    public ItemData itemData;
    public GameObject prefab;
}