using UnityEngine;
using System.Collections.Generic;

namespace InventoryAndCrafting
{
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [Header("Recipe Info")]
        public string recipeName;
        public string description;
        public ItemData resultItem;
        public int resultAmount = 1;

        [Header("Crafting Requirements")]
        public float craftingTime = 1f;
        public List<RequiredItem> requirements = new List<RequiredItem>();
        public bool unlocked = true;

        [Header("Categories")]
        public CraftingCategory category;

        [Header("Result Properties")]
        [Tooltip("Om resultatet ska ha konsumerbara egenskaper även om ItemType inte är Consumable")]
        public bool hasConsumableProperties = false;

        [Header("Consumable Properties Override")]
        public bool overrideConsumableProperties = false;
        public float healthRestore;
        public float thirstRestore;
        public float hungerRestore;
        public float sicknessRestore;
        public float sexRestore;
        public float manaRestore;
        public float staminaRestore;

        [System.Serializable]
        public class RequiredItem
        {
            public ItemData item;
            public int amount = 1;
        }
    }
}
