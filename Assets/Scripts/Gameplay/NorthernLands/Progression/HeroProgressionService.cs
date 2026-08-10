using System;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;

namespace Unity.BossRoom.Gameplay.NorthernLands.Progression
{
    public sealed class HeroProgressionService
    {
        readonly NorthernLandsProgressState m_Progress;

        public HeroProgressionService(NorthernLandsProgressState progress)
        {
            m_Progress = progress;
        }

        public int AddExperience(int amount)
        {
            m_Progress.Run.experience += Math.Max(0, amount);
            var levelsGained = 0;

            while (m_Progress.Run.experience >= ExperienceForLevel(m_Progress.Run.level))
            {
                m_Progress.Run.experience -= ExperienceForLevel(m_Progress.Run.level);
                m_Progress.Run.level++;
                m_Progress.Run.pendingTalentChoices += 2;
                m_Progress.Run.heroStats.maxHealth += 12f;
                m_Progress.Run.heroStats.maxMana += 5f;
                m_Progress.Run.heroStats.baseDamage += 1.5f;
                m_Progress.Run.heroStats.currentHealth = m_Progress.Run.heroStats.maxHealth;
                m_Progress.Run.heroStats.currentMana = m_Progress.Run.heroStats.maxMana;
                levelsGained++;
            }

            return levelsGained;
        }

        public bool TryLearnTalent(string talentId)
        {
            if (m_Progress.Run.pendingTalentChoices <= 0 || string.IsNullOrWhiteSpace(talentId) || Array.IndexOf(m_Progress.Run.learnedTalentIds, talentId) >= 0)
            {
                return false;
            }

            var learned = new string[m_Progress.Run.learnedTalentIds.Length + 1];
            Array.Copy(m_Progress.Run.learnedTalentIds, learned, m_Progress.Run.learnedTalentIds.Length);
            learned[^1] = talentId;
            m_Progress.Run.learnedTalentIds = learned;
            m_Progress.Run.pendingTalentChoices--;
            ApplyTalent(talentId, m_Progress.Run.heroStats);
            return true;
        }

        public static int ExperienceForLevel(int level)
        {
            return 75 + Math.Max(1, level) * 35;
        }

        static void ApplyTalent(string talentId, NorthernLandsHeroStats stats)
        {
            switch (talentId)
            {
                case "vitality": stats.maxHealth += 20f; stats.currentHealth += 20f; break;
                case "deep_mana": stats.maxMana += 15f; stats.currentMana += 15f; break;
                case "swift_step": stats.moveSpeed += 0.35f; break;
                case "iron_skin": stats.armor += 3f; break;
                case "sharp_edge": stats.baseDamage += 3f; break;
                case "critical_eye": stats.criticalChance += 0.04f; break;
                case "evasion": stats.dodgeChance += 0.04f; break;
                case "third_dash": stats.maxDashCharges++; break;
                case "magic_mastery": stats.maxMana += 8f; stats.baseDamage += 1f; break;
                case "last_chance": break;
                case "blood_feast": break;
                default: throw new ArgumentException($"Unknown talent '{talentId}'.", nameof(talentId));
            }
        }
    }
}
