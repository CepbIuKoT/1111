using NorthernLands.Core.StateMachine;
using NUnit.Framework;

namespace NorthernLands.Tests.EditMode
{
    public sealed class GameStateMachineTests
    {
        [Test]
        public void StateChangeNotifiesWithPreviousAndNextState()
        {
            var stateMachine = new GameStateMachine();
            var previous = GameState.Playing;
            var next = GameState.Playing;

            stateMachine.Changed += (oldState, newState) =>
            {
                previous = oldState;
                next = newState;
            };

            stateMachine.ChangeTo(GameState.MainMenu);

            Assert.That(previous, Is.EqualTo(GameState.Booting));
            Assert.That(next, Is.EqualTo(GameState.MainMenu));
        }
    }
}
