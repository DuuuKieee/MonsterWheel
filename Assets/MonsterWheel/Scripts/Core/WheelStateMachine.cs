using System;

namespace MonsterWheel.Core
{
    /// <summary>Idle → Spinning → StoppingReel → StoppingRing → Idle.</summary>
    public sealed class WheelStateMachine
    {
        public WheelState State { get; private set; } = WheelState.Idle;

        /// <summary>(from, to)</summary>
        public event Action<WheelState, WheelState> StateChanged;

        /// <summary>Handles the player's button. Returns false when the press is ignored (while stopping).</summary>
        public bool Press()
        {
            switch (State)
            {
                case WheelState.Idle:
                    Set(WheelState.Spinning);
                    return true;
                case WheelState.Spinning:
                    Set(WheelState.StoppingReel);
                    return true;
                default:
                    return false;
            }
        }

        public void NotifyReelStopped()
        {
            Require(WheelState.StoppingReel);
            Set(WheelState.StoppingRing);
        }

        public void NotifyRingStopped()
        {
            Require(WheelState.StoppingRing);
            Set(WheelState.Idle);
        }

        private void Require(WheelState expected)
        {
            if (State != expected)
                throw new InvalidOperationException($"Expected state {expected} but was {State}.");
        }

        private void Set(WheelState next)
        {
            var prev = State;
            State = next;
            StateChanged?.Invoke(prev, next);
        }
    }
}
