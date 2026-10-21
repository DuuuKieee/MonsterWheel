using MonsterWheel.Core;
using UnityEngine;

namespace MonsterWheel.View
{
    /// <summary>
    /// Plays the spin sound in two parts cut from one clip: the steady click section loops while the
    /// wheel spins, and the slow-down tail is scheduled so its last click lands when the ring stops.
    /// </summary>
    public sealed class SpinAudio : MonoBehaviour
    {
        public AudioSource LoopSource;
        public AudioSource TailSource;

        // Schedule the tail slightly ahead so the audio thread starts it on time.
        private const float ScheduleLead = 0.1f;

        private float _tailLength;
        private bool _tailStarted;
        private WheelState _lastState = WheelState.Idle;

        public void Init(WheelConfig cfg)
        {
            var clip = cfg.SpinClip;
            if (clip == null || LoopSource == null || TailSource == null)
            {
                enabled = false;
                return;
            }

            if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            Setup(LoopSource, Slice(clip, cfg.SpinLoopStart, cfg.SpinLoopEnd, "SpinLoop"), true, cfg.SpinVolume);
            Setup(TailSource, Slice(clip, cfg.SpinLoopEnd, clip.length, "SpinTail"), false, cfg.SpinVolume);
            _tailLength = TailSource.clip.length;
        }

        public void Render(WheelEngine engine)
        {
            var state = engine.State;

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

            _lastState = state;
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

        /// <summary>Copies [start, end) seconds of <paramref name="source"/> into a new clip (end clamped to the clip).</summary>
        public static AudioClip Slice(AudioClip source, float start, float end, string name)
        {
            var channels = source.channels;
            var frequency = source.frequency;
            var first = Mathf.Clamp(Mathf.RoundToInt(start * frequency), 0, source.samples - 1);
            var last = Mathf.Clamp(Mathf.RoundToInt(end * frequency), first + 1, source.samples);
            var data = new float[(last - first) * channels];
            source.GetData(data, first);
            var clip = AudioClip.Create(name, last - first, channels, frequency, false);
            clip.SetData(data, 0);
            return clip;
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
