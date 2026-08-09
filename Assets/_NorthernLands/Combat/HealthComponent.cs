using System;
using UnityEngine;

namespace NorthernLands.Combat
{
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maximum = 100f;
        [SerializeField, Min(0f)] private float armor;
        [SerializeField, Range(0f, 1f)] private float blockReduction = 0.65f;

        public float Current { get; private set; }
        public float Maximum => maximum;
        public float Normalized => maximum <= 0f ? 0f : Current / maximum;
        public bool IsDead { get; private set; }
        public bool IsBlocking { get; set; }

        public event Action<float, float> Changed;
        public event Action<DamageInfo> Died;

        private void Awake() => RestoreFull();

        public void Configure(float maxHealth, float armorValue = 0f)
        {
            maximum = Mathf.Max(1f, maxHealth);
            armor = Mathf.Max(0f, armorValue);
            RestoreFull();
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (IsDead || damage.Amount <= 0f)
                return;

            var afterArmor = DamageCalculator.AfterArmor(damage.Amount, armor);
            var finalDamage = DamageCalculator.AfterBlock(
                afterArmor,
                damage.CanBeBlocked && IsBlocking,
                blockReduction);

            Current = Mathf.Max(0f, Current - finalDamage);
            Changed?.Invoke(Current, maximum);

            if (Current > 0f)
                return;

            IsDead = true;
            Died?.Invoke(damage);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            Current = Mathf.Min(maximum, Current + amount);
            Changed?.Invoke(Current, maximum);
        }

        public void RestoreFull()
        {
            IsDead = false;
            Current = maximum;
            Changed?.Invoke(Current, maximum);
        }
    }
}
