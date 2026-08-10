using System;

namespace Unity.BossRoom.Gameplay.NorthernLands.Content
{
    public enum NorthernWorldId
    {
        NorthernLands,
        AshenWorld,
        StarWastes,
        DeadWorld,
        AncientDungeon,
        TowerOfGods,
        QuietDimension
    }

    public enum RaceAbilityKind
    {
        Teleport,
        Heal,
        NovaDamage,
        TemporaryArmor,
        Summon,
        Clone,
        TimeStop,
        Freeze,
        Poison,
        Fear,
        Reflect,
        CheatDeath,
        AstralForm,
        Rewind,
        QuietDimension,
        Dash,
        ManaBurst,
        LifeSteal,
        Shield,
        Curse,
        BlinkStrike
    }

    [Serializable]
    public class RaceDefinition
    {
        public string id;
        public string displayName;
        public string description;
        public RaceAbilityKind ability;
        public float healthMultiplier = 1f;
        public float manaMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float armorBonus;
        public float criticalChanceBonus;
        public float dodgeChanceBonus;
        public float cooldownSeconds = 30f;
    }

    [Serializable]
    public class WorldDefinition
    {
        public NorthernWorldId id;
        public string displayName;
        public string sceneName;
        public string cityName;
        public string biome;
        public bool randomEnemies;
        public bool randomPortals;
        public int recommendedLevel;
    }

    [Serializable]
    public class NorthernLandsContentDocument
    {
        public int schemaVersion = 1;
        public RaceDefinition[] races = Array.Empty<RaceDefinition>();
        public WorldDefinition[] worlds = Array.Empty<WorldDefinition>();
    }
}
