namespace AutoccultistNS.Actor.Actions
{
    using System.Threading;
    using System.Threading.Tasks;
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

        public override string ToString()
        {
            return $"ChooseMansusCardAction(MansusSolution = {this.MansusSolution})";
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            if (!GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: No mansus visit is in progress.");
            }

            await this.AwaitMansusReady(cancellationToken);

            this.ChooseCard();

            await this.AcceptCard(cancellationToken);

            return true;
        }

        private async Task AwaitMansusReady(CancellationToken cancellationToken)
        {
            var awaitInteractable = GameAPI.AwaitInteractable(cancellationToken);
            if (await Task.WhenAny(awaitInteractable, Task.Delay(10000, cancellationToken)) != awaitInteractable)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Timed out waiting for game to become interactable.");
            }

            // Give the mansus some time to animate its cards into being.
            // FIXME: Should include this in the Interactable check.
            await Task.Delay(1000, cancellationToken);
        }

        private void ChooseCard()
        {
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
        }

        private async Task AcceptCard(CancellationToken cancellationToken)
        {
            var awaitIngress = GameAPI.AwaitTabletopIngress(cancellationToken);
            if (await Task.WhenAny(awaitIngress, Task.Delay(5000, cancellationToken)) != awaitIngress)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Timed out waiting for ingress to appear.");
            }

            if (!GameAPI.EmptyMansusEgress())
            {
                throw new ActionFailureException(this, "Failed to empty the mansus.");
            }

            // Seems we get a lvl 3 unpause when the mansus completes.  Let's reassert the pause
            // so that the actor keeps control.
            GameAPI.ReassertPause();

            // Unpause from the user's perspective, so the game will resume once the actor
            // is done with its actions.
            GameAPI.UserUnpause();
        }
    }
}
