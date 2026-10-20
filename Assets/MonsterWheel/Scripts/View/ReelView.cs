using UnityEngine;
using UnityEngine.UI;

namespace MonsterWheel.View
{
    /// <summary>
    /// Vertical reel inside a masked frame. Items are recycled: item k (k = -2..2) shows icon
    /// (floor(pos) + k) mod 9 at y = (k - frac(pos)) * ItemHeight, so increasing pos scrolls down.
    /// </summary>
    public sealed class ReelView : MonoBehaviour
    {
        public Image[] Items;
        public float ItemHeight = 120f;
        [Tooltip("Vertical stretch at full speed, faking motion blur.")]
        public float BlurStretch = 0.25f;

        private Sprite[] _icons;

        public void Init(Sprite[] icons) => _icons = icons;

        public void Render(float reelPos, float speedRatio)
        {
            var baseIndex = Mathf.FloorToInt(reelPos);
            var frac = reelPos - baseIndex;
            var half = Items.Length / 2;
            var stretch = speedRatio > 0.5f ? 1f + BlurStretch * speedRatio : 1f;

            for (var j = 0; j < Items.Length; j++)
            {
                var k = j - half;
                var item = Items[j];
                item.sprite = _icons[Mod(baseIndex + k, _icons.Length)];
                item.rectTransform.anchoredPosition = new Vector2(0f, (k - frac) * ItemHeight);
                item.rectTransform.localScale = new Vector3(1f, stretch, 1f);
            }
        }

        private static int Mod(int value, int length)
        {
            var m = value % length;
            return m < 0 ? m + length : m;
        }
    }
}
