namespace MonsterWheel.Core
{
    /// <summary>Icon order of the ring slots. Slot 0 is top-left, slots are numbered clockwise.</summary>
    public static class RingLayout
    {
        public const int SlotCount = 18;
        public const int IconCount = 9;
        /// <summary>The icon sequence is rotated this many slots clockwise from the top-left corner.</summary>
        public const int ClockwiseShift = 2;

        /// <summary>
        /// Slot i shows icon (i - ClockwiseShift) mod 9, so each icon appears twice and opposite slots (i, i+9) match.
        /// </summary>
        public static int[] BuildIconOrder()
        {
            var order = new int[SlotCount];
            for (var i = 0; i < SlotCount; i++)
                order[i] = ((i - ClockwiseShift) % IconCount + IconCount) % IconCount;
            return order;
        }
    }
}
