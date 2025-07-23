using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using InventoryAndCrafting;

public class Hotbar : MonoBehaviour, IDropHandler
{
    public List<Image> hotbarSlots = new List<Image>();
    public List<TMP_Text> hotbarNumbers = new List<TMP_Text>();
    public List<ItemData> hotbarItems = new List<ItemData>();
    public List<GameObject> slotObjects = new List<GameObject>();

    [Header("Slot Colors")]
    public Color emptySlotColor = Color.gray;  // Färg för tomma slots
    public Color filledSlotColor = Color.green;  // Färg för slots med items
    public Color selectedSlotColor = Color.red;  // Färg för den valda sloten

    private int selectedSlot = 0;

    void Start()
    {
        // Initiera hotbarItems-listan med null värden
        while (hotbarItems.Count < hotbarSlots.Count)
        {
            hotbarItems.Add(null);
            slotObjects.Add(null);
        }

        UpdateHotbarUI();
        ActivateSlotObject();
    }

    void Update()
    {
        HandleHotbarInput();
        HandleScrollInput();
    }

    void HandleHotbarInput()
    {
        // Använd GetKeyDown för alla sifferknappar
        for (int i = 0; i < 10; i++)
        {
            KeyCode keyCode = i == 9 ? KeyCode.Alpha0 : KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(keyCode))
            {
                SelectSlot(i == 9 ? 9 : i);
                break;
            }
        }
    }

    void HandleScrollInput()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
        {
            // Hitta alla icke-tomma slots
            List<int> occupiedSlots = new List<int>();
            for (int i = 0; i < hotbarItems.Count; i++)
            {
                if (hotbarItems[i] != null)
                {
                    occupiedSlots.Add(i);
                }
            }

            // Om inga slots är fyllda, avbryt
            if (occupiedSlots.Count == 0)
                return;

            // Hitta index för nuvarande valda slot i occupiedSlots-listan
            int currentIndex = occupiedSlots.IndexOf(selectedSlot);

            // Scrolla genom fyllda slots
            if (scrollDelta > 0)
            {
                // Nästa slot
                currentIndex = (currentIndex + 1) % occupiedSlots.Count;
            }
            else
            {
                // Föregående slot
                currentIndex = (currentIndex - 1 + occupiedSlots.Count) % occupiedSlots.Count;
            }

            // Uppdatera selectedSlot till den nya sloten
            selectedSlot = occupiedSlots[currentIndex];

            UpdateHotbarUI();
            ActivateSlotObject();
        }
    }

    void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            selectedSlot = slotIndex;
            UpdateHotbarUI();
            ActivateSlotObject();
        }
    }

    void UpdateHotbarUI()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image slotImage = hotbarSlots[i];
            slotImage.enabled = true;

            // Välj färg baserat på slot-status
            if (hotbarItems[i] != null)
            {
                // Endast färgändring för slots med items
                if (i == selectedSlot)
                {
                    slotImage.color = selectedSlotColor;
                }
                else
                {
                    slotImage.color = filledSlotColor;
                }

                // Uppdatera ikon
                slotImage.sprite = hotbarItems[i].itemIcon;
            }
            else
            {
                // Tomma slots förblir i emptySlotColor
                slotImage.color = emptySlotColor;
                slotImage.sprite = null;
            }

            // Uppdatera siffror
            hotbarNumbers[i].text = (i == 9) ? "0" : (i + 1).ToString();
        }
    }

    void ActivateSlotObject()
    {
        for (int i = 0; i < slotObjects.Count; i++)
        {
            if (slotObjects[i] != null)
            {
                slotObjects[i].SetActive(i == selectedSlot);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Hämta den item som dras från inventoryt
        InventorySlot inventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();

        if (inventorySlot != null && inventorySlot.CurrentItem != null)
        {
            // Hitta vilken hotbar-slot som items har dragits till
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    hotbarSlots[i].rectTransform,
                    eventData.position,
                    eventData.pressEventCamera))
                {
                    // Lägg till item i hotbaren
                    hotbarItems[i] = inventorySlot.CurrentItem;
                    UpdateHotbarUI();
                    break;
                }
            }
        }
    }

    // Metod för att hämta item i den valda sloten
    public ItemData GetSelectedItem()
    {
        return hotbarItems[selectedSlot];
    }
}