using System;

namespace Unity.BossRoom.Gameplay.NorthernLands.Items
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Ring
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public class EquipmentItemData
    {
        public string instanceId;
        public string displayName;
        public EquipmentSlot slot;
        public ItemRarity rarity;
        public int itemLevel = 1;
        public float damage;
        public float armor;
        public float health;
        public float mana;
        public float speed;
        public float criticalChance;
        public bool isLiving;
        public string livingName;
        public int soulLevel = 1;
        public int soulExperience;
        public int killCount;
        public string[] voiceLines = Array.Empty<string>();
    }
}
