using UnityEngine;

namespace NorthernLands.GameData.Races
{
    [CreateAssetMenu(fileName = "Race_", menuName = "Northern Lands/Race Definition")]
    public sealed class RaceDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea(3, 8)] [SerializeField] private string description;
        [SerializeField] private Sprite portrait;

        [Header("Base modifiers")]
        [SerializeField] private float healthMultiplier = 1f;
        [SerializeField] private float manaMultiplier = 1f;
        [SerializeField] private float damageMultiplier = 1f;

        [Header("Unique ability")]
        [SerializeField] private string abilityId;
        [TextArea(2, 6)] [SerializeField] private string abilityRules;
        [Min(0f)] [SerializeField] private float durationSeconds;
        [Min(0f)] [SerializeField] private float cooldownSeconds;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public float HealthMultiplier => healthMultiplier;
        public float ManaMultiplier => manaMultiplier;
        public float DamageMultiplier => damageMultiplier;
        public string AbilityId => abilityId;
        public string AbilityRules => abilityRules;
        public float DurationSeconds => durationSeconds;
        public float CooldownSeconds => cooldownSeconds;

        private void OnValidate()
        {
            id = id?.Trim().ToLowerInvariant().Replace(' ', '_');
            healthMultiplier = Mathf.Max(0.01f, healthMultiplier);
            manaMultiplier = Mathf.Max(0.01f, manaMultiplier);
            damageMultiplier = Mathf.Max(0.01f, damageMultiplier);
        }
    }
}
