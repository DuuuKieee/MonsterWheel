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

        public float Position { get; private set; }
        public float End { get; }
        public bool IsDone { get; private set; }

        /// <summary>Normalized progress in [0, 1].</summary>
        public float Progress => _duration <= 0f ? 1f : (_elapsed >= _duration ? 1f : _elapsed / _duration);

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
