using System;

namespace Unity.BossRoom.Gameplay.NorthernLands.Items
{
    public sealed class LivingItemService
    {
        const int k_BaseSoulExperience = 50;

        public bool RecordOwnerKill(EquipmentItemData item, int experience)
        {
            if (item == null || !item.isLiving)
            {
                return false;
            }

            item.killCount++;
            item.soulExperience += Math.Max(1, experience);
            return ApplySoulLevels(item);
        }

        public bool Consume(EquipmentItemData livingItem, EquipmentItemData sacrifice)
        {
            if (livingItem == null || sacrifice == null || !livingItem.isLiving || livingItem.instanceId == sacrifice.instanceId)
            {
                return false;
            }

            livingItem.soulExperience += k_BaseSoulExperience + sacrifice.itemLevel * 10 + (int)sacrifice.rarity * 20;
            livingItem.damage += sacrifice.damage * 0.2f;
            livingItem.armor += sacrifice.armor * 0.2f;
            livingItem.health += sacrifice.health * 0.2f;
            livingItem.mana += sacrifice.mana * 0.2f;
            livingItem.speed += sacrifice.speed * 0.2f;
            livingItem.criticalChance += sacrifice.criticalChance * 0.2f;
            ApplySoulLevels(livingItem);
            return true;
        }

        static bool ApplySoulLevels(EquipmentItemData item)
        {
            var gainedLevel = false;
            while (item.soulExperience >= ExperienceForNextLevel(item.soulLevel))
            {
                item.soulExperience -= ExperienceForNextLevel(item.soulLevel);
                item.soulLevel++;
                gainedLevel = true;

                switch (item.slot)
                {
                    case EquipmentSlot.Weapon:
                        item.damage += 2f + item.soulLevel * 0.5f;
                        break;
                    case EquipmentSlot.Armor:
                        item.armor += 1f + item.soulLevel * 0.25f;
                        item.health += 5f + item.soulLevel;
                        break;
                    case EquipmentSlot.Ring:
                        item.mana += 4f + item.soulLevel;
                        item.speed += 0.01f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return gainedLevel;
        }

        static int ExperienceForNextLevel(int soulLevel)
        {
            return k_BaseSoulExperience + Math.Max(1, soulLevel) * 25;
        }
    }
}
