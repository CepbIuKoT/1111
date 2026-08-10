using System;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Progression;

namespace Unity.BossRoom.Gameplay.NorthernLands.Quests
{
    public sealed class QuestProgressService
    {
        public const string FirstHuntQuestId = "riverholm_first_hunt";

        readonly NorthernLandsProgressState m_Progress;

        public QuestProgressService(NorthernLandsProgressState progress)
        {
            m_Progress = progress;
        }

        public QuestProgressData Start(string questId, int requiredAmount)
        {
            var existing = Get(questId);
            if (existing != null)
            {
                return existing;
            }

            var quest = new QuestProgressData
            {
                questId = questId,
                requiredAmount = Math.Max(1, requiredAmount)
            };
            var quests = m_Progress.Run.quests;
            Array.Resize(ref quests, quests.Length + 1);
            quests[^1] = quest;
            m_Progress.Run.quests = quests;
            return quest;
        }

        public bool AddProgress(string questId, int amount)
        {
            var quest = Get(questId);
            if (quest == null || quest.completed)
            {
                return false;
            }

            quest.currentAmount = Math.Min(quest.requiredAmount, quest.currentAmount + Math.Max(0, amount));
            quest.completed = quest.currentAmount >= quest.requiredAmount;
            return quest.completed;
        }

        public bool TryClaimReward(string questId, int goldReward, int experienceReward, HeroProgressionService progression)
        {
            var quest = Get(questId);
            if (quest == null || !quest.completed || quest.rewardClaimed)
            {
                return false;
            }

            quest.rewardClaimed = true;
            m_Progress.Run.gold += Math.Max(0, goldReward);
            progression.AddExperience(experienceReward);
            return true;
        }

        public QuestProgressData Get(string questId)
        {
            var quests = m_Progress.Run.quests;
            for (var i = 0; i < quests.Length; i++)
            {
                if (quests[i].questId == questId)
                {
                    return quests[i];
                }
            }

            return null;
        }
    }
}
