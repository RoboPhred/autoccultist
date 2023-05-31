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
        private readonly ICollection<ICardState> enRouteCards;
        private readonly ICollection<ISituationState> situations;

        private readonly IMansusState mansus;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateImpl"/> class.
        /// </summary>
        /// <param name="tabletopCards">The tabletop cards in this state.</param>
        /// <param name="enRouteCards">The en route cards in this state.</param>
        /// <param name="situations">The situations in this state.</param>
        /// <param name="mansus">The mansus state.</param>
        public GameStateImpl(ICollection<ICardState> tabletopCards, ICollection<ICardState> enRouteCards, ICollection<ISituationState> situations, IMansusState mansus)
        {
            this.tabletopCards = tabletopCards;
            this.enRouteCards = enRouteCards;
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
        public ICollection<ICardState> EnRouteCards
        {
            get
            {
                this.VerifyAccess();
                return this.enRouteCards;
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
