using UnityEngine;

public class ExplorationTrigger : MonoBehaviour
{
    [Header("Location Settings")]
    public string locationName = "Unnamed Location";
    [Multiline]
    public string locationDescription = "An interesting place to explore.";
    public bool showLocationNameOnEnter = true;

    [Header("Quest Settings")]
    public bool isQuestLocation = true;

    [Header("Visual Effects")]
    public GameObject discoveryVFX;
    public float vfxDuration = 3f;

    private bool hasBeenExplored = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenExplored)
        {
            DiscoverLocation();
        }
    }

    private void DiscoverLocation()
    {
        hasBeenExplored = true;

        // Visa platsnamn om aktiverat
        if (showLocationNameOnEnter)
        {
            // Här kan du implementera ett UI-system för att visa platsnamn
            Debug.Log($"Plats upptäckt: {locationName}");
        }

        // Spela upp VFX om det finns
        if (discoveryVFX != null)
        {
            GameObject vfx = Instantiate(discoveryVFX, transform.position, Quaternion.identity);
            Destroy(vfx, vfxDuration);
        }

        // Uppdatera quest-framsteg
        if (isQuestLocation)
        {
            QuestManager.Instance?.UpdateQuestProgress("explore", locationName, 1);
        }
        if (showLocationNameOnEnter)
        {
            // Använd UI-systemet för att visa platsinformation
            LocationDiscoveryUI.Instance?.ShowLocationDiscovery(locationName, locationDescription);
        }
    }

    // För att återställa utforskning (för debuggning)
    public void ResetExploration()
    {
        hasBeenExplored = false;
    }

    // Visualisera trigger-området i editorn
    private void OnDrawGizmos()
    {
        Gizmos.color = hasBeenExplored ? Color.green : Color.blue;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (sphereCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
        }
        else
        {
            // Default visualization om ingen collider finns
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }


}