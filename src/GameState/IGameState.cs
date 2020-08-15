namespace Autoccultist.GameState
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
        ICollection<ICardState> TabletopCards { get; }

        /// <summary>
        /// Gets a collection of situations currently present in the game.
        /// </summary>
        ICollection<ISituationState> Situations { get; }
    }
}
