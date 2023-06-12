namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using SecretHistories.Infrastructure.Persistence;

    /// <summary>
    /// Provides a path for the AI to follow in the form of a list of motivations to complete.
    /// </summary>
    public interface IArc
    {
        /// <summary>
        /// Gets the name of the arc.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the selection hint to be used to determine the current arc on loading a save.
        /// </summary>
        IGameStateCondition SelectionHint { get; }

        /// <summary>
        /// Gets the list of motivations for this playthrough.
        /// </summary>
        IReadOnlyList<IMotivation> Motivations { get; }

        /// <summary>
        /// Gets a value indicating whether a new game can be started from this arc.
        /// </summary>
        bool SupportsNewGame { get; }

        /// <summary>
        /// Gets a game persistence provider to start a new instance of this arc.
        /// </summary>
        GamePersistenceProvider GetNewGameProvider();
    }
}
