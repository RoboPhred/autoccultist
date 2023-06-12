namespace AutoccultistNS.Actor.Actions
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;

    /// <summary>
    /// An action that closes a situation window.
    /// </summary>
    public class ChooseMansusCardAction : SyncActionBase
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

        public override string ToString()
        {
            return $"ChooseMansusCardAction(MansusSolution = {this.MansusSolution})";
        }

        /// <inheritdoc/>
        protected override ActionResult OnExecute()
        {
            if (!GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: No mansus visit is in progress.");
            }

            if (!GameAPI.IsInteractable)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Game is not interactable.");
            }

            var gameState = GameStateProvider.Current;

            if (gameState.Mansus.State != PortalActiveState.AwaitingSelection)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Mansus is not awaiting selection.");
            }

            if (this.MansusSolution.FaceUpCard?.ChooseCard(new[] { gameState.Mansus.FaceUpCard }) != null)
            {
                Autoccultist.Instance.LogTrace("Choosing face up card from mansus.");

                // This is the card we want.
                GameAPI.ChooseMansusDeck(gameState.Mansus.FaceUpDeck);
            }
            else if (gameState.Mansus.DeckCards.TryGetValue(this.MansusSolution.Deck, out var card))
            {
                Autoccultist.Instance.LogTrace($"Choosing deck {this.MansusSolution.Deck} from mansus.");

                // This is the card we want.
                GameAPI.ChooseMansusDeck(this.MansusSolution.Deck);
            }
            else
            {
                throw new ActionFailureException(this, $"ChooseMansusCardAction: Deck {this.MansusSolution.Deck} is not available.  Available decks: {string.Join(", ", gameState.Mansus.DeckCards.Keys)}");
            }

            return ActionResult.Completed;
        }
    }
}
