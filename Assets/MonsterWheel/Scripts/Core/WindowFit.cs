using System;

namespace MonsterWheel.Core
{
    public static class WindowFit
    {
        /// <summary>
        /// The target size if it fits in the available area; otherwise the largest size with the same
        /// aspect ratio that does.
        /// </summary>
        public static (int width, int height) Fit(int availableWidth, int availableHeight, int targetWidth, int targetHeight)
        {
            if (availableWidth >= targetWidth && availableHeight >= targetHeight)
                return (targetWidth, targetHeight);

            var scale = Math.Min(availableWidth / (double)targetWidth, availableHeight / (double)targetHeight);
            return ((int)Math.Floor(targetWidth * scale), (int)Math.Floor(targetHeight * scale));
        }
    }
}
