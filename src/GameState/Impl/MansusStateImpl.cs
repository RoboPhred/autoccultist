namespace Autoccultist.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.CS.TabletopUI;
    using Assets.TabletopUi.Scripts.Infrastructure;

    /// <summary>
    /// Implements <see cref="IMansusState"/>.
    /// </summary>
    internal class MansusStateImpl : IMansusState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MansusStateImpl"/> class.
        /// </summary>
        /// <param name="mapController">The map controller to load state from.</param>
        public MansusStateImpl(MapController mapController)
        {
            var tokenContainer = mapController ? Reflection.GetPrivateField<MapTokenContainer>(mapController, "_mapTokenContainer") : null;
            if (mapController == null || tokenContainer == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            var activeDoor = Reflection.GetPrivateField<DoorSlot>(tokenContainer, "activeSlot");
            if (activeDoor == null)
            {
                this.IsActive = false;
                this.DeckCards = new Dictionary<string, ICardState>();
                this.FaceUpDeck = null;
                this.FaceUpCard = null;
                return;
            }

            this.IsActive = true;

            var cards = Reflection.GetPrivateField<ElementStackToken[]>(mapController, "cards");

            if (cards[0] != null)
            {
                this.FaceUpCard = CardStateImpl.CardStatesFromStack(cards[0], CardLocation.Mansus).First();
            }

            var deckCards = new Dictionary<string, ICardState>
            {
                { activeDoor.GetDeckName(0), this.FaceUpCard },
            };

            if (cards[1] != null)
            {
                deckCards.Add(activeDoor.GetDeckName(1), CardStateImpl.CardStatesFromStack(cards[1], CardLocation.Mansus).First());
            }

            if (cards[2] != null)
            {
                deckCards.Add(activeDoor.GetDeckName(2), CardStateImpl.CardStatesFromStack(cards[2], CardLocation.Mansus).First());
            }

            this.DeckCards = deckCards;
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
