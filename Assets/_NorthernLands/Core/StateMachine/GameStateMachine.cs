using System;

namespace NorthernLands.Core.StateMachine
{
    public sealed class GameStateMachine
    {
        public GameState Current { get; private set; } = GameState.Booting;

        public event Action<GameState, GameState> Changed;

        public void ChangeTo(GameState next)
        {
            if (Current == next)
                return;

            var previous = Current;
            Current = next;
            Changed?.Invoke(previous, next);
        }
    }
}
