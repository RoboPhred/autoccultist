namespace AutoccultistNS.Tokens
{
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using SecretHistories.Commands;
    using SecretHistories.Entities;
    using SecretHistories.Entities.NullEntities;
    using SecretHistories.UI;
    using UnityEngine;

    public static class AutomationTokenFactory
    {
        public static Token CreateToken(IImperative imperative)
        {
            if (!imperative.IsConditionMet(GameStateProvider.Current))
            {
                return NullToken.Create();
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

            return token;
        }
    }
}
