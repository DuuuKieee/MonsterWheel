using System;

namespace MonsterWheel.Core
{
    /// <summary>
    /// Game rules and motion of the Monster Wheel, independent of rendering.
    /// Ring position is measured in slots (clockwise), reel position in icons.
    /// </summary>
    public sealed class WheelEngine
    {
        private readonly WheelSettings _settings;
        private readonly IRandom _random;
        private readonly WheelStateMachine _machine = new WheelStateMachine();

        private float _ringSpeed;
        private float _reelSpeed;
        private DecelerationMotion _reelStop;
        private DecelerationMotion _reelSettle;
        private DecelerationMotion _ringStop;
        private float _handoffTimer;
        private int _reelTarget;

        public WheelEngine(WheelSettings settings, IRandom random)
        {
            _settings = settings;
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Layout = RingLayout.BuildIconOrder();
        }

        public event Action RoundFinished;

        public WheelState State => _machine.State;
        public int[] Layout { get; }
        public float RingPosition { get; private set; }
        public float ReelPosition { get; private set; }
        public int TargetIcon { get; private set; } = -1;

        /// <summary>True while idle after a finished round (the result is on screen).</summary>
        public bool HasResult { get; private set; }

        public int ActiveSlot => Mod((int)Math.Floor(RingPosition), RingLayout.SlotCount);
        public int CenterIcon => Mod((int)Math.Round(ReelPosition), RingLayout.IconCount);

        public float RingSpeedRatio =>
            (_ringStop != null ? _ringStop.Speed : _ringSpeed) / _settings.RingSpinSpeed;

        public float ReelSpeedRatio =>
            Math.Abs(CurrentReelMotion != null ? CurrentReelMotion.Speed : _reelSpeed) / _settings.ReelSpinSpeed;

        private DecelerationMotion CurrentReelMotion => _reelSettle ?? _reelStop;

        public void Press()
        {
            var from = _machine.State;
            if (!_machine.Press()) return;

            if (from == WheelState.Idle)
            {
                HasResult = false;
                _ringStop = null;
                _reelStop = null;
                _reelSettle = null;
            }
            else
            {
                BeginReelStop();
            }
        }

        public void Tick(float dt)
        {
            if (dt <= 0f) return;
            if (dt > _settings.MaxDeltaTime) dt = _settings.MaxDeltaTime;

            switch (_machine.State)
            {
                case WheelState.Spinning:
                    SpinRing(dt);
                    SpinReel(dt);
                    break;
                case WheelState.StoppingReel:
                    SpinRing(dt);
                    TickReelStop(dt);
                    break;
                case WheelState.StoppingRing:
                    TickRingStop(dt);
                    break;
            }
        }

        private void SpinRing(float dt)
        {
            _ringSpeed = RampUp(_ringSpeed, _settings.RingSpinSpeed, dt);
            RingPosition = Wrap(RingPosition + _ringSpeed * dt, RingLayout.SlotCount);
        }

        private void SpinReel(float dt)
        {
            _reelSpeed = RampUp(_reelSpeed, _settings.ReelSpinSpeed, dt);
            ReelPosition = Wrap(ReelPosition + _reelSpeed * dt, RingLayout.IconCount);
        }

        private void BeginReelStop()
        {
            TargetIcon = _random.Next(RingLayout.IconCount);

            // Smallest integer end >= position + one round that centers the target icon.
            var end = (int)Math.Ceiling(ReelPosition) + RingLayout.IconCount;
            end += Mod(TargetIcon - end, RingLayout.IconCount);

            // Decelerate slightly past the target, then settle back: a fixed-size overshoot that never
            // brings the neighbouring icon to the center.
            _reelTarget = end;
            var speed = Math.Max(_reelSpeed, _settings.ReelSpinSpeed * _settings.MinStopSpeedRatio);
            _reelStop = DecelerationMotion.FromSpeed(ReelPosition, end + _settings.ReelOvershoot, speed, EaseKind.OutCubic);
            _reelSettle = null;
            _handoffTimer = 0f;
        }

        private void TickReelStop(float dt)
        {
            if (!_reelStop.IsDone)
            {
                _reelStop.Advance(dt);
                ReelPosition = _reelStop.Position;
                if (_reelStop.IsDone)
                    _reelSettle = new DecelerationMotion(ReelPosition, _reelTarget, _settings.ReelSettleTime, EaseKind.OutQuad);
                return;
            }

            if (!_reelSettle.IsDone)
            {
                _reelSettle.Advance(dt);
                ReelPosition = _reelSettle.Position;
                if (!_reelSettle.IsDone) return;
                ReelPosition = Mod(_reelTarget, RingLayout.IconCount);
                _reelSpeed = 0f;
                return;
            }

            _handoffTimer += dt;
            if (_handoffTimer < _settings.HandoffDelay) return;

            _machine.NotifyReelStopped();
            BeginRingStop();
        }

        private void BeginRingStop()
        {
            var baseSlot = (int)Math.Floor(RingPosition);
            var steps = StopPlanner.StepsToTarget(Mod(baseSlot, RingLayout.SlotCount), TargetIcon, Layout);
            var speed = Math.Max(_ringSpeed, _settings.RingSpinSpeed * _settings.MinStopSpeedRatio);
            _ringStop = DecelerationMotion.FromSpeed(RingPosition, baseSlot + steps, speed, EaseKind.OutQuad);
        }

        private void TickRingStop(float dt)
        {
            _ringStop.Advance(dt);
            RingPosition = _ringStop.Position;
            if (!_ringStop.IsDone) return;

            RingPosition = Mod((int)_ringStop.End, RingLayout.SlotCount);
            _ringSpeed = 0f;
            HasResult = true;
            _machine.NotifyRingStopped();
            RoundFinished?.Invoke();
        }

        private float RampUp(float speed, float max, float dt)
        {
            if (_settings.SpinUpTime <= 0f) return max;
            speed += max / _settings.SpinUpTime * dt;
            return speed > max ? max : speed;
        }

        private static float Wrap(float value, int length)
        {
            value %= length;
            return value < 0f ? value + length : value;
        }

        private static int Mod(int value, int length)
        {
            var m = value % length;
            return m < 0 ? m + length : m;
        }
    }
}
