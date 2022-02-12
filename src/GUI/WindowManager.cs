namespace Autoccultist.GUI
{
    using UnityEngine;

    /// <summary>
    /// Handles window tiling.
    /// </summary>
    public static class WindowManager
    {
        private static float consumedWidth = 0;

        /// <summary>
        /// Prepares the window manager for a gui render pass.
        /// </summary>
        public static void OnPreGUI()
        {
            consumedWidth = Screen.width - 10;
        }

        /// <summary>
        /// Gets a rect for a window of the given size.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <returns>The rect to use for the window.</returns>
        public static Rect GetWindowRect(float width, float height)
        {
            var rect = new Rect(Screen.width - consumedWidth, 0, width, height);
            consumedWidth += width + 5;
            return rect;
        }
    }
}
