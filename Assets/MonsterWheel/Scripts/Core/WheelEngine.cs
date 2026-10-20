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
        private DecelerationMotion _reelWindUp;
        private DecelerationMotion _reelStop;
        private DecelerationMotion _reelSettle;
        private DecelerationMotion _ringStop;
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

        private DecelerationMotion CurrentReelMotion => _reelSettle ?? _reelStop ?? ActiveWindUp;

        private DecelerationMotion ActiveWindUp => _reelWindUp != null && !_reelWindUp.IsDone ? _reelWindUp : null;

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
                BeginReelWindUp();
            }
            else
            {
                BeginStop();
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
                    if (ActiveWindUp != null) AdvanceReelWindUp(dt);
                    else SpinReel(dt);
                    break;
                case WheelState.StoppingReel:
                    AdvanceRingStop(dt);
                    if (AdvanceReelStop(dt)) _machine.NotifyReelStopped();
                    break;
                case WheelState.StoppingRing:
                    AdvanceRingStop(dt);
                    if (_ringStop.IsDone) FinishRound();
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

        /// <summary>The reel pulls back a little before launching forward, like gathering momentum.</summary>
        private void BeginReelWindUp()
        {
            _reelWindUp = _settings.ReelWindUpDistance > 0f && _settings.ReelWindUpTime > 0f
                ? new DecelerationMotion(ReelPosition, ReelPosition - _settings.ReelWindUpDistance,
                    _settings.ReelWindUpTime, EaseKind.SmoothStep)
                : null;
        }

        private void AdvanceReelWindUp(float dt)
        {
            _reelWindUp.Advance(dt);
            ReelPosition = Wrap(_reelWindUp.Position, RingLayout.IconCount);
        }

        /// <summary>Picks the result and starts reel and ring decelerating together.</summary>
        private void BeginStop()
        {
            TargetIcon = _random.Next(RingLayout.IconCount);
            var reelDuration = BeginReelStop();
            BeginRingStop(reelDuration + _settings.RingStopLag);
        }

        /// <returns>Total time until the reel rests on the target.</returns>
        private float BeginReelStop()
        {
            // Smallest integer end >= position + one round that centers the target icon.
            var end = (int)Math.Ceiling(ReelPosition) + RingLayout.IconCount;
            end += Mod(TargetIcon - end, RingLayout.IconCount);

            // Decelerate slightly past the target, then settle back: a fixed-size overshoot that never
            // brings the neighbouring icon to the center.
            _reelTarget = end;
            _reelWindUp = null;
            var speed = Math.Max(_reelSpeed, _settings.ReelSpinSpeed * _settings.MinStopSpeedRatio);
            _reelStop = DecelerationMotion.FromSpeed(ReelPosition, end + _settings.ReelOvershoot, speed, EaseKind.OutCubic);
            _reelSettle = null;
            return _reelStop.Duration + _settings.ReelSettleTime;
        }

        /// <summary>
        /// Starts the ring's constant deceleration toward the first slot showing the target icon that is
        /// far enough away for the stop to last at least <paramref name="minDuration"/>, so the ring
        /// always comes to rest shortly after the reel.
        /// </summary>
        private void BeginRingStop(float minDuration)
        {
            var slope = Easing.Slope(EaseKind.OutQuad, 0f, 0f);
            var speed = Math.Max(_ringSpeed, _settings.RingSpinSpeed * _settings.MinStopSpeedRatio);
            var minDistance = speed * minDuration / slope;
            var baseSlot = (int)Math.Floor(RingPosition);
            var frac = RingPosition - baseSlot;
            var steps = StopPlanner.FirstMatchAtOrAfter(
                Mod(baseSlot, RingLayout.SlotCount), TargetIcon, Layout, (int)Math.Ceiling(minDistance + frac));

            // The matching slot can be up to half a lap further than needed. Covering it at the current
            // speed takes longer than minDuration, which matters while the ring is still spinning up.
            // So pick the speed that ends exactly at minDuration, capped at the normal spin speed.
            var distance = steps - frac;
            speed = Math.Min(distance * slope / minDuration, Math.Max(speed, _settings.RingSpinSpeed));
            _ringStop = DecelerationMotion.FromSpeed(RingPosition, baseSlot + steps, speed, EaseKind.OutQuad);
        }

        /// <returns>True on the tick the reel comes to rest.</returns>
        private bool AdvanceReelStop(float dt)
        {
            if (!_reelStop.IsDone)
            {
                _reelStop.Advance(dt);
                ReelPosition = _reelStop.Position;
                if (_reelStop.IsDone)
                    _reelSettle = new DecelerationMotion(ReelPosition, _reelTarget, _settings.ReelSettleTime, EaseKind.OutQuad);
                return false;
            }

            _reelSettle.Advance(dt);
            ReelPosition = _reelSettle.Position;
            if (!_reelSettle.IsDone) return false;

            ReelPosition = Mod(_reelTarget, RingLayout.IconCount);
            _reelSpeed = 0f;
            return true;
        }

        private void AdvanceRingStop(float dt)
        {
            _ringStop.Advance(dt);
            RingPosition = _ringStop.Position;
        }

        private void FinishRound()
        {
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
