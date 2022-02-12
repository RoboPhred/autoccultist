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
            consumedWidth = 0;
        }

        /// <summary>
        /// Gets a rect for a window of the given size.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <returns>The rect to use for the window.</returns>
        public static Rect GetWindowRect(float width, float height)
        {
            consumedWidth += width + 5;
            var x = new Rect(Screen.width - consumedWidth, 0, width, height);
            return x;
        }
    }
}
