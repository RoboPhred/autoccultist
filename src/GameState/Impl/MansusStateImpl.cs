namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Assets.Scripts.Application.UI;

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
            // This is silly.  We are in the mansus, but we are not ready yet.
            if (otherworld == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            this.IsActive = true;

            var deckStacks = GameAPI.GetMansusChoices(out var faceUpDeckName);
            if (deckStacks != null)
            {
                this.FaceUpDeck = faceUpDeckName;
                this.DeckCards = (IReadOnlyDictionary<string, ICardState>)deckStacks.ToDictionary(x => x.Key, x => (ICardState)CardStateImpl.CardStatesFromStack(x.Value, CardLocation.Mansus).First());
            }
            else
            {
                this.FaceUpDeck = null;
                this.DeckCards = null;
            }
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
