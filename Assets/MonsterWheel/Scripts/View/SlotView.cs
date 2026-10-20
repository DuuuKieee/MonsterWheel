using UnityEngine;
using UnityEngine.UI;

namespace MonsterWheel.View
{
    /// <summary>One ring slot: a monster icon with a halo behind it.</summary>
    public sealed class SlotView : MonoBehaviour
    {
        public Image Icon;
        public Image Halo;

        /// <summary>Applies the glow level (0 = dimmed, 1 = lit) times an extra scale multiplier.</summary>
        public void SetGlow(float level, float scaleMultiplier, WheelConfig cfg)
        {
            Icon.color = Color.Lerp(cfg.DimColor, Color.white, level);
            var halo = cfg.HaloColor;
            halo.a *= level;
            Halo.color = halo;
            transform.localScale = Vector3.one * (Mathf.Lerp(1f, cfg.ActiveScale, level) * scaleMultiplier);
        }
    }
}
