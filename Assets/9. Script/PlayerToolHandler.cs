using UnityEngine;
using System.Collections.Generic;
using InventoryAndCrafting;

public class PlayerToolHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform primaryToolHoldPoint;    // Höger hand
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("Active Tool")]
    [SerializeField] private GameObject currentToolObject;
    private ItemData currentToolItemData;

    [System.Serializable]
    public class ToolPrefabMapping
    {
        public ItemData itemData;
        public GameObject prefab;
    }

    [Header("Tool Prefabs")]
    public List<ToolPrefabMapping> toolPrefabMappings = new List<ToolPrefabMapping>();

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private void Start()
    {
        // Säkerställ referenser
        if (equipmentManager == null)
            equipmentManager = EquipmentManager.Instance;

        // Hitta primaryToolHoldPoint om den inte är tilldelad
        if (primaryToolHoldPoint == null)
        {
            primaryToolHoldPoint = transform.Find("RightHand") ??
                                transform.Find("Hand") ??
                                CreateDefaultToolHoldPoint();
        }

        // Registrera händelser
        if (equipmentManager != null)
        {
            equipmentManager.onEquipmentChanged += UpdateEquippedTool;
        }
        else
        {
            Debug.LogError("EquipmentManager kunde inte hittas eller är null!");
        }

        // Initial uppdatering
        UpdateEquippedTool();
    }

    private Transform CreateDefaultToolHoldPoint()
    {
        GameObject holdPoint = new GameObject("RightHandHoldPoint");
        holdPoint.transform.SetParent(transform);
        holdPoint.transform.localPosition = new Vector3(0.5f, 0, 0.3f);
        return holdPoint.transform;
    }

    private void UpdateEquippedTool()
    {
        if (equipmentManager == null)
        {
            if (showDebugMessages)
                Debug.LogError("EquipmentManager är null! Se till att det finns en referens.");
            return;
        }

        var allSlots = equipmentManager.GetAllSlots();

        if (allSlots == null || allSlots.Count == 0)
        {
            if (showDebugMessages)
                Debug.LogError("Inga equipment slots hittades! Kontrollera EquipmentManager.");
            return;
        }

        EquipmentSlot toolSlot = null;

        // Hitta rätt verktygsslot
        foreach (var slot in allSlots)
        {
            if (slot == null) continue;

            bool isToolSlot =
                slot.slotType == EquipmentSlotType.MainHand ||
                slot.slotType == EquipmentSlotType.WorkTool;

            if (isToolSlot && slot.CurrentItem != null && IsItemATool(slot.CurrentItem))
            {
                toolSlot = slot;
                if (showDebugMessages)
                    Debug.Log($"Hittade verktygsslot med {slot.CurrentItem.itemName}");
                break;
            }
        }

        // Hantera verktygsutplacering
        if (toolSlot == null || toolSlot.CurrentItem == null)
        {
            if (showDebugMessages)
                Debug.Log("Inget verktyg hittades i utrustningsslots, tar bort nuvarande verktyg.");
            RemoveCurrentTool();
            return;
        }

        // Kontrollera om samma verktyg redan är utrustat
        if (currentToolItemData == toolSlot.CurrentItem) return;

        // Ta bort nuvarande verktyg och utrusta nytt
        RemoveCurrentTool();
        EquipTool(toolSlot.CurrentItem);
    }

    private bool IsItemATool(ItemData item)
    {
        return item != null && (
            item.itemType == ItemType.Tool ||
            item.toolType != InventoryAndCrafting.ToolType.None ||
            toolPrefabMappings.Exists(m => m.itemData == item)
        );
    }

    private void EquipTool(ItemData toolItem)
    {
        if (toolItem == null)
        {
            Debug.LogError("toolItem är null!");
            return;
        }

        if (primaryToolHoldPoint == null)
        {
            Debug.LogError("primaryToolHoldPoint är null! Skapar en ny.");
            primaryToolHoldPoint = CreateDefaultToolHoldPoint();
        }

        GameObject toolPrefab = GetToolPrefabForItem(toolItem);

        if (toolPrefab != null)
        {
            if (showDebugMessages)
                Debug.Log($"Skapar verktyg: {toolItem.itemName} från prefab {toolPrefab.name}");

            // Instantiate verktyget på rätt position
            currentToolObject = Instantiate(toolPrefab, primaryToolHoldPoint);
            currentToolItemData = toolItem;

            // Säkerställ att verktyget har en ToolSystem-komponent
            ToolSystem toolSystem = currentToolObject.GetComponent<ToolSystem>();
            if (toolSystem == null)
            {
                toolSystem = currentToolObject.AddComponent<ToolSystem>();
                if (showDebugMessages)
                    Debug.Log("Lade till ToolSystem på verktyget eftersom det saknades.");

                // Konvertera från InventoryAndCrafting.ToolType till ToolType enum
                toolSystem.toolType = ToolSystem.ConvertFromInventoryToolType(toolItem.toolType);
            }

            // Initialisera verktyget med spelaren som referens
            toolSystem.Initialize(transform);

            if (showDebugMessages)
                Debug.Log($"Verktyg utrustat: {toolItem.itemName}");
        }
        else
        {
            Debug.LogError($"Kunde inte hitta verktygsprefab för {toolItem.itemName}!");
        }
    }

    private GameObject GetToolPrefabForItem(ItemData item)
    {
        // Sök i mappningen först
        var mapping = toolPrefabMappings.Find(m => m.itemData == item);
        if (mapping?.prefab != null)
        {
            if (showDebugMessages)
                Debug.Log($"Hittade mappning för {item.itemName} i toolPrefabMappings");
            return mapping.prefab;
        }

        // Fallback till itemPrefab
        if (item.itemPrefab != null)
        {
            if (showDebugMessages)
                Debug.Log($"Använder itemPrefab för {item.itemName}");
            return item.itemPrefab;
        }

        Debug.LogError($"Ingen prefab hittades för {item.itemName}! Se till att antingen itemPrefab är satt i ItemData eller att verktyget finns i toolPrefabMappings.");

        // Skapa en enkel fallback-kub om inget annat finns
        GameObject fallbackCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallbackCube.name = $"Fallback_{item.itemName}";
        fallbackCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);

        return fallbackCube;
    }

    private void RemoveCurrentTool()
    {
        if (currentToolObject != null)
        {
            // Meddela verktyget att det blir oequipat
            ToolSystem toolSystem = currentToolObject.GetComponent<ToolSystem>();
            if (toolSystem != null)
            {
                toolSystem.OnUnequipped();
            }

            Destroy(currentToolObject);
            currentToolObject = null;
            currentToolItemData = null;

            if (showDebugMessages)
                Debug.Log("Verktyg borttaget");
        }
    }

    private void Update()
    {
        // Lyssna efter input för att använda verktyget
        if (Input.GetMouseButtonDown(0) && currentToolObject != null)
        {
            UseTool();
        }
    }

    public void UseTool()
    {
        // Använd ToolSystem direkt om det finns på verktyget
        ToolSystem toolSystem = currentToolObject.GetComponent<ToolSystem>();
        if (toolSystem != null)
        {
            bool success = toolSystem.UseTool();

            if (showDebugMessages)
                Debug.Log($"Använder verktyg: {toolSystem.toolType}, resultat: {success}");
        }
        else if (showDebugMessages)
        {
            Debug.LogError("Kunde inte hitta ToolSystem-komponenten på det aktuella verktyget!");
        }
    }

    private void OnDestroy()
    {
        // Rensa referenser för att undvika memory leaks
        if (equipmentManager != null)
        {
            equipmentManager.onEquipmentChanged -= UpdateEquippedTool;
        }

        RemoveCurrentTool();
    }
}