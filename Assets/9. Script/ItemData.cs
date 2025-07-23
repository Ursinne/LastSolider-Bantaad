using UnityEngine;

namespace InventoryAndCrafting
{
    public enum PersonalityType
    {
        Analyst,
        Rebel,
        Adventurer,
        Visionary
    }

    public enum ItemType
    {
        Tool,  // Lagt till Tool som en ny ItemType
        Survival,
        Material,
        Weapon,
        Armor,
        All
    }

    public enum ToolType
    {
        None,
        Axe,
        Bottle,
        Drill,
        FishingRod,
        Hammer,
        Knife,
        Hoe,
        Pickaxe,
        Shovel,
        Syringe,
        StickPoint,
        Wrench,
        Sickle
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public enum EquipmentSlotType
    {
        None,
        Head,
        Chest,
        Hands,
        Feet,
        MainHand,
        OffHand,
        WorkTool
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        public Sprite itemIcon;
        [TextArea(3, 5)]
        public string description;
        [TextArea(2, 3)]
        public string tooltipText;
        public ItemType itemType;
        public ItemRarity rarity = ItemRarity.Common;
        [Tooltip("Prefab som kommer instantieras när detta föremål släpps i världen")]
        public GameObject itemPrefab;

        [Header("Stack Settings")]
        public bool isStackable = true;
        public bool useCustomStackSize = false;
        public int maxStackSize = 24;
        public int basePrice;
        public int amount = 1;

        [Header("Equipment Stats")]
        public bool isEquippable;
        [Tooltip("Bu item hangi equipment slot'una yerleştirilebilir")]
        public EquipmentSlotType equipmentSlotType = EquipmentSlotType.None;
        public int requiredLevel;
        public float durability = 100f;
        public float maxDurability = 100f;

        [Header("Tool Settings")]
        public ToolType toolType = ToolType.None;
        public float toolEfficiency = 1.0f;  // Hur effektivt verktyget är
        public float harvestAmount = 1.0f;   // Hur mycket resurs som samlas in per användning
        public string[] compatibleResources;  // Vilka typer av resurser verktyget kan användas på
        public float durabilityLossPerUse = 1.0f;  // Hur mycket hållbarhet som förloras per användning

        [Header("Weapon Stats")]
        public float damage;
        public float attackSpeed;
        public float criticalChance;
        public float criticalMultiplier = 1.5f;

        [Header("Armor Stats")]
        public float defense;
        //public float magicResistance;
        public float weight;

        [Header("Consumable Stats")]
        public float healthRestore;
        public float thirstRestore;
        public float hungerRestore;
        public float SicknessRestore;
        public float sexRestore;
        public float manaRestore;
        public float staminaRestore;
        public float duration;
        public bool hasTemporaryEffect;

        [Header("Effects")]
        public bool appliesBuffs;
        public float strengthBuff;
        public float agilityBuff;
        public float intelligenceBuff;
        public float healthBuff;
        public float manaBuff;

        public string GetRarityColorHex()
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return "#FFFFFF"; // White
                case ItemRarity.Uncommon:
                    return "#1EFF00"; // Green
                case ItemRarity.Rare:
                    return "#0070DD"; // Blue
                case ItemRarity.Epic:
                    return "#A335EE"; // Purple
                case ItemRarity.Legendary:
                    return "#FF8000"; // Orange
                case ItemRarity.Mythic:
                    return "#FF0000"; // Red
                default:
                    return "#FFFFFF";
            }
        }

        public string GetFormattedTooltip()
        {
            string colorHex = GetRarityColorHex();
            string tooltip = $"<color={colorHex}>{itemName}</color>\n";
            tooltip += $"<color=#888888><i>{itemType}</i></color>\n";

            // Description her zaman en üstte gösterilsin
            if (!string.IsNullOrEmpty(description))
            {
                tooltip += $"\n<color=#888888>{description}</color>";
            }

            if (isEquippable)
            {
                tooltip += $"\n\nRequired Level: {requiredLevel}";
                tooltip += $"\nDurability: {durability}/{maxDurability}";

                if (itemType == ItemType.Weapon)
                {
                    tooltip += $"\n\nDamage: {damage}";
                    if (attackSpeed > 0) tooltip += $"\nAttack Speed: {attackSpeed:F1}";
                    if (criticalChance > 0) tooltip += $"\nCrit Chance: {criticalChance:F1}%";
                    if (criticalMultiplier > 1) tooltip += $"\nCrit Multiplier: {criticalMultiplier:F1}x";
                }
                else if (itemType == ItemType.Armor)
                {
                    tooltip += $"\n\nDefense: {defense}";
                    if (weight > 0) tooltip += $"\nWeight: {weight:F1}";
                }
                else if (itemType == ItemType.Tool)
                {
                    tooltip += $"\n\nTool Type: {toolType}";
                    tooltip += $"\nEfficiency: {toolEfficiency:F1}";
                    if (compatibleResources != null && compatibleResources.Length > 0)
                    {
                        tooltip += $"\nUsable on: {string.Join(", ", compatibleResources)}";
                    }
                }
            }
            else if (itemType == ItemType.Survival)
            {
                if (healthRestore > 0) tooltip += $"\nRestores {healthRestore} Health";
                if (thirstRestore > 0) tooltip += $"\nRestores {thirstRestore} Thirst";
                if (hungerRestore > 0) tooltip += $"\nRestores {hungerRestore} Hunger";
                if (SicknessRestore > 0) tooltip += $"\nRestores {SicknessRestore} Sickness";
                if (sexRestore > 0) tooltip += $"\nRestores {sexRestore} Sex";
                if (manaRestore > 0) tooltip += $"\nRestores {manaRestore} Mana";
                if (staminaRestore > 0) tooltip += $"\nRestores {staminaRestore} Stamina";
                if (hasTemporaryEffect) tooltip += $"\nDuration: {duration}s";
            }

            // Bufflar sadece değer varsa gösterilsin
            if (appliesBuffs && (strengthBuff > 0 || agilityBuff > 0 || intelligenceBuff > 0 ||
                healthBuff > 0 || manaBuff > 0))
            {
                tooltip += "\n\nBuffs:";
                if (strengthBuff > 0) tooltip += $"\n+ {strengthBuff} Strength";
                if (agilityBuff > 0) tooltip += $"\n+ {agilityBuff} Agility";
                if (intelligenceBuff > 0) tooltip += $"\n+ {intelligenceBuff} Intelligence";
                if (healthBuff > 0) tooltip += $"\n+ {healthBuff} Health";
                if (manaBuff > 0) tooltip += $"\n+ {manaBuff} Mana";
            }

            if (basePrice > 0)
            {
                tooltip += $"\n\nValue: {basePrice:N0} gold";
            }

            return tooltip;
        }
    }
}