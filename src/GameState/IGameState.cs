namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public interface IGameState
    {
        // TODO: IReadOnlyCollection
        /// <summary>
        /// Gets a collection of cards on the tabletop.
        /// </summary>
        ICollection<ICardState> TabletopCards { get; }

        // TODO: IReadOnlyCollection
        /// <summary>
        /// Gets a collection of cards that are currently en route to somewhere.
        /// </summary>
        ICollection<ICardState> EnRouteCards { get; }

        // TODO: IReadOnlyCollection
        /// <summary>
        /// Gets a collection of situations currently present in the game.
        /// </summary>
        ICollection<ISituationState> Situations { get; }

        /// <summary>
        /// Gets the mansus state.
        /// </summary>
        IMansusState Mansus { get; }
    }
}
