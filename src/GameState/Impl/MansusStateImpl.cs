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
            if (otherworld == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            var deckStacks = GameAPI.GetMansusChoices(out var faceUpDeckName);

            this.IsActive = true;
            this.FaceUpDeck = faceUpDeckName;
            this.DeckCards = (IReadOnlyDictionary<string, ICardState>)deckStacks.ToDictionary(x => x.Key, x => CardStateImpl.CardStatesFromStack(x.Value, CardLocation.Mansus).First());
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
