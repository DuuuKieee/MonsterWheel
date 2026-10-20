namespace MonsterWheel.Core
{
    public enum EaseKind
    {
        OutQuad,
        OutCubic,
        OutBack
    }

    public static class Easing
    {
        public static float OutQuad(float t)
        {
            t = Clamp01(t);
            var u = 1f - t;
            return 1f - u * u;
        }

        public static float OutCubic(float t)
        {
            t = Clamp01(t);
            var u = 1f - t;
            return 1f - u * u * u;
        }

        /// <summary>Ease-out with a small overshoot past 1 before settling; <paramref name="s"/> controls overshoot.</summary>
        public static float OutBack(float t, float s)
        {
            t = Clamp01(t) - 1f;
            return 1f + (s + 1f) * t * t * t + s * t * t;
        }

        public static float Evaluate(EaseKind kind, float t, float backOvershoot)
        {
            switch (kind)
            {
                case EaseKind.OutQuad: return OutQuad(t);
                case EaseKind.OutBack: return OutBack(t, backOvershoot);
                default: return OutCubic(t);
            }
        }

        /// <summary>Derivative of the curve with respect to t.</summary>
        public static float Slope(EaseKind kind, float t, float backOvershoot)
        {
            t = Clamp01(t);
            var u = t - 1f;
            switch (kind)
            {
                case EaseKind.OutQuad: return -2f * u;
                case EaseKind.OutBack: return 3f * (backOvershoot + 1f) * u * u + 2f * backOvershoot * u;
                default: return 3f * u * u;
            }
        }

        private static float Clamp01(float t) => t < 0f ? 0f : t > 1f ? 1f : t;
    }
}
