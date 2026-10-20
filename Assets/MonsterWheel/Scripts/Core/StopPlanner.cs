using System;

namespace MonsterWheel.Core
{
    /// <summary>Decides how far the ring light must travel to stop on a given icon.</summary>
    public static class StopPlanner
    {
        /// <summary>
        /// Smallest step count n >= layout.Length * minLaps such that the slot reached by moving
        /// n steps clockwise from <paramref name="currentSlot"/> shows <paramref name="targetIcon"/>.
        /// </summary>
        public static int StepsToTarget(int currentSlot, int targetIcon, int[] layout, int minLaps = 1)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            return FirstMatchAtOrAfter(currentSlot, targetIcon, layout, layout.Length * minLaps);
        }

        /// <summary>
        /// Smallest step count n >= <paramref name="minSteps"/> such that the slot reached by moving
        /// n steps clockwise from <paramref name="currentSlot"/> shows <paramref name="targetIcon"/>.
        /// </summary>
        public static int FirstMatchAtOrAfter(int currentSlot, int targetIcon, int[] layout, int minSteps)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            var len = layout.Length;
            if (currentSlot < 0 || currentSlot >= len)
                throw new ArgumentOutOfRangeException(nameof(currentSlot), currentSlot, "Slot outside ring.");
            if (minSteps < 0) minSteps = 0;

            for (var n = minSteps; n < minSteps + len; n++)
            {
                if (layout[(currentSlot + n) % len] == targetIcon)
                    return n;
            }

            throw new ArgumentException($"Icon {targetIcon} is not on the ring.", nameof(targetIcon));
        }
    }
}
