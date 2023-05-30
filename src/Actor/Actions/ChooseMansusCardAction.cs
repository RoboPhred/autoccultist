namespace AutoccultistNS.Actor.Actions
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;

    /// <summary>
    /// An action that closes a situation window.
    /// </summary>
    public class ChooseMansusCardAction : ActionBase
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

        /// <inheritdoc/>
        public override void Execute()
        {
            this.VerifyNotExecuted();

            if (!GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: No mansus visit is in progress.");
            }

            if (!GameAPI.IsInteractable)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Game is not interactable.");
            }

            var gameState = GameStateProvider.Current;

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
        }
    }
}
