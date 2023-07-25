namespace AutoccultistNS.UI
{
    using UnityEngine;

    /// <summary>
    /// Interface for a view that can override the view host's icon.
    /// </summary>
    public interface IViewHasIcon
    {
        Sprite Icon { get; }
    }
}
