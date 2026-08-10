using NUnit.Framework;
using Unity.BossRoom.Gameplay.NorthernLands.Menu;

namespace Unity.BossRoom.Tests.Runtime
{
    public class NorthernLandsMenuFlowTests
    {
        [Test]
        public void ContinueIsDisabledWithoutSave()
        {
            Assert.That(
                NorthernLandsMenuFlow.Resolve(NorthernLandsMenuAction.Continue, false, false),
                Is.EqualTo(NorthernLandsMenuDestination.Disabled));
        }

        [Test]
        public void NewGameOpensRaceSelectionWhenRaceWasNotChosen()
        {
            Assert.That(
                NorthernLandsMenuFlow.Resolve(NorthernLandsMenuAction.NewGame, false, false),
                Is.EqualTo(NorthernLandsMenuDestination.RaceSelection));
        }

        [Test]
        public void NewGameKeepsPermanentRaceAcrossRuns()
        {
            Assert.That(
                NorthernLandsMenuFlow.Resolve(NorthernLandsMenuAction.NewGame, false, true),
                Is.EqualTo(NorthernLandsMenuDestination.Campaign));
        }
    }
}
