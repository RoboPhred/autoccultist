namespace AutoccultistNS.GUI
{
    using UnityEngine;

    /// <summary>
    /// Handles window tiling.
    /// </summary>
    public static class WindowManager
    {
        private static int nextWindowId = 0;
        private static float consumedWidth = 0;

        /// <summary>
        /// Gets the next available window id.
        /// </summary>
        /// <returns>The new window id.</returns>
        public static int GetNextWindowID()
        {
            // GUIUtility.GetControlID(FocusType.Passive) seems to return the same value on occasion...
            return nextWindowId++;
        }

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
            Rect x;
            x = new Rect(consumedWidth + 10, 30, width, height);
            consumedWidth += width + 20;
            return x;
        }
    }
}
