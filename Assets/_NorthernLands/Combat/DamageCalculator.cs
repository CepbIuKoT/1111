using UnityEngine;

namespace NorthernLands.Combat
{
    public static class DamageCalculator
    {
        public static float AfterArmor(float rawDamage, float armor)
        {
            rawDamage = Mathf.Max(0f, rawDamage);
            armor = Mathf.Max(0f, armor);
            return rawDamage * (100f / (100f + armor));
        }

        public static float AfterBlock(float damage, bool blocking, float reduction)
        {
            if (!blocking)
                return Mathf.Max(0f, damage);

            return Mathf.Max(0f, damage) * (1f - Mathf.Clamp01(reduction));
        }
    }
}
