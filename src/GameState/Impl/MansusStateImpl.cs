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
            var activeDoor = Reflection.GetPrivateField<DoorSlot>(mapController, "activeSlot");
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

            this.FaceUpCard = CardStateImpl.CardStatesFromStack(cards[0], CardLocation.Mansus).First();

            this.DeckCards = new Dictionary<string, ICardState>
            {
                { activeDoor.GetDeckName(0), this.FaceUpCard },
                { activeDoor.GetDeckName(1), CardStateImpl.CardStatesFromStack(cards[1], CardLocation.Mansus).First() },
                { activeDoor.GetDeckName(2), CardStateImpl.CardStatesFromStack(cards[2], CardLocation.Mansus).First() },
            };
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
