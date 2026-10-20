namespace MonsterWheel.Core
{
    public interface IRandom
    {
        /// <summary>Returns an int in [0, maxExclusive).</summary>
        int Next(int maxExclusive);
    }

    public sealed class SystemRandom : IRandom
    {
        private readonly System.Random _random;

        public SystemRandom() => _random = new System.Random();
        public SystemRandom(int seed) => _random = new System.Random(seed);

        public int Next(int maxExclusive) => _random.Next(maxExclusive);
    }
}
