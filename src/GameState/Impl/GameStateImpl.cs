namespace AutoccultistNS.GameState.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides the base implementation of a game state.
    /// </summary>
    internal class GameStateImpl : GameStateObject, IGameState
    {
        private static int prevHash = 0;

        private readonly Lazy<IReadOnlyCollection<ICardState>> allCards;
        private readonly IReadOnlyCollection<ICardState> tabletopCards;
        private readonly IReadOnlyCollection<ICardState> enRouteCards;
        private readonly IReadOnlyCollection<ICardState> codexCards;
        private readonly IReadOnlyCollection<ISituationState> situations;
        private readonly IReadOnlyDictionary<string, int> memories;
        private readonly IPortalState mansus;
        private readonly Lazy<int> hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateImpl"/> class.
        /// </summary>
        /// <param name="tabletopCards">The tabletop cards in this state.</param>
        /// <param name="enRouteCards">The en route cards in this state.</param>
        /// <param name="codexCards">The codex cards in this state.</param>
        /// <param name="situations">The situations in this state.</param>
        /// <param name="mansus">The mansus state.</param>
        public GameStateImpl(
            IEnumerable<ICardState> tabletopCards,
            IEnumerable<ICardState> enRouteCards,
            IEnumerable<ICardState> codexCards,
            IEnumerable<ISituationState> situations,
            IReadOnlyDictionary<string, int> memories,
            IPortalState mansus)
        {
            this.tabletopCards = new HashCalculatingCollection<ICardState>(tabletopCards);
            this.enRouteCards = new HashCalculatingCollection<ICardState>(enRouteCards);
            this.codexCards = new HashCalculatingCollection<ICardState>(codexCards);
            this.situations = new HashCalculatingCollection<ISituationState>(situations);

            this.allCards = new Lazy<IReadOnlyCollection<ICardState>>(() =>
            {
                var allCards = this.TabletopCards.Concat(this.EnRouteCards).Concat(this.Situations.SelectMany(s => s.StoredCards.Concat(s.GetSlottedCards()).Concat(s.OutputCards))).Concat(this.CodexCards);
                switch (this.Mansus.State)
                {
                    case PortalActiveState.AwaitingCollection:
                        allCards = allCards.Concat(new[] { this.Mansus.OutputCard });
                        break;
                    case PortalActiveState.AwaitingSelection:
                        allCards = allCards.Concat(this.Mansus.DeckCards.Values);
                        break;
                }

                return new HashCalculatingCollection<ICardState>(allCards);
            });

            this.memories = memories;
            this.mansus = mansus;

            this.hashCode = new Lazy<int>(
                () => PerfMonitor.Monitor(
                    "GameStateImpl Hash",
                    () =>
                    {
                        var hash = HashUtils.Hash(
                            this.tabletopCards,
                            this.enRouteCards,
                            this.codexCards,
                            this.situations,
                            HashUtils.HashAllUnordered(this.memories.Select(x => $"{x.Key}={x.Value}")),
                            this.mansus);

                        if (hash != prevHash)
                        {
                            PerfMonitor.AddCount("GameStateImpl New Hash");
                            prevHash = hash;
                        }

                        return hash;
                    }));
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> AllCards
        {
            get
            {
                this.VerifyAccess();
                return this.allCards.Value;
            }
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
        public IReadOnlyDictionary<string, int> Memories
        {
            get
            {
                this.VerifyAccess();
                return this.memories;
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

        public override int GetHashCode()
        {
            return this.hashCode.Value;
        }
    }
}
