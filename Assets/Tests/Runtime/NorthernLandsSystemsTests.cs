using NUnit.Framework;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Items;
using Unity.BossRoom.Gameplay.NorthernLands.Progression;
using Unity.BossRoom.Gameplay.NorthernLands.Quests;
using Unity.BossRoom.Gameplay.NorthernLands.Reputation;

namespace Unity.BossRoom.Tests.Runtime
{
    public class NorthernLandsSystemsTests
    {
        [Test]
        public void LevelUpRestoresVitalsAndGrantsTwoTalentChoices()
        {
            var state = new NorthernLandsProgressState();
            var progression = new HeroProgressionService(state);
            state.Run.heroStats.currentHealth = 1f;

            var gained = progression.AddExperience(HeroProgressionService.ExperienceForLevel(1));

            Assert.That(gained, Is.EqualTo(1));
            Assert.That(state.Run.level, Is.EqualTo(2));
            Assert.That(state.Run.pendingTalentChoices, Is.EqualTo(2));
            Assert.That(state.Run.heroStats.currentHealth, Is.EqualTo(state.Run.heroStats.maxHealth));
        }

        [Test]
        public void LivingWeaponConsumesItemAndKeepsAbsorbedStats()
        {
            var living = new EquipmentItemData { instanceId = "living", slot = EquipmentSlot.Weapon, isLiving = true };
            var sacrifice = new EquipmentItemData { instanceId = "loot", itemLevel = 5, rarity = ItemRarity.Rare, damage = 10f };
            var service = new LivingItemService();

            Assert.That(service.Consume(living, sacrifice), Is.True);
            Assert.That(living.damage, Is.GreaterThanOrEqualTo(2f));
            Assert.That(living.soulExperience + living.soulLevel, Is.GreaterThan(1));
        }

        [Test]
        public void FirstRiverholmQuestCompletesAfterFourKillsAndPaysOnce()
        {
            var state = new NorthernLandsProgressState();
            var progression = new HeroProgressionService(state);
            var quests = new QuestProgressService(state);
            quests.Start(QuestProgressService.FirstHuntQuestId, 4);

            for (var i = 0; i < 4; i++)
            {
                quests.AddProgress(QuestProgressService.FirstHuntQuestId, 1);
            }

            Assert.That(quests.TryClaimReward(QuestProgressService.FirstHuntQuestId, 100, 50, progression), Is.True);
            Assert.That(quests.TryClaimReward(QuestProgressService.FirstHuntQuestId, 100, 50, progression), Is.False);
            Assert.That(state.Run.gold, Is.EqualTo(100));
        }

        [Test]
        public void CrimeIsTrackedPerWorldAndCanBeClearedByIntermediary()
        {
            var state = new NorthernLandsProgressState();
            var reputation = new CityReputationService(state);
            state.Run.gold = 50;

            reputation.RecordCrime(NorthernWorldId.NorthernLands, 12);

            Assert.That(reputation.Get(NorthernWorldId.NorthernLands).isCriminal, Is.True);
            Assert.That(reputation.Get(NorthernWorldId.AshenWorld).isCriminal, Is.False);
            Assert.That(reputation.TryClearNameWithGold(NorthernWorldId.NorthernLands, 25), Is.True);
            Assert.That(state.Run.gold, Is.EqualTo(25));
        }

        [Test]
        public void DeadWorldObjectiveUnlocksTowerAfterFiveSoulsAndTwoAsh()
        {
            var state = new NorthernLandsProgressState();

            for (var i = 0; i < 5; i++)
            {
                state.RecordSoulKill(i == 1 || i == 3);
            }

            Assert.That(state.Run.soulKills, Is.EqualTo(5));
            Assert.That(state.Run.soulAsh, Is.EqualTo(2));
            Assert.That(state.CanEnter(NorthernWorldId.TowerOfGods), Is.True);
        }

        [Test]
        public void SecondDeadWorldDeathResetsRunButKeepsPermanentRace()
        {
            var state = new NorthernLandsProgressState();
            state.EternalRace.raceId = "northborn";
            state.Run.gold = 200;

            Assert.That(state.HandleDeadWorldDeath(), Is.False);
            Assert.That(state.HandleDeadWorldDeath(), Is.True);
            Assert.That(state.Run.gold, Is.Zero);
            Assert.That(state.EternalRace.raceId, Is.EqualTo("northborn"));
        }
    }
}
