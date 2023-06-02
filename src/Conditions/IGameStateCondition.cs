namespace AutoccultistNS
{
    using AutoccultistNS.GameState;

    /// <summary>
    /// Describes a condition matchable against a <see cref="Autoccultist.IGameState"/>.
    /// </summary>
    public interface IGameStateCondition
    {
        /// <summary>
        /// Determines if the condition is met by the given game state.
        /// </summary>
        /// <param name="state">The game state to check the condition against.</param>
        /// <param name="failureDescription">A description of why the condition failed, or null if it did not.</param>
        /// <returns>A value indicating if the condition is met by the game state.</returns>
        bool IsConditionMet(IGameState state, out ConditionFailure failureDescription);
    }
}
