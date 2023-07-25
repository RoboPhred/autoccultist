namespace AutoccultistNS.UI
{

    /// <summary>
    /// Interface for a view that can show contents in a <see cref="ViewWindow"/>.
    /// </summary>
    public interface IWindowView<TWindowContext>
        where TWindowContext : IWindowViewHost<TWindowContext>
    {
        void Attach(TWindowContext window);

        void Update();

        void Detatch();
    }
}
