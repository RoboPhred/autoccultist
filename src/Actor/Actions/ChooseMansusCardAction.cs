namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
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
            try
            {
                await RealtimeDelay.Timeout(c => GameAPI.AwaitInteractable(c), TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (TimeoutException)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Timed out waiting for game to become interactable.");
            }

            // Give the mansus some time to animate its cards into being.
            // FIXME: Should include this in the Interactable check.
            await RealtimeDelay.Of(1000, cancellationToken);
        }

        private void ChooseCard()
        {
            var state = GameStateProvider.Current;

            if (state.Mansus.State != PortalActiveState.AwaitingSelection)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Mansus is not awaiting selection.");
            }

            if (this.MansusSolution.FaceUpCard?.ChooseCard(new[] { state.Mansus.FaceUpCard }, state) != null)
            {
                Autoccultist.LogTrace("Choosing face up card from mansus.");

                // This is the card we want.
                GameAPI.ChooseMansusDeck(state.Mansus.FaceUpDeck);
            }
            else if (state.Mansus.DeckCards.TryGetValue(this.MansusSolution.Deck, out var card))
            {
                Autoccultist.LogTrace($"Choosing deck {this.MansusSolution.Deck} from mansus.");

                // This is the card we want.
                GameAPI.ChooseMansusDeck(this.MansusSolution.Deck);
            }
            else
            {
                throw new ActionFailureException(this, $"ChooseMansusCardAction: Deck {this.MansusSolution.Deck} is not available.  Available decks: {string.Join(", ", state.Mansus.DeckCards.Keys)}");
            }
        }

        private async Task AcceptCard(CancellationToken cancellationToken)
        {
            try
            {
                await RealtimeDelay.Timeout(c => GameAPI.AwaitTabletopIngress(c), TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (TimeoutException)
            {
                throw new ActionFailureException(this, "ChooseMansusCardAction: Timed out waiting for ingress to appear.");
            }

            var sphere = GameAPI.GetMansusEgressSphere();

            if (sphere == null)
            {
                throw new ActionFailureException(this, "Failed to find mansus egress sphere.");
            }

            if (sphere.Tokens.Count > 0 && AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var shroudedTokens = sphere.Tokens.Where(x => x.Shrouded).ToArray();
                foreach (var token in shroudedTokens)
                {
                    token.Unshroud();
                }

                if (shroudedTokens.Length > 0)
                {
                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }

                sphere.EvictAllTokens(new Context(Context.ActionSource.PlayerDumpAll));
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
