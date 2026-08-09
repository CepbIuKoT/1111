using UnityEngine;

namespace NorthernLands.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, GameObject source, bool canBeBlocked = true)
        {
            Amount = Mathf.Max(0f, amount);
            Source = source;
            CanBeBlocked = canBeBlocked;
        }

        public float Amount { get; }
        public GameObject Source { get; }
        public bool CanBeBlocked { get; }
    }
}
