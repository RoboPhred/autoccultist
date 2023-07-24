namespace AutoccultistNS.UI
{
    public interface IWindowViewHost<TWindowContext>
        where TWindowContext : IWindowViewHost<TWindowContext>
    {
        WidgetMountPoint Content { get; }

        WidgetMountPoint Footer { get; }

        void ReplaceView(IWindowView<TWindowContext> view);

        void PushView(IWindowView<TWindowContext> view);

        void PopView();
    }
}
