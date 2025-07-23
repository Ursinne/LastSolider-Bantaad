using UnityEngine;
using InventoryAndCrafting;

public class PickupTool : MonoBehaviour
{
    public ItemData itemData; // Referens till ItemData för verktyget
    public float pickupRange = 5f;
    public KeyCode pickupKey = KeyCode.F;
    
    [Header("Animation Settings")]
    public string pickupAnimationTrigger = "isPickup"; // Namn på animation-trigger
    public string pickupAnimationBool = "isPickingUp"; // Alternativ animation som bool
    public float animationDuration = 1.0f; // Hur länge animationen varar
    
    // Hur länge vi väntar efter spelets start innan vi försöker använda InventoryManager
    private float startupDelay = 1.0f;
    private float timeSinceStartup = 0f;
    private bool startupComplete = false;

    private void Start()
    {
        // Kontrollera om ItemData är satt
        if (itemData == null)
        {
            Debug.LogError($"ItemData inte tilldelad på {gameObject.name}! Tilldela detta i inspektorn.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Se till att itemData är satt innan vi använder den
            if (itemData != null)
            {
                Debug.Log("Du kan plocka upp " + itemData.itemName + ". Tryck på F för att plocka upp.");
            }
            else
            {
                Debug.Log("Du kan plocka upp detta verktyg. Tryck på F för att plocka upp.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Spelaren lämnade området
        }
    }

    private void Update()
    {
        // Vänta tills startfördröjningen är över innan vi använder systemet
        if (!startupComplete)
        {
            timeSinceStartup += Time.deltaTime;
            if (timeSinceStartup >= startupDelay)
            {
                startupComplete = true;
                
                // Kontrollera om InventoryManager har initialiserats
                if (InventoryManager.Instance == null)
                {
                    Debug.LogWarning("InventoryManager.Instance är null efter startup-delay. " +
                                   "Kontrollera att InventoryManager har initialiserats korrekt.");
                }
            }
            return; // Avbryt Update under startupfördröjningen
        }
        
        if (Input.GetKeyDown(pickupKey))
        {
            // Hitta spelaren
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Kunde inte hitta spelaren för avståndsberäkning.");
                return;
            }
            
            float distance = Vector3.Distance(transform.position, player.transform.position);
            Debug.Log($"Avstånd till {gameObject.name}: {distance:F2} meter, pickupRange: {pickupRange}");

            // Kolla om spelaren är tillräckligt nära för att plocka upp verktyget
            // Kolla om spelaren är tillräckligt nära för att plocka upp verktyget
            if (distance <= pickupRange + 0.1f)  // Lägg till en liten tolerans på 0.1
            {
                Pickup(player);
            }
            else
            {
                Debug.Log($"För långt borta för att plocka upp {gameObject.name}. Avstånd: {distance:F2}, måste vara mindre än {pickupRange}");
            }
        }
    }

    private void Pickup(GameObject player)
    {
        // Säkerställ att vi har ett ItemData-objekt
        if (itemData == null)
        {
            Debug.LogError("ItemData är null på " + gameObject.name + "! Kan inte plocka upp.");
            return;
        }

        // Säkerställ att InventoryManager finns
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance är null! Kontrollera att InventoryManager finns i scenen.");
            return;
        }

        Debug.Log("Försöker plocka upp " + itemData.itemName);

        // Spela upp pickup-animation på spelaren
        PlayPickupAnimation(player);

        // Använd InventoryManager för att lägga till föremålet
        bool added = InventoryManager.Instance.AddItem(itemData, 1);

        if (added)
        {
            // Visa notifikation
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowItemNotification(itemData, 1);
                Debug.Log("Notifikation visad via NotificationManager");
            }
            else
            {
                Debug.Log("NotificationManager.Instance är null, men verktyget lades till i inventoriet ändå.");
            }

            // Uppdatera quest-information om QuestManager finns
            QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, 1);

            // Ta bort föremålet från världen
            Destroy(gameObject);
            Debug.Log("Plockade upp och lade till " + itemData.name + " i inventoriet");
        }
        else
        {
            Debug.Log("Kunde inte lägga till " + itemData.name + " i inventoriet. Kanske är det fullt?");
        }
    }

    private void PlayPickupAnimation(GameObject player)
    {
        // Försök först med PlayerAnimationController
        PlayerAnimationController animController = player.GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            // Använd bool-animation om den är tillgänglig
            if (!string.IsNullOrEmpty(pickupAnimationBool))
            {
                animController.PlayAnimation(pickupAnimationBool, animationDuration);
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController: {pickupAnimationBool}");
                return;
            }
            
            // Annars använd trigger
            bool success = animController.TriggerAnimation(pickupAnimationTrigger);
            if (success)
            {
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController trigger: {pickupAnimationTrigger}");
                return;
            }
        }
        
        // Fallback till att använda Animator direkt
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
            playerAnimator = player.GetComponentInChildren<Animator>();
            
        if (playerAnimator != null)
        {
            // Försök med trigger först
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == pickupAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                {
                    playerAnimator.SetTrigger(pickupAnimationTrigger);
                    Debug.Log($"Spelar pickup-animation via Animator trigger: {pickupAnimationTrigger}");
                    return;
                }
                else if (param.name == pickupAnimationBool && param.type == AnimatorControllerParameterType.Bool)
                {
                    // Sätt bool, återställ efter en kort stund
                    playerAnimator.SetBool(pickupAnimationBool, true);
                    StartCoroutine(ResetAnimationBool(playerAnimator, pickupAnimationBool, animationDuration));
                    Debug.Log($"Spelar pickup-animation via Animator bool: {pickupAnimationBool}");
                    return;
                }
            }
            
            // Försök med generisk "Pickup" trigger om den finns
            foreach (string genericTrigger in new[]{"Pickup", "PickUp", "PickItem", "GetItem", "Take", "Grab"})
            {
                foreach (AnimatorControllerParameter param in playerAnimator.parameters)
                {
                    if (param.name == genericTrigger && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        playerAnimator.SetTrigger(genericTrigger);
                        Debug.Log($"Spelar pickup-animation via generisk trigger: {genericTrigger}");
                        return;
                    }
                }
            }
            
            Debug.LogWarning("Kunde inte hitta någon lämplig animation för pickup. " +
                          "Lägg till parametern 'Pickup' (trigger) eller 'isPickingUp' (bool) till din Animator.");
        }
        else
        {
            Debug.LogWarning("Kunde inte hitta Animator på spelaren. Ingen pickup-animation spelades.");
        }
    }
    
    private System.Collections.IEnumerator ResetAnimationBool(Animator animator, string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetBool(paramName, false);
        }
    }
}

