namespace AutoccultistNS.UI
{
    using UnityEngine;

    public interface IWindowView<TWindowContext>
        where TWindowContext : IWindowViewHost<TWindowContext>
    {
        string Title { get; }

        Sprite Icon { get; }

        void Attach(TWindowContext window);

        void Update();

        void Detatch();
    }
}
