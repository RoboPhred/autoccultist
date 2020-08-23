namespace Autoccultist.Actor.Actions
{
    using Assets.CS.TabletopUI;
    using Assets.TabletopUi.Scripts.Infrastructure;
    using Autoccultist.Brain;
    using Autoccultist.GameState;
    using Autoccultist.GameState.Impl;

    /// <summary>
    /// An action that closes a situation window.
    /// </summary>
    public class ChooseMansusCardAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseMansusCardAction"/> class.
        /// </summary>
        /// <param name="mansusSolution">The mansus solution to this mansus event.</param>
        public ChooseMansusCardAction(IMansusSolution mansusSolution)
        {
            this.MansusSolution = mansusSolution;
        }

        /// <summary>
        /// Gets the mansus solution used by this action.
        /// </summary>
        public IMansusSolution MansusSolution { get; }

        // FIXME: Clean up all this reflection!  Move stuff into GameAPI or IGameState.

        /// <inheritdoc/>
        public void Execute()
        {
            var mapController = Registry.Retrieve<MapController>();
            var activeDoor = GameAPI.TabletopManager.mapTokenContainer.GetActiveDoor();

            var cards = Reflection.GetPrivateField<ElementStackToken[]>(mapController, "cards");

            // FIXME: Move mansus stuff into IGameState, avoid instantiating state objects here.
            var faceUpStates = CardStateImpl.CardStatesFromStack(cards[0]);

            // Card 0 is always the face up card.
            if (this.MansusSolution.FaceUpCard?.ChooseCard(faceUpStates) != null)
            {
                // This is the card we want.
                mapController.HideMansusMap(activeDoor.transform, cards[0]);
            }
            else
            {
                if (this.MansusSolution.Deck == activeDoor.GetDeckName(0))
                {
                    mapController.HideMansusMap(activeDoor.transform, cards[0]);
                }
                else if (this.MansusSolution.Deck == activeDoor.GetDeckName(1))
                {
                    mapController.HideMansusMap(activeDoor.transform, cards[1]);
                }
                else if (this.MansusSolution.Deck == activeDoor.GetDeckName(2))
                {
                    mapController.HideMansusMap(activeDoor.transform, cards[2]);
                }
                else
                {
                    AutoccultistPlugin.Instance.LogWarn($"ChooseMansusCardAction: Deck {this.MansusSolution.Deck} is not available on the mansus.");
                }
            }
        }
    }
}
