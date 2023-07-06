namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Tokens.Payloads;
    using SecretHistories.UI;

    /// <summary>
    /// Implements <see cref="IPortalState"/>.
    /// </summary>
    internal class PortalStateImpl : GameStateObject, IPortalState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalStateImpl"/> class.
        /// </summary>
        /// <param name="otherworld">The map controller to load state from.</param>
        private PortalStateImpl(Ingress ingress)
        {
            // This is silly.  We are in the mansus, but we are not ready yet.
            if (ingress == null)
            {
                this.State = PortalActiveState.Closed;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            this.PortalId = ingress.GetOtherworldId();

            var output = ingress.GetEgressOutputSphere();
            if (output == null)
            {
                Autoccultist.LogWarn($"Open ingress {ingress.EntityId} has no output sphere.");
                this.State = PortalActiveState.Closed;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            // I think the token sits straight in the output sphere, so .Tokens should work?
            var outputStacks = output.GetTokens().Select(x => x.Payload).OfType<ElementStack>().ToList();
            if (outputStacks.Count > 0)
            {
                this.State = PortalActiveState.AwaitingCollection;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                this.OutputCard = CardStateImpl.CardStatesFromStack(outputStacks.First(), CardLocation.Mansus, null).First();
            }
            else
            {
                var deckStacks = GameAPI.GetMansusChoices(out var faceUpDeckName);
                if (deckStacks != null)
                {
                    this.State = PortalActiveState.AwaitingSelection;
                    this.FaceUpDeck = faceUpDeckName;
                    this.DeckCards = (IReadOnlyDictionary<string, ICardState>)deckStacks.ToDictionary(x => x.Key, x => (ICardState)CardStateImpl.CardStatesFromStack(x.Value, CardLocation.Mansus, null).First());
                    this.FaceUpCard = this.DeckCards[faceUpDeckName];
                }
                else
                {
                    this.State = PortalActiveState.Transitioning;
                    this.FaceUpDeck = null;
                    this.DeckCards = null;
                    this.FaceUpCard = null;
                }
            }
        }

        /// <inheritdoc/>
        public PortalActiveState State { get; }

        /// <inheritdoc/>
        public string PortalId { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, ICardState> DeckCards { get; }

        /// <inheritdoc/>
        public string FaceUpDeck { get; }

        /// <inheritdoc/>
        public ICardState FaceUpCard { get; }

        /// <inheritdoc/>
        public ICardState OutputCard { get; }

        protected override int ComputeContentHash()
        {
            return HashUtils.Hash(
                this.State,
                this.PortalId,
                HashUtils.HashAllUnordered(this.DeckCards?.Select(x => x.Key + x.Value) ?? Enumerable.Empty<string>()),
                this.OutputCard);
        }

        internal static IPortalState FromCurrentState()
        {
            var ingress = GameAPI.GetActiveIngress();
            return new PortalStateImpl(ingress);
        }
    }
}
