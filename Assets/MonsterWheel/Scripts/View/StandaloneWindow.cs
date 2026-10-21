using MonsterWheel.Core;
using UnityEngine;

namespace MonsterWheel.View
{
    /// <summary>
    /// Desktop builds: open a 720x1280 window when the screen has room for it, otherwise the largest
    /// 9:16 window that fits the work area (taskbar excluded), centered. Runs on every launch, so it also
    /// overrides the size Unity remembers from the previous session.
    /// </summary>
    public static class StandaloneWindow
    {
        public const int TargetWidth = 720;
        public const int TargetHeight = 1280;

        // Space taken by the window frame and title bar around the game area.
        private const int FrameWidth = 16;
        private const int FrameHeight = 40;

#if UNITY_STANDALONE && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            var display = Screen.mainWindowDisplayInfo;
            var area = display.workArea;
            var (width, height) = WindowFit.Fit(area.width - FrameWidth, area.height - FrameHeight, TargetWidth, TargetHeight);
            Screen.SetResolution(width, height, FullScreenMode.Windowed);

            var x = area.x + (area.width - width - FrameWidth) / 2;
            var y = area.y + (area.height - height - FrameHeight) / 2;
            Screen.MoveMainWindowTo(display, new Vector2Int(Mathf.Max(area.x, x), Mathf.Max(area.y, y)));
        }
#endif
    }
}
