namespace AutoccultistNS.UI
{
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tokens;
    using SecretHistories.Commands;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class ImperativeListWindow : ViewWindow<ImperativeListWindow.IWindowContext>, ImperativeListWindow.IWindowContext
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
            if (!imperative.IsConditionMet(GameStateProvider.Current))
            {
                return;
            }

            var tabletop = GameAPI.TabletopSphere;

            // HACK FIXME: I am completely unable to find how dropzones function in the game.  But I can find the payload...
            // Let's look for our dropzone, and just use it's location.
            // This will NOT function as a dropzone, as it will spawn circularly around the dropzone's origin instead of choosing a point
            // arranged within the dropzone itself.
            var dz = GameAPI.TabletopSphere.GetTokens().FirstOrDefault(x => x.PayloadId == "!dropzone_Situation");
            var location = new TokenLocation(dz?.transform.localPosition ?? Vector3.zero, tabletop);
            var quickDuration = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultQuickTravelDuration;
            var creationCommand = new AutomationCreationCommand(imperative.Id);
            var zoneTokenCreationCommand = new TokenCreationCommand(creationCommand, location).WithDestination(location, quickDuration);
            var token = zoneTokenCreationCommand.Execute(Context.Unknown(), tabletop);
            if (token.IsValid())
            {
                SoundManager.PlaySfx("SituationTokenSpawn");
            }
        }
    }
}
