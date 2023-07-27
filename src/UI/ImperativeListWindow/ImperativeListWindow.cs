namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Tokens;
    using Roost.Piebald;
    using UnityEngine;

    public class ImperativeListWindow : AbstractMetaViewWindow<ImperativeListWindow.IWindowContext>, ImperativeListWindow.IWindowContext
    {
        public interface IWindowContext : IWindowViewHost<IWindowContext>
        {
            void StartImperative(IImperative imperative);
        }

        protected override int DefaultWidth => 500;

        protected override int DefaultHeight => 800;

        protected override string DefaultTitle => "Automations";

        protected override Sprite DefaultIcon => ResourcesManager.GetSpriteForUI("autoccultist_gears");

        protected override IWindowView<IWindowContext> DefaultView => new ImperativeArcsGoalsView();

        public void StartImperative(IImperative imperative)
        {
            AutomationTokenFactory.CreateToken(imperative);
        }
    }
}
