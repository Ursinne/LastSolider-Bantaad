using UnityEngine;
using InventoryAndCrafting;

public class PickupTool : MonoBehaviour
{
    public ItemData itemData; // Referens till ItemData f�r verktyget
    public float pickupRange = 5f;
    public KeyCode pickupKey = KeyCode.F;
    
    [Header("Animation Settings")]
    public string pickupAnimationTrigger = "isPickup"; // Namn p� animation-trigger
    public string pickupAnimationBool = "isPickingUp"; // Alternativ animation som bool
    public float animationDuration = 1.0f; // Hur l�nge animationen varar
    
    // Hur l�nge vi v�ntar efter spelets start innan vi f�rs�ker anv�nda InventoryManager
    private float startupDelay = 1.0f;
    private float timeSinceStartup = 0f;
    private bool startupComplete = false;

    private void Start()
    {
        // Kontrollera om ItemData �r satt
        if (itemData == null)
        {
            Debug.LogError($"ItemData inte tilldelad p� {gameObject.name}! Tilldela detta i inspektorn.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Se till att itemData �r satt innan vi anv�nder den
            if (itemData != null)
            {
                Debug.Log("Du kan plocka upp " + itemData.itemName + ". Tryck p� F f�r att plocka upp.");
            }
            else
            {
                Debug.Log("Du kan plocka upp detta verktyg. Tryck p� F f�r att plocka upp.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Spelaren l�mnade omr�det
        }
    }

    private void Update()
    {
        // V�nta tills startf�rdr�jningen �r �ver innan vi anv�nder systemet
        if (!startupComplete)
        {
            timeSinceStartup += Time.deltaTime;
            if (timeSinceStartup >= startupDelay)
            {
                startupComplete = true;
                
                // Kontrollera om InventoryManager har initialiserats
                if (InventoryManager.Instance == null)
                {
                    Debug.LogWarning("InventoryManager.Instance �r null efter startup-delay. " +
                                   "Kontrollera att InventoryManager har initialiserats korrekt.");
                }
            }
            return; // Avbryt Update under startupf�rdr�jningen
        }
        
        if (Input.GetKeyDown(pickupKey))
        {
            // Hitta spelaren
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Kunde inte hitta spelaren f�r avst�ndsber�kning.");
                return;
            }
            
            float distance = Vector3.Distance(transform.position, player.transform.position);
            Debug.Log($"Avst�nd till {gameObject.name}: {distance:F2} meter, pickupRange: {pickupRange}");

            // Kolla om spelaren �r tillr�ckligt n�ra f�r att plocka upp verktyget
            // Kolla om spelaren �r tillr�ckligt n�ra f�r att plocka upp verktyget
            if (distance <= pickupRange + 0.1f)  // L�gg till en liten tolerans p� 0.1
            {
                Pickup(player);
            }
            else
            {
                Debug.Log($"F�r l�ngt borta f�r att plocka upp {gameObject.name}. Avst�nd: {distance:F2}, m�ste vara mindre �n {pickupRange}");
            }
        }
    }

    private void Pickup(GameObject player)
    {
        // S�kerst�ll att vi har ett ItemData-objekt
        if (itemData == null)
        {
            Debug.LogError("ItemData �r null p� " + gameObject.name + "! Kan inte plocka upp.");
            return;
        }

        // S�kerst�ll att InventoryManager finns
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance �r null! Kontrollera att InventoryManager finns i scenen.");
            return;
        }

        Debug.Log("F�rs�ker plocka upp " + itemData.itemName);

        // Spela upp pickup-animation p� spelaren
        PlayPickupAnimation(player);

        // Anv�nd InventoryManager f�r att l�gga till f�rem�let
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
                Debug.Log("NotificationManager.Instance �r null, men verktyget lades till i inventoriet �nd�.");
            }

            // Uppdatera quest-information om QuestManager finns
            QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, 1);

            // Ta bort f�rem�let fr�n v�rlden
            Destroy(gameObject);
            Debug.Log("Plockade upp och lade till " + itemData.name + " i inventoriet");
        }
        else
        {
            Debug.Log("Kunde inte l�gga till " + itemData.name + " i inventoriet. Kanske �r det fullt?");
        }
    }

    private void PlayPickupAnimation(GameObject player)
    {
        // F�rs�k f�rst med PlayerAnimationController
        PlayerAnimationController animController = player.GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            // Anv�nd bool-animation om den �r tillg�nglig
            if (!string.IsNullOrEmpty(pickupAnimationBool))
            {
                animController.PlayAnimation(pickupAnimationBool, animationDuration);
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController: {pickupAnimationBool}");
                return;
            }
            
            // Annars anv�nd trigger
            bool success = animController.TriggerAnimation(pickupAnimationTrigger);
            if (success)
            {
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController trigger: {pickupAnimationTrigger}");
                return;
            }
        }
        
        // Fallback till att anv�nda Animator direkt
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
            playerAnimator = player.GetComponentInChildren<Animator>();
            
        if (playerAnimator != null)
        {
            // F�rs�k med trigger f�rst
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
                    // S�tt bool, �terst�ll efter en kort stund
                    playerAnimator.SetBool(pickupAnimationBool, true);
                    StartCoroutine(ResetAnimationBool(playerAnimator, pickupAnimationBool, animationDuration));
                    Debug.Log($"Spelar pickup-animation via Animator bool: {pickupAnimationBool}");
                    return;
                }
            }
            
            // F�rs�k med generisk "Pickup" trigger om den finns
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
            
            Debug.LogWarning("Kunde inte hitta n�gon l�mplig animation f�r pickup. " +
                          "L�gg till parametern 'Pickup' (trigger) eller 'isPickingUp' (bool) till din Animator.");
        }
        else
        {
            Debug.LogWarning("Kunde inte hitta Animator p� spelaren. Ingen pickup-animation spelades.");
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
//    public ItemData itemData; // Referens till ItemData f�r verktyget
//    public float pickupRange = 3f;
//    public KeyCode pickupKey = KeyCode.F;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            Debug.Log("Du kan plocka upp " + itemData.itemName + ". Tryck p� F f�r att plocka upp.");
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            // Spelaren l�mnade omr�det
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(pickupKey))
//        {
//            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);

//            // Kolla om spelaren �r tillr�ckligt n�ra f�r att plocka upp verktyget
//            if (distance < pickupRange)
//            {
//                Pickup();
//            }
//            else
//            {
//                //Debug.Log("Du �r f�r l�ngt borta f�r att plocka upp " + itemData.itemName);
//            }
//        }
//    }

//    private void Pickup()
//    {
//        //Debug.Log("F�rs�ker plocka upp " + itemData.itemName);

//        // Anv�nd InventoryManager f�r att l�gga till f�rem�let
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

//                // Ta bort f�rem�let fr�n v�rlden
//                Destroy(gameObject);
//                Debug.Log("Plockade upp och lade till " + itemData.itemName + " i inventoriet");
//            }
//            else
//            {
//                Debug.Log("Kunde inte l�gga till " + itemData.itemName + " i inventoriet. Kanske �r det fullt?");
//            }
//        }
//        else
//        {
//            Debug.LogError("ItemData eller InventoryManager saknas!");
//        }
//    }
//}