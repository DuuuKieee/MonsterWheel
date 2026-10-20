namespace MonsterWheel.Core
{
    /// <summary>
    /// Moves a position from start to end over a fixed duration with an easing curve.
    /// Position is computed from elapsed time (not integrated), so it lands exactly on End.
    /// </summary>
    public sealed class DecelerationMotion
    {
        private readonly float _start;
        private readonly float _duration;
        private readonly EaseKind _ease;
        private readonly float _backOvershoot;
        private float _elapsed;

        public DecelerationMotion(float start, float end, float duration, EaseKind ease, float backOvershoot = 1.2f)
        {
            _start = start;
            End = end;
            _duration = duration;
            _ease = ease;
            _backOvershoot = backOvershoot;
            Position = start;
        }

        /// <summary>
        /// Builds a motion whose initial speed equals <paramref name="startSpeed"/>, so it continues
        /// seamlessly from a constant-speed spin. Duration = slope(0) * distance / startSpeed.
        /// </summary>
        public static DecelerationMotion FromSpeed(float start, float end, float startSpeed, EaseKind ease, float backOvershoot = 1.2f)
        {
            var duration = Easing.Slope(ease, 0f, backOvershoot) * (end - start) / startSpeed;
            return new DecelerationMotion(start, end, duration, ease, backOvershoot);
        }

        public float Position { get; private set; }
        public float End { get; }
        public float Duration => _duration;
        public bool IsDone { get; private set; }

        /// <summary>Current velocity in position units per second (0 once done).</summary>
        public float Speed => IsDone || _duration <= 0f
            ? 0f
            : (End - _start) / _duration * Easing.Slope(_ease, _elapsed / _duration, _backOvershoot);

        public void Advance(float dt)
        {
            if (IsDone) return;
            _elapsed += dt;
            if (_elapsed >= _duration)
            {
                Position = End;
                IsDone = true;
                return;
            }
            Position = _start + (End - _start) * Easing.Evaluate(_ease, _elapsed / _duration, _backOvershoot);
        }
    }
}
