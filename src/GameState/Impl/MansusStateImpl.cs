namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Assets.Scripts.Application.UI;
    using SecretHistories.Entities;
    using SecretHistories.Tokens.Payloads;
    using SecretHistories.UI;

    /// <summary>
    /// Implements <see cref="IMansusState"/>.
    /// </summary>
    internal class MansusStateImpl : IMansusState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MansusStateImpl"/> class.
        /// </summary>
        /// <param name="otherworld">The map controller to load state from.</param>
        public MansusStateImpl(Otherworld otherworld)
        {
            if (otherworld == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            var compendium = Watchman.Get<Compendium>();

            var portal = compendium.GetEntityById<Portal>(otherworld.EntityId);
            if (portal == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            var activeDoor = Reflection.GetPrivateField<Ingress>(otherworld, "_activeIngress");
            if (activeDoor == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            this.IsActive = true;

            var spheres = activeDoor.GetSpheres();
            var consequences = activeDoor.GetConsequences();

            var faceUpToken = spheres[0].GetTokens().First().Payload as ElementStack;
            var faceUpDeckName = compendium.GetEntityById<Recipe>(consequences[0].Id).DeckEffects.Keys.First();

            var deckOneToken = spheres[1].GetTokens().First().Payload as ElementStack;
            var deckOneName = compendium.GetEntityById<Recipe>(consequences[1].Id).DeckEffects.Keys.First();

            var deckTwoToken = spheres[2].GetTokens().First().Payload as ElementStack;
            var deckTwoName = compendium.GetEntityById<Recipe>(consequences[2].Id).DeckEffects.Keys.First();

            this.FaceUpCard = CardStateImpl.CardStatesFromStack(faceUpToken, CardLocation.Mansus).First();

            this.DeckCards = new Dictionary<string, ICardState>
            {
                { faceUpDeckName, this.FaceUpCard },
                { deckOneName, CardStateImpl.CardStatesFromStack(deckOneToken, CardLocation.Mansus).First() },
                { deckTwoName, CardStateImpl.CardStatesFromStack(deckTwoToken, CardLocation.Mansus).First() },
            };
        }

        /// <inheritdoc/>
        public bool IsActive { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, ICardState> DeckCards { get; }

        /// <inheritdoc/>
        public string FaceUpDeck { get; }

        /// <inheritdoc/>
        public ICardState FaceUpCard { get; }
    }
}
