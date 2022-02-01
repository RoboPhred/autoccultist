namespace Autoccultist.Actor.Actions
{
    using Autoccultist.Brain;
    using Autoccultist.GameState;

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
            var gameState = GameStateProvider.Current;

            if (!gameState.Mansus.IsActive)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: No mansus visit is in progress.");
            }

            if (this.MansusSolution.FaceUpCard?.ChooseCard(new[] { gameState.Mansus.FaceUpCard }) != null)
            {
                // This is the card we want.
                GameAPI.ChooseMansusCard(gameState.Mansus.FaceUpCard.ToElementStack());
            }
            else if (gameState.Mansus.DeckCards.TryGetValue(this.MansusSolution.Deck, out var card))
            {
                // This is the card we want.
                GameAPI.ChooseMansusCard(card.ToElementStack());
            }
            else
            {
                throw new ActionFailureException(this, $"ChooseMansusCardAction: Deck {this.MansusSolution.Deck} is not available.");
            }
        }
    }
}
