namespace MonsterWheel.Core
{
    public enum EaseKind
    {
        OutCubic,
        OutBack
    }

    public static class Easing
    {
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

        public static float Evaluate(EaseKind kind, float t, float backOvershoot) =>
            kind == EaseKind.OutBack ? OutBack(t, backOvershoot) : OutCubic(t);

        private static float Clamp01(float t) => t < 0f ? 0f : t > 1f ? 1f : t;
    }
}
