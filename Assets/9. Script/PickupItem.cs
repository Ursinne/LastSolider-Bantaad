using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using InventoryAndCrafting;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData; // Referens till ItemData
    public int amount = 1;
    public float pickupRange = 3f; // Pickup avstånd
    public KeyCode pickupKey = KeyCode.F; // Tangent för att plocka upp

    [Header("Animation Settings")]
    public string pickupAnimationTrigger = "Pickup"; // Namn på animation-trigger
    public string pickupAnimationBool = "isPickingUp"; // Alternativ animation som bool
    public float animationDuration = 1.0f; // Hur länge animationen varar

    [Header("Feedback")]
    public AudioClip pickupSound; // Ljud när man plockar upp
    public GameObject pickupEffectPrefab; // Visuell effekt när man plockar upp

    [Header("Debug Settings")]
    public bool showDebugInfo = true; // Sätt till false i produktion

    private AudioSource audioSource;
    private Transform playerTransform; // Cachelagrar spelarens transform för bättre prestanda

    private void Start()
    {
        // Lägg till AudioSource om den inte redan finns
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D ljud
            audioSource.maxDistance = 10f;
            audioSource.volume = 0.7f;
        }
        
        // Försök hitta spelaren vid start och cachelagra referensen
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
                Debug.Log($"Spelare inom trigger-räckvidd för {gameObject.name}");

            // Visa en tooltip eller interaktionstext om du vill
            // UI_InteractionManager.Instance?.ShowInteractionText($"Press {pickupKey} to pick up {itemData?.itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
                Debug.Log($"Spelare lämnade trigger-räckvidd för {gameObject.name}");
                
            // Dölj interaktionstext när spelaren går bort
            // UI_InteractionManager.Instance?.HideInteractionText();
        }
    }

    private void Update()
    {
        // Kontrollera om spelaren fortfarande finns
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return; // Ingen spelare hittad
            }
        }

        // Beräkna och visa avståndet varje frame för debugging
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Visa alltid avstånd när Shift-tangenten hålls ner (för debugging)
        if (showDebugInfo && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log($"Avstånd till {gameObject.name}: {distance:F2} meter, pickupRange: {pickupRange}");
        }

        if (Input.GetKeyDown(pickupKey)) // tangent för att plocka upp
        {
            // Lägg till mer detaljerad loggning för felsökning
            if (showDebugInfo)
            {
                Debug.Log($"Försöker plocka upp {gameObject.name}. Avstånd: {distance:F2}, pickupRange: {pickupRange}, Kan plocka upp: {distance < pickupRange}");
            }

            // Kolla om spelaren är nära nog att plocka upp föremålet
            if (distance < pickupRange)
            {
                if (showDebugInfo)
                    Debug.Log($"Plockar upp {gameObject.name} på avstånd {distance:F2}");
                
                Pickup(playerTransform.gameObject);
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log($"För långt borta för att plocka upp {gameObject.name}. Avstånd: {distance:F2}, måste vara mindre än {pickupRange}");
            }
        }
    }

    // Uppdaterade Pickup-metoden som tar emot spelarobjektet
    private void Pickup(GameObject player)
    {
        if (showDebugInfo)
            Debug.Log($"Plockar upp: {itemData?.itemName}");

        // Spela upp animation
        PlayPickupAnimation(player);

        // Spela upplock-ljud om det finns
        if (audioSource != null && pickupSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Lägg till lite variation
            audioSource.PlayOneShot(pickupSound);
        }

        // Skapa visuell effekt om det finns en sådan
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Ta bort efter 2 sekunder
        }

        // Använd InventoryManager för att lägga till föremålet
        if (itemData != null && InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemData, amount);
            if (added)
            {
                QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, amount);

                // Visa notifikation via NotificationManager
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowItemNotification(itemData, amount);
                    
                    if (showDebugInfo)
                        Debug.Log("Notifikation visad via NotificationManager");
                }
                else
                {
                    Debug.LogWarning("NotificationManager.Instance är null!");
                }

                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("ItemData eller InventoryManager saknas!");
        }
    }

    private void PlayPickupAnimation(GameObject player)
    {
        // Försök först med PlayerAnimationController om det finns
        PlayerAnimationController animController = player.GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            // Använd bool-animation om den är tillgänglig
            if (!string.IsNullOrEmpty(pickupAnimationBool))
            {
                animController.PlayAnimation(pickupAnimationBool, animationDuration);
                
                if (showDebugInfo)
                    Debug.Log($"Spelar pickup-animation via PlayerAnimationController: {pickupAnimationBool}");
                
                return;
            }

            // Annars använd trigger
            bool success = animController.TriggerAnimation(pickupAnimationTrigger);
            if (success)
            {
                if (showDebugInfo)
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
                    
                    if (showDebugInfo)
                        Debug.Log($"Spelar pickup-animation via Animator trigger: {pickupAnimationTrigger}");
                    
                    return;
                }
                else if (param.name == pickupAnimationBool && param.type == AnimatorControllerParameterType.Bool)
                {
                    // Sätt bool, återställ efter en kort stund
                    playerAnimator.SetBool(pickupAnimationBool, true);
                    StartCoroutine(ResetAnimationBool(playerAnimator, pickupAnimationBool, animationDuration));
                    
                    if (showDebugInfo)
                        Debug.Log($"Spelar pickup-animation via Animator bool: {pickupAnimationBool}");
                    
                    return;
                }
            }

            // Försök med generisk "Pickup" trigger om den finns
            foreach (string genericTrigger in new[] { "Pickup", "PickUp", "PickItem", "GetItem", "Take", "Grab" })
            {
                foreach (AnimatorControllerParameter param in playerAnimator.parameters)
                {
                    if (param.name == genericTrigger && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        playerAnimator.SetTrigger(genericTrigger);
                        
                        if (showDebugInfo)
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
    
    // Visualisera pickup-rangen i Unity Editor
    void OnDrawGizmos()
    {
        // Rita en grön sfär som visar pickup-rangen
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        
        // Rita en liten röd kub i objektets centrum för att visa exakt var avståndet mäts från
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
    }
}