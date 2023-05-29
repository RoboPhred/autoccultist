namespace AutoccultistNS.GameState.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Assets.Scripts.Application.UI;
    using SecretHistories.Tokens.Payloads;

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

            var activeDoor = Reflection.GetPrivateField<Ingress>(otherworld, "_activeIngress");
            if (activeDoor == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            throw new NotImplementedException("Mansus state");

            // this.IsActive = true;

            // var dominion = activeDoor.Dominions

            // var cards = Reflection.GetPrivateField<ElementStackToken[]>(otherworld, "cards");

            // this.FaceUpCard = CardStateImpl.CardStatesFromStack(cards[0], CardLocation.Mansus).First();

            // this.DeckCards = new Dictionary<string, ICardState>
            // {
            //     { activeDoor.GetDeckName(0), this.FaceUpCard },
            //     { activeDoor.GetDeckName(1), CardStateImpl.CardStatesFromStack(cards[1], CardLocation.Mansus).First() },
            //     { activeDoor.GetDeckName(2), CardStateImpl.CardStatesFromStack(cards[2], CardLocation.Mansus).First() },
            // };
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
