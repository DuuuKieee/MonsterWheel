using UnityEngine;

namespace MonsterWheel.View
{
    /// <summary>Renders the 18 ring slots: the lit slot, a fading trail behind it, and a pulse on the result.</summary>
    public sealed class RingView : MonoBehaviour
    {
        public SlotView[] Slots;

        private WheelConfig _config;
        private float[] _glow;
        private float _pulseTime;

        public void Init(int[] layout, WheelConfig cfg)
        {
            _config = cfg;
            _glow = new float[Slots.Length];
            for (var i = 0; i < Slots.Length; i++)
                Slots[i].Icon.sprite = cfg.Icons[layout[i]];
        }

        public void Render(int activeSlot, bool showResult, float dt)
        {
            var decay = dt / _config.TrailDecay;
            for (var i = 0; i < _glow.Length; i++)
                _glow[i] = i == activeSlot ? 1f : Mathf.Max(0f, _glow[i] - decay);

            _pulseTime = showResult ? _pulseTime + dt : 0f;
            var pulse = 1f;
            if (showResult)
            {
                var wave = 0.5f - 0.5f * Mathf.Cos(_pulseTime / _config.PulsePeriod * 2f * Mathf.PI);
                pulse = Mathf.Lerp(1f, _config.PulseScale / _config.ActiveScale, wave);
            }

            for (var i = 0; i < Slots.Length; i++)
                Slots[i].SetGlow(_glow[i], i == activeSlot ? pulse : 1f, _config);
        }
    }
}
