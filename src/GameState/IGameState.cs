namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Gets a collection of cards on the tabletop.
        /// </summary>
        IReadOnlyCollection<ICardState> TabletopCards { get; }

        /// <summary>
        /// Gets a collection of cards that are currently en route to somewhere.
        /// </summary>
        IReadOnlyCollection<ICardState> EnRouteCards { get; }

        /// <summary>
        /// Get a collection of cards in the codex mod, if installed.
        /// </summary>
        IReadOnlyCollection<ICardState> CodexCards { get; }

        /// <summary>
        /// Gets a collection of situations currently present in the game.
        /// </summary>
        IReadOnlyCollection<ISituationState> Situations { get; }

        /// <summary>
        /// Gets the bot's memories.
        /// </summary>
        IReadOnlyDictionary<string, int> Memories { get; }

        /// <summary>
        /// Gets the mansus state.
        /// </summary>
        IPortalState Mansus { get; }
    }
}
