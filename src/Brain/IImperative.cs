namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    /// <summary>
    /// An imperative is a set of reactions the bot wants to perform.
    /// Imperatives both have conditions at which they start, and conditions at which they are satisfied.
    /// When active, imperatives can generate a list of reactions they wish to be handled by the bot.
    /// Reactions may not execute immediately, but will be considered by the <see cref="NucleusAccumbens"/> for execution.
    /// </summary>
    public interface IImperative
    {
        /// <summary>
        /// Gets the name of this imperative.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether or not this imperative can begin based on the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>The condition result for determining if this imperative can activate.</returns>
        ConditionResult CanActivate(IGameState state);

        /// <summary>
        /// Gets whether or not this imperative is satisfied based on the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>The condition result for determining if this imperative is satisfied.</returns>
        ConditionResult IsSatisfied(IGameState state);

        /// <summary>
        /// Gets a list of goal descriptions for all goals this imperative is working on.
        /// </summary>
        /// <param name="gameState">The game state to check conditions against.</param>
        /// <remarks>
        /// This is for user consumption in the UI, to inform the user what the bot is currently working on.
        /// </remarks>
        IEnumerable<string> DescribeCurrentGoals(IGameState gameState);

        /// <summary>
        /// Gets the active reactions for this imperative.
        /// </summary>
        IEnumerable<IImpulse> GetReactions(IGameState state);
    }
}
