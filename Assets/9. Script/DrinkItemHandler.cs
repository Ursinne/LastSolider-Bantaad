using UnityEngine;
using InventoryAndCrafting;

public class DrinkItemHandler : MonoBehaviour
{
    [Header("Drink Settings")]
    public KeyCode drinkKey = KeyCode.R; // Knapp för att dricka
    public string drinkAnimationName = "isDrinking"; // Animationsparameter
    public float drinkAnimationDuration = 2.0f; // Hur länge animationen spelas

    [Header("Item Tag")]
    public string drinkableItemTag = "Bottle"; // Tagg för drickbara föremål i inventariet

    [Header("Audio")]
    public AudioClip drinkSound; // Ljudeffekt när man dricker
    private AudioSource audioSource;

    // Referens till spelarens komponenter
    private PlayerAnimationController animController;
    private PlayerAttributes playerAttributes;

    void Start()
    {
        // Hitta nödvändiga komponenter
        animController = GetComponent<PlayerAnimationController>();
        playerAttributes = GetComponent<PlayerAttributes>();

        // Lägg till ljudkälla om det behövs
        if (drinkSound != null && GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Kontrollera om spelaren trycker på drinkKey (R som standard)
        if (Input.GetKeyDown(drinkKey))
        {
            TryDrinkFromInventory();
        }
    }

    void TryDrinkFromInventory()
    {
        // Kontrollera om InventoryManager finns
        if (InventoryManager.Instance == null) return;

        // Hitta en flaska/drickbar föremål i inventariet
        ItemData drinkableItem = FindDrinkableItemInInventory();

        if (drinkableItem != null)
        {
            // Använd flaskan
            UseDrinkableItem(drinkableItem);
        }
        else
        {
            Debug.Log("Du har inget att dricka!");
        }
    }

    ItemData FindDrinkableItemInInventory()
    {
        // Gå igenom inventariet och sök efter drickbart föremål
        foreach (var slot in InventoryManager.Instance.Slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                ItemData item = slot.GetItem();

                // Kontrollera om objektet är drickbart
                if (IsDrinkableItem(item))
                {
                    return item;
                }
            }
        }

        return null; // Inget drickbart objekt hittades
    }

    bool IsDrinkableItem(ItemData item)
    {
        // Kontrollera om föremålet är drickbart baserat på egenskaper
        if (item == null) return false;

        // Metod 1: Kontrollera på itemType
        if (item.itemType == ItemType.Survival && item.thirstRestore > 0)
            return true;

        // Metod 2: Kontrollera på toolType
        //if (item.toolType == ToolType.Bottle)
        //    return true;

        // Metod 3: Kontrollera på namn (fallback)
        if (item.itemName.ToLower().Contains("water") ||
            item.itemName.ToLower().Contains("drink") ||
            item.itemName.ToLower().Contains("bottle") ||
            item.itemName.ToLower().Contains("flask"))
            return true;

        return false;
    }

    void UseDrinkableItem(ItemData item)
    {
        // Spela drickanimation
        if (animController != null)
        {
            animController.PlayAnimation(drinkAnimationName, drinkAnimationDuration);
            Debug.Log($"Dricker från {item.itemName}");
        }

        // Spela ljud
        if (audioSource != null && drinkSound != null)
        {
            audioSource.PlayOneShot(drinkSound);
        }

        // Uppdatera spelarattribut om komponenten finns
        if (playerAttributes != null)
        {
            // Använd värden från ItemData om de finns
            float thirstRestore = item.thirstRestore > 0 ? item.thirstRestore : 25f;
            float healthGain = item.healthRestore > 0 ? item.healthRestore : 10f;
            float sicknessReduction = item.SicknessRestore > 0 ? item.SicknessRestore : 5f;

            //playerAttributes.DrinkWater(thirstRestore, healthGain, sicknessReduction, 0);
        }

        // Ta bort item från inventariet om det är förbrukningsbart
        // Eller minska antalet om det är stackbart
        if (item.isStackable)
        {
            InventoryManager.Instance.RemoveItem(item, 1);
        }
        else
        {
            // För flaskor som inte förbrukas, kan du implementera en annan logik här
            // Till exempel minska durability på flaskan, eller låta den vara kvar
            Debug.Log("Använde " + item.itemName + " utan att förbruka den");
        }
    }
}