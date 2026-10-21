using MonsterWheel.Core;
using UnityEngine;

namespace MonsterWheel.View
{
    /// <summary>
    /// Plays the spin sound in two pre-cut parts: the steady click section loops while the wheel spins,
    /// and the slow-down tail is scheduled so its last click lands when the ring stops. A result sound
    /// plays once when the ring stops. Only plays ready-made clips, so it also works on WebGL.
    /// </summary>
    public sealed class SpinAudio : MonoBehaviour
    {
        public AudioSource LoopSource;
        public AudioSource TailSource;
        public AudioSource ResultSource;

        // Schedule the tail slightly ahead so the audio thread starts it on time.
        private const float ScheduleLead = 0.1f;

        private bool _hasSpin;
        private bool _hasResult;
        private float _tailLength;
        private bool _tailStarted;
        private WheelState _lastState = WheelState.Idle;

        public float TailLength => _tailLength;

        /// <summary>Number of times the result sound has been triggered.</summary>
        public int ResultPlays { get; private set; }

        public void Init(WheelConfig cfg)
        {
            _hasSpin = cfg.SpinLoopClip != null && cfg.SpinTailClip != null && LoopSource != null && TailSource != null;
            if (_hasSpin)
            {
                Setup(LoopSource, cfg.SpinLoopClip, true, cfg.SpinVolume);
                Setup(TailSource, cfg.SpinTailClip, false, cfg.SpinVolume);
                _tailLength = cfg.SpinTailLength;
            }

            _hasResult = cfg.ResultClip != null && ResultSource != null;
            if (_hasResult) Setup(ResultSource, cfg.ResultClip, false, cfg.ResultVolume);

            enabled = _hasSpin || _hasResult;
        }

        public void Render(WheelEngine engine)
        {
            var state = engine.State;

            if (_hasResult && state == WheelState.Idle && _lastState == WheelState.StoppingRing)
            {
                ResultSource.Play();
                ResultPlays++;
            }

            if (_hasSpin) RenderSpin(engine, state);
            _lastState = state;
        }

        private void RenderSpin(WheelEngine engine, WheelState state)
        {

            if (state == WheelState.Spinning && _lastState == WheelState.Idle)
            {
                TailSource.Stop();
                LoopSource.Play();
                _tailStarted = false;
            }
            else if ((state == WheelState.StoppingReel || state == WheelState.StoppingRing) && !_tailStarted)
            {
                var remaining = engine.RingStopTimeRemaining;
                if (remaining <= _tailLength + ScheduleLead) StartTail(remaining);
            }
            else if (state == WheelState.Idle && LoopSource.isPlaying)
            {
                LoopSource.Stop();
            }
        }

        /// <summary>Starts the tail so that it ends <paramref name="remaining"/> seconds from now.</summary>
        private void StartTail(float remaining)
        {
            _tailStarted = true;
            var startIn = remaining - _tailLength;
            if (startIn > 0f)
            {
                var at = AudioSettings.dspTime + startIn;
                TailSource.time = 0f;
                TailSource.PlayScheduled(at);
                LoopSource.SetScheduledEndTime(at);
            }
            else
            {
                // The stop is shorter than the tail: join the tail part-way so both still end together.
                TailSource.time = Mathf.Min(-startIn, _tailLength);
                TailSource.Play();
                LoopSource.Stop();
            }
        }

        private static void Setup(AudioSource source, AudioClip clip, bool loop, float volume)
        {
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.playOnAwake = false;
        }
    }
}