//using UnityEngine;
//using InventoryAndCrafting;

//public class PickupTool : MonoBehaviour
//{
//    public ItemData itemData; // Referens till ItemData för verktyget
//    public float pickupRange = 3f;
//    public KeyCode pickupKey = KeyCode.F;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            Debug.Log("Du kan plocka upp " + itemData.itemName + ". Tryck på F för att plocka upp.");
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            // Spelaren lämnade området
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(pickupKey))
//        {
//            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);

//            // Kolla om spelaren är tillräckligt nära för att plocka upp verktyget
//            if (distance < pickupRange)
//            {
//                Pickup();
//            }
//            else
//            {
//                //Debug.Log("Du är för långt borta för att plocka upp " + itemData.itemName);
//            }
//        }
//    }

//    private void Pickup()
//    {
//        //Debug.Log("Försöker plocka upp " + itemData.itemName);

//        // Använd InventoryManager för att lägga till föremålet
//        if (itemData != null && InventoryManager.Instance != null)
//        {
//            bool added = InventoryManager.Instance.AddItem(itemData, 1);

//            if (added)
//            {
//                // Visa notifikation via NotificationManager
//                if (NotificationManager.Instance != null)
//                {
//                    NotificationManager.Instance.ShowItemNotification(itemData, 1);
//                    Debug.Log("Notifikation visad via NotificationManager");
//                }

//                // Ta bort föremålet från världen
//                Destroy(gameObject);
//                Debug.Log("Plockade upp och lade till " + itemData.itemName + " i inventoriet");
//            }
//            else
//            {
//                Debug.Log("Kunde inte lägga till " + itemData.itemName + " i inventoriet. Kanske är det fullt?");
//            }
//        }
//        else
//        {
//            Debug.LogError("ItemData eller InventoryManager saknas!");
//        }
//    }
//}