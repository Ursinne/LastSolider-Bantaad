using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PickupDisplay : MonoBehaviour
{
    public TextMeshProUGUI pickupText;
    public Image pickupIcon;
    public float displayDuration = 4f;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    private void Start()
    {
        Debug.Log("PickupDisplay started");
        pickupText.gameObject.SetActive(false);
        pickupIcon.gameObject.SetActive(false);
    }

    public void ShowPickup(string itemName, Sprite itemIcon)
    {
        Debug.Log($"ShowPickup called with itemName: {itemName} and itemIcon: {itemIcon}");
        StopAllCoroutines(); // Stopp alla pågående coroutine för att visa den nya informationen

        if (itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName]++;
        }
        else
        {
            itemCounts[itemName] = 1;
        }

        pickupText.text = "You found: " + itemName + " (x" + itemCounts[itemName] + ")";

        if (itemIcon != null)
        {
            Debug.Log($"Setting icon for item: {itemName}");
            pickupIcon.sprite = itemIcon;
            pickupIcon.enabled = true;
        }
        else
        {
            Debug.LogWarning($"No icon provided for item: {itemName}");
            pickupIcon.sprite = null;
            pickupIcon.enabled = false;
        }

        Debug.Log("Activating text and icon");
        pickupText.gameObject.SetActive(true);
        pickupIcon.gameObject.SetActive(true);
        StartCoroutine(HidePickup());
    }

    private IEnumerator HidePickup()
    {
        yield return new WaitForSeconds(displayDuration);
        Debug.Log("Hiding text and icon");
        pickupText.gameObject.SetActive(false);
        pickupIcon.gameObject.SetActive(false);
    }
}
