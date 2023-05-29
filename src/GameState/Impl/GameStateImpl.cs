namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides the base implementation of a game state.
    /// </summary>
    internal class GameStateImpl : GameStateObject, IGameState
    {
        private readonly ICollection<ICardState> tabletopCards;
        private readonly ICollection<ISituationState> situations;

        private readonly IMansusState mansus;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateImpl"/> class.
        /// </summary>
        /// <param name="tabletopCards">The tabletop cards in this state.</param>
        /// <param name="situations">The situations in this state.</param>
        /// <param name="mansus">The mansus state.</param>
        public GameStateImpl(ICollection<ICardState> tabletopCards, ICollection<ISituationState> situations, IMansusState mansus)
        {
            this.tabletopCards = tabletopCards;
            this.situations = situations;
            this.mansus = mansus;
        }

        /// <inheritdoc/>
        public ICollection<ICardState> TabletopCards
        {
            get
            {
                this.VerifyAccess();
                return this.tabletopCards;
            }
        }

        /// <inheritdoc/>
        public ICollection<ISituationState> Situations
        {
            get
            {
                this.VerifyAccess();
                return this.situations;
            }
        }

        /// <inheritdoc/>
        public IMansusState Mansus
        {
            get
            {
                this.VerifyAccess();
                return this.mansus;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ICardState> GetAllCards()
        {
            this.VerifyAccess();

            var allCards = this.tabletopCards.Concat(this.situations.SelectMany(s => s.StoredCards.Concat(s.GetSlottedCards()).Concat(s.OutputCards)));
            if (this.mansus.IsActive)
            {
                allCards = allCards.Concat(this.mansus.DeckCards.Values);
            }

            return allCards;
        }
    }
}
