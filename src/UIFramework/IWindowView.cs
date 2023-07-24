namespace AutoccultistNS.UI
{
    using UnityEngine;

    public interface IWindowView<TWindowContext>
        where TWindowContext : IWindowViewHost<TWindowContext>
    {
        void Attach(TWindowContext window);

        void Update();

        void Detatch();
    }
}
