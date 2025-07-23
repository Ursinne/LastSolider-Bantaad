using UnityEngine;
using InventoryAndCrafting;

public class DrinkItemHandler : MonoBehaviour
{
    [Header("Drink Settings")]
    public KeyCode drinkKey = KeyCode.R; // Knapp f�r att dricka
    public string drinkAnimationName = "isDrinking"; // Animationsparameter
    public float drinkAnimationDuration = 2.0f; // Hur l�nge animationen spelas

    [Header("Item Tag")]
    public string drinkableItemTag = "Bottle"; // Tagg f�r drickbara f�rem�l i inventariet

    [Header("Audio")]
    public AudioClip drinkSound; // Ljudeffekt n�r man dricker
    private AudioSource audioSource;

    // Referens till spelarens komponenter
    private PlayerAnimationController animController;
    private PlayerAttributes playerAttributes;

    void Start()
    {
        // Hitta n�dv�ndiga komponenter
        animController = GetComponent<PlayerAnimationController>();
        playerAttributes = GetComponent<PlayerAttributes>();

        // L�gg till ljudk�lla om det beh�vs
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
        // Kontrollera om spelaren trycker p� drinkKey (R som standard)
        if (Input.GetKeyDown(drinkKey))
        {
            TryDrinkFromInventory();
        }
    }

    void TryDrinkFromInventory()
    {
        // Kontrollera om InventoryManager finns
        if (InventoryManager.Instance == null) return;

        // Hitta en flaska/drickbar f�rem�l i inventariet
        ItemData drinkableItem = FindDrinkableItemInInventory();

        if (drinkableItem != null)
        {
            // Anv�nd flaskan
            UseDrinkableItem(drinkableItem);
        }
        else
        {
            Debug.Log("Du har inget att dricka!");
        }
    }

    ItemData FindDrinkableItemInInventory()
    {
        // G� igenom inventariet och s�k efter drickbart f�rem�l
        foreach (var slot in InventoryManager.Instance.Slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                ItemData item = slot.GetItem();

                // Kontrollera om objektet �r drickbart
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
        // Kontrollera om f�rem�let �r drickbart baserat p� egenskaper
        if (item == null) return false;

        // Metod 1: Kontrollera p� itemType
        if (item.itemType == ItemType.Survival && item.thirstRestore > 0)
            return true;

        // Metod 2: Kontrollera p� toolType
        //if (item.toolType == ToolType.Bottle)
        //    return true;

        // Metod 3: Kontrollera p� namn (fallback)
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
            Debug.Log($"Dricker fr�n {item.itemName}");
        }

        // Spela ljud
        if (audioSource != null && drinkSound != null)
        {
            audioSource.PlayOneShot(drinkSound);
        }

        // Uppdatera spelarattribut om komponenten finns
        if (playerAttributes != null)
        {
            // Anv�nd v�rden fr�n ItemData om de finns
            float thirstRestore = item.thirstRestore > 0 ? item.thirstRestore : 25f;
            float healthGain = item.healthRestore > 0 ? item.healthRestore : 10f;
            float sicknessReduction = item.SicknessRestore > 0 ? item.SicknessRestore : 5f;

            //playerAttributes.DrinkWater(thirstRestore, healthGain, sicknessReduction, 0);
        }

        // Ta bort item fr�n inventariet om det �r f�rbrukningsbart
        // Eller minska antalet om det �r stackbart
        if (item.isStackable)
        {
            InventoryManager.Instance.RemoveItem(item, 1);
        }
        else
        {
            // F�r flaskor som inte f�rbrukas, kan du implementera en annan logik h�r
            // Till exempel minska durability p� flaskan, eller l�ta den vara kvar
            Debug.Log("Anv�nde " + item.itemName + " utan att f�rbruka den");
        }
    }
}