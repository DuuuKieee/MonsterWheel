using System.Collections.Generic;
using MonsterWheel.Core;
using UnityEngine;

namespace MonsterWheel.View
{
    [CreateAssetMenu(menuName = "Monster Wheel/Wheel Config", fileName = "WheelConfig")]
    public sealed class WheelConfig : ScriptableObject
    {
        [Tooltip("Exactly 9 monster icons. Ring slot i shows icon i mod 9.")]
        public Sprite[] Icons = new Sprite[RingLayout.IconCount];

        [Header("Motion")]
        [Tooltip("Ring light speed while spinning (slots per second).")]
        public float RingSpinSpeed = 18f;
        [Tooltip("Reel speed while spinning (icons per second).")]
        public float ReelSpinSpeed = 24f;
        public float SpinUpTime = 0.3f;
        public float ReelBackOvershoot = 1.2f;
        public float HandoffDelay = 0.15f;
        [Range(0.05f, 1f)] public float MinStopSpeedRatio = 0.5f;

        [Header("Glow")]
        public float TrailDecay = 0.25f;
        public float PulsePeriod = 0.6f;
        public float ActiveScale = 1.25f;
        public float PulseScale = 1.4f;
        public Color DimColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        public Color HaloColor = new Color(1f, 0.85f, 0.3f, 0.9f);

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (Icons == null || Icons.Length != RingLayout.IconCount)
                errors.Add($"Icons must contain exactly {RingLayout.IconCount} sprites.");
            else
                for (var i = 0; i < Icons.Length; i++)
                    if (Icons[i] == null) errors.Add($"Icons[{i}] is missing.");

            void Positive(float v, string name)
            {
                if (!(v > 0f)) errors.Add($"{name} must be > 0.");
            }

            Positive(RingSpinSpeed, nameof(RingSpinSpeed));
            Positive(ReelSpinSpeed, nameof(ReelSpinSpeed));
            Positive(SpinUpTime, nameof(SpinUpTime));
            Positive(ReelBackOvershoot, nameof(ReelBackOvershoot));
            Positive(HandoffDelay, nameof(HandoffDelay));
            Positive(MinStopSpeedRatio, nameof(MinStopSpeedRatio));
            Positive(TrailDecay, nameof(TrailDecay));
            Positive(PulsePeriod, nameof(PulsePeriod));
            Positive(ActiveScale, nameof(ActiveScale));
            Positive(PulseScale, nameof(PulseScale));
            return errors;
        }

        public WheelSettings ToSettings()
        {
            var s = WheelSettings.Default;
            s.RingSpinSpeed = RingSpinSpeed;
            s.ReelSpinSpeed = ReelSpinSpeed;
            s.SpinUpTime = SpinUpTime;
            s.ReelBackOvershoot = ReelBackOvershoot;
            s.HandoffDelay = HandoffDelay;
            s.MinStopSpeedRatio = MinStopSpeedRatio;
            return s;
        }

        private void OnValidate()
        {
            foreach (var error in Validate())
                Debug.LogWarning($"[WheelConfig] {error}", this);
        }
    }
}
