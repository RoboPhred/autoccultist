namespace Autoccultist.GameState.Impl
{
    using System.Collections.Generic;

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
    }
}
