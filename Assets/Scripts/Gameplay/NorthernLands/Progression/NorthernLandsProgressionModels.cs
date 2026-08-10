using System;

namespace Unity.BossRoom.Gameplay.NorthernLands.Progression
{
    [Serializable]
    public class QuestProgressData
    {
        public string questId;
        public int currentAmount;
        public int requiredAmount;
        public bool completed;
        public bool rewardClaimed;
    }

    [Serializable]
    public class NorthernLandsHeroStats
    {
        public float maxHealth = 120f;
        public float maxMana = 60f;
        public float currentHealth = 120f;
        public float currentMana = 60f;
        public float baseDamage = 12f;
        public float armor;
        public float moveSpeed = 5f;
        public float dodgeChance = 0.05f;
        public float criticalChance = 0.05f;
        public int maxDashCharges = 2;
    }
}
