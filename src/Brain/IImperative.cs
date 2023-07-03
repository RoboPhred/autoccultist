namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    // TODO: Should this just implement IGameStateCondition and forgo CanActivate/IsSatisfied?
    /// <summary>
    /// An imperative is a set of impulses for the bot to monitor and execute.
    /// Imperatives both have conditions at which they start, and conditions at which they are satisfied.
    /// When active, imperatives can generate a list of impulses they wish to be handled by the bot.
    /// Impulses may not execute immediately, but will be considered by the <see cref="NucleusAccumbens"/> for execution.
    /// </summary>
    public interface IImperative
    {
        /// <summary>
        /// Gets the name of this imperative.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the children of this imperative.
        /// </summary>
        IReadOnlyCollection<IImperative> Children { get; }

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
        /// <param name="state">The game state to check conditions against.</param>
        /// <remarks>
        /// This is for user consumption in the UI, to inform the user what the bot is currently working on.
        /// </remarks>
        IEnumerable<string> DescribeCurrentGoals(IGameState state);

        /// <summary>
        /// Gets a list of impulses for the current game state.
        /// </summary>
        IEnumerable<IImpulse> GetImpulses(IGameState state);
    }
}
