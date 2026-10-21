using MonsterWheel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterWheel.View
{
    /// <summary>Glue between input, the rules engine and the views. Space starts / stops the wheel.</summary>
    public sealed class MonsterWheelController : MonoBehaviour
    {
        public WheelConfig Config;
        public RingView Ring;
        public ReelView Reel;
        [Tooltip("Optional spin sound.")]
        public SpinAudio Audio;

        private WheelEngine _engine;

        public WheelEngine Engine => _engine;

        private void Awake()
        {
            if (!IsSetUpCorrectly())
            {
                enabled = false;
                return;
            }

            _engine = new WheelEngine(Config.ToSettings(), new SystemRandom());
            Ring.Init(_engine.Layout, Config);
            Reel.Init(Config.Icons);
            Render(0f);
            InitAudio();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                _engine.Press();

            _engine.Tick(Time.deltaTime);
            Render(Time.deltaTime);
        }

        /// <summary>Sound is optional: a platform audio problem must never stop the wheel from working.</summary>
        private void InitAudio()
        {
            if (Audio == null) return;
            try
            {
                Audio.Init(Config);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, Audio);
                Audio.enabled = false;
            }
        }

        private void Render(float dt)
        {
            Ring.Render(_engine.ActiveSlot, _engine.HasResult, dt);
            Reel.Render(_engine.ReelPosition, _engine.ReelSpeedRatio);
            if (Audio != null && Audio.enabled) Audio.Render(_engine);
        }

        private bool IsSetUpCorrectly()
        {
            var ok = true;
            if (Config == null) { Debug.LogError("[MonsterWheel] Config is not assigned.", this); return false; }
            foreach (var error in Config.Validate())
            {
                Debug.LogError($"[MonsterWheel] {error}", Config);
                ok = false;
            }
            if (Ring == null || Ring.Slots == null || Ring.Slots.Length != RingLayout.SlotCount)
            {
                Debug.LogError($"[MonsterWheel] RingView needs exactly {RingLayout.SlotCount} slots.", this);
                ok = false;
            }
            if (Reel == null || Reel.Items == null || Reel.Items.Length == 0)
            {
                Debug.LogError("[MonsterWheel] ReelView has no items.", this);
                ok = false;
            }
            return ok;
        }
    }
}
