namespace AutoccultistNS.UI
{
    public interface IWindowViewHost<TWindowContext>
        where TWindowContext : IWindowViewHost<TWindowContext>
    {
        /// <summary>
        /// Gets the content mount point for the window.
        /// </summary>
        WidgetMountPoint Content { get; }

        /// <summary>
        /// Gets the footer mount point for the window.
        /// </summary>
        WidgetMountPoint Footer { get; }

        /// <summary>
        /// Replace the current view with the new view.
        /// The view stack is mantained.
        /// </summary>
        void ReplaceView(IWindowView<TWindowContext> view);

        /// <summary>
        /// Push a new view onto the view stack.
        /// The current view will keep its state and be hidden.
        /// </summary>
        void PushView(IWindowView<TWindowContext> view);

        /// <summary>
        /// Restore the previous view in the stack.
        void PopView();
    }
}
