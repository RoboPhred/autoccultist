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
            // GUIUtility.GetControlID(FocusType.Passive) seems to return the same value on occasions...
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
            // FIXME: Since switching to this, random windows refuse to coexist with others.
            // It might be something to do with windows getting too close together, although I am certain
            // their rects do not overlap.
            var x = new Rect(Screen.width - consumedWidth - width - 10, 30, width, height);
            consumedWidth += width + 20;
            return x;
        }
    }
}
