namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides the base implementation of a game state.
    /// </summary>
    internal class GameStateImpl : GameStateObject, IGameState
    {
        private readonly IReadOnlyCollection<ICardState> tabletopCards;
        private readonly IReadOnlyCollection<ICardState> enRouteCards;
        private readonly IReadOnlyCollection<ICardState> codexCards;
        private readonly IReadOnlyCollection<ISituationState> situations;

        private readonly IPortalState mansus;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateImpl"/> class.
        /// </summary>
        /// <param name="tabletopCards">The tabletop cards in this state.</param>
        /// <param name="enRouteCards">The en route cards in this state.</param>
        /// <param name="codexCards">The codex cards in this state.</param>
        /// <param name="situations">The situations in this state.</param>
        /// <param name="mansus">The mansus state.</param>
        public GameStateImpl(IReadOnlyCollection<ICardState> tabletopCards, IReadOnlyCollection<ICardState> enRouteCards, IReadOnlyCollection<ICardState> codexCards, IReadOnlyCollection<ISituationState> situations, IPortalState mansus)
        {
            this.tabletopCards = tabletopCards;
            this.enRouteCards = enRouteCards;
            this.situations = situations;
            this.codexCards = codexCards;
            this.mansus = mansus;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> TabletopCards
        {
            get
            {
                this.VerifyAccess();
                return this.tabletopCards;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> EnRouteCards
        {
            get
            {
                this.VerifyAccess();
                return this.enRouteCards;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> CodexCards
        {
            get
            {
                this.VerifyAccess();
                return this.codexCards;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ISituationState> Situations
        {
            get
            {
                this.VerifyAccess();
                return this.situations;
            }
        }

        /// <inheritdoc/>
        public IPortalState Mansus
        {
            get
            {
                this.VerifyAccess();
                return this.mansus;
            }
        }
    }
}
