namespace AutoccultistNS.Brain
{
    using SecretHistories.Infrastructure.Persistence;

    /// <summary>
    /// Provides a path for the AI to follow in the form of a list of motivations to complete.
    /// </summary>
    public interface IArc : IImperative
    {
        /// <summary>
        /// Gets the selection hint to be used to determine the current arc on loading a save.
        /// </summary>
        IGameStateCondition SelectionHint { get; }

        /// <summary>
        /// Gets a value indicating whether a new game can be started from this arc.
        /// </summary>
        bool SupportsNewGame { get; }

        /// <summary>
        /// Gets a game persistence provider to start a new instance of this arc, if supported.
        /// </summary>
        /// <returns>A <see cref="IGamePersistenceProvider"/> instance to start the new game, or `null` if not supported.</returns>
        GamePersistenceProvider GetNewGameProvider();
    }
}
