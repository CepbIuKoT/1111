using NUnit.Framework;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;

namespace Unity.BossRoom.Tests.Runtime
{
    public class NorthernLandsContentTests
    {
        [Test]
        public void CatalogContainsAllRequiredRacesAndWorlds()
        {
            var catalog = new NorthernLandsContentCatalog();

            Assert.That(catalog.Races.Count, Is.EqualTo(45));
            Assert.That(catalog.Worlds.Count, Is.EqualTo(7));
        }

        [Test]
        public void TowerUnlocksAfterFiveSoulKillsAndTwoAshDrops()
        {
            var progress = new NorthernLandsProgressState();

            progress.RecordSoulKill(true);
            progress.RecordSoulKill(true);
            progress.RecordSoulKill(false);
            progress.RecordSoulKill(false);
            progress.RecordSoulKill(false);

            Assert.That(progress.Run.towerUnlocked, Is.True);
            Assert.That(progress.CanEnter(NorthernWorldId.TowerOfGods), Is.True);
        }

        [Test]
        public void SecondDeadWorldDeathResetsRunButNotPermanentRace()
        {
            var progress = new NorthernLandsProgressState();
            var catalog = new NorthernLandsContentCatalog();
            Assert.That(progress.TryChoosePermanentRace("northborn", catalog), Is.True);

            Assert.That(progress.HandleDeadWorldDeath(), Is.False);
            Assert.That(progress.HandleDeadWorldDeath(), Is.True);

            Assert.That(progress.EternalRace.raceId, Is.EqualTo("northborn"));
            Assert.That(progress.Run.level, Is.EqualTo(1));
            Assert.That(progress.Run.currentWorld, Is.EqualTo(NorthernWorldId.NorthernLands));
        }
    }
}
