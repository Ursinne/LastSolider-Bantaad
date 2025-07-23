using UnityEngine;
using System.Collections;
using InventoryAndCrafting;

public class ResourceNode : MonoBehaviour
{
    public enum ResourceType
    {
        Wood,
        Stone,
        Iron,
        Food,
        Soil,
        Fish,
        Water
    }

    [Header("Resource Settings")]
    public ResourceType resourceType;
    public float resourceAmount = 100f;
    public float maxResourceAmount = 100f;
    public float respawnTime = 300f; // Sekunder tills resursen återväxer (5 minuter)
    public bool canRespawn = true;

    [Header("Harvest Settings")]
    public InventoryAndCrafting.ToolType requiredToolType = InventoryAndCrafting.ToolType.None; // Vilket verktyg som krävs
    public float harvestEfficiency = 1.0f; // Effektivitet för skörd
    public float harvestThreshold = 0.0f; // Minimivärde som krävs för att skörda

    [Header("Drop Settings")]
    public ItemData resourceItem; // Referens till ItemData från InventorySystem
    public int minDropAmount = 1;
    public int maxDropAmount = 3;

    private bool isHarvestable = true;
    private float respawnTimer = 0f;
    private float lastHarvestTime = 0f;
    private float harvestCooldown = 0.5f; // Förhindra för snabb harvesting

    [Header("Visual Feedback")]
    public GameObject harvestEffectPrefab;
    public AudioClip harvestSound;
    private AudioSource audioSource;

    private void Awake()
    {
        // Lägg till AudioSource om den inte redan finns
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && harvestSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D ljud
            audioSource.maxDistance = 20f;
        }
    }

    private void Update()
    {
        // Hanterar respawn av resursen
        if (!isHarvestable && canRespawn)
        {
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
        }
    }

    public void Harvest(ToolSystem tool)
    {
        if (!isHarvestable) return;

        // Kontrollera om det har gått tillräckligt lång tid sedan senaste harvesting
        if (Time.time - lastHarvestTime < harvestCooldown)
        {
            return;
        }

        lastHarvestTime = Time.time;

        // Kontrollera om rätt verktyg används
        InventoryAndCrafting.ToolType toolType = tool.GetInventoryToolType();
        bool isCompatibleTool = (requiredToolType == InventoryAndCrafting.ToolType.None || toolType == requiredToolType);

        if (!isCompatibleTool)
        {
            Debug.Log($"Behöver {requiredToolType} för att skörda denna resurs! Nuvarande verktyg: {toolType}");
            return;
        }

        // Beräkna skördemängd baserat på verktygets effektivitet
        float harvestAmount = tool.harvestAmount * harvestEfficiency;

        // Minska resursens mängd
        resourceAmount -= harvestAmount;

        // Spela harvesting ljud
        if (audioSource != null && harvestSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Lägg till variation
            audioSource.PlayOneShot(harvestSound);
        }

        // Spela visuell effekt
        if (harvestEffectPrefab != null)
        {
            GameObject effect = Instantiate(harvestEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(effect, 2f); // Ta bort efter 2 sekunder
        }

        // Ge resurser till spelaren
        if (resourceAmount <= harvestThreshold)
        {
            GiveResourcesToPlayer();

            // Om resursen är slut, hantera det
            if (resourceAmount <= 0)
            {
                isHarvestable = false;
                respawnTimer = 0f;

                // Uppdatera visuell representation
                UpdateVisuals();
            }
        }

        Debug.Log($"Skördade {harvestAmount} {resourceType} från resursnoden. Återstår: {resourceAmount}");
    }

    private void GiveResourcesToPlayer()
    {
        if (resourceItem == null)
        {
            Debug.LogWarning("ResourceItem är inte tilldelad i " + gameObject.name);
            return;
        }

        // Bestäm hur mycket som droppas
        int dropAmount = Random.Range(minDropAmount, maxDropAmount + 1);

        // Hämta spelarens inventory och lägg till item
        if (InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(resourceItem, dropAmount);

            if (added)
            {
                // Visa notifikation
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowItemNotification(resourceItem, dropAmount);
                }

                // Uppdatera quest-framsteg
                QuestManager.Instance?.UpdateQuestProgress("gather", resourceItem.itemName, dropAmount);

                Debug.Log($"La till {dropAmount}x {resourceItem.itemName} i spelarens inventory");
            }
            else
            {
                Debug.LogWarning("Kunde inte lägga till item i inventory. Kanske är det fullt?");

                // Droppa objekt i världen om inventory är fullt
                DropItemInWorld(dropAmount);
            }
        }
        else
        {
            Debug.LogWarning("Kunde inte hitta InventoryManager!");

            // Fallback: Droppa objekt i världen
            DropItemInWorld(dropAmount);
        }
    }

    private void DropItemInWorld(int amount)
    {
        if (resourceItem?.itemPrefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 1.5f;
            randomOffset.y = 0.5f; // Håll ovanför marknivå

            Vector3 spawnPos = transform.position + randomOffset;
            GameObject item = Instantiate(resourceItem.itemPrefab, spawnPos, Quaternion.identity);

            // Lägg till lite fysik för att få det att ramla naturligt
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = item.AddComponent<Rigidbody>();
            }

            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }
    }

    private void UpdateVisuals()
    {
        // Exempel: Skala ner objektet för att visa att resursen är slut
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Eller ändra material till en "harvested" version
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Ändra färg till grå för att visa att den är tömd
            renderer.material.color = Color.gray;
        }
    }

    private void Respawn()
    {
        resourceAmount = maxResourceAmount;
        isHarvestable = true;
        respawnTimer = 0f;

        // Återställ visuell representation
        transform.localScale = Vector3.one; // Eller originalskalan

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Återställ färgen
            renderer.material.color = Color.white; // Eller originalfärgen
        }

        Debug.Log($"{resourceType}-resurs har återväxt!");
    }
}