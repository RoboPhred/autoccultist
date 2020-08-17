namespace Autoccultist.Brain
{
    using System.Collections.Generic;
    using Autoccultist.GameState;

    /// <summary>
    /// A description of a task with a starting condition, a target game state, and a list of imperatives to complete it.
    /// </summary>
    public interface IGoal
    {
        /// <summary>
        /// Gets the human-friendly display name of this goal.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the condition which is required to be met for this goal to activate.
        /// <para>
        /// The goal will remain activated after these conditions are met,
        /// and continue operating until the CompletedWhen condition is met.
        /// </summary>
        IGameStateCondition Requirements { get; }

        /// <summary>
        /// Gets the condition to determine when this goal is completed.
        /// <para>
        /// Once started, the goal will continue to operate until its completion conditions are met.
        /// </summary>
        IGameStateCondition CompletedWhen { get; }

        /// <summary>
        /// Gets a list of imperatives this goal provides.
        /// <para>
        /// Each imperative provides an operation and conditions under which the operation will be performed.
        /// </summary>
        IReadOnlyList<IImperative> Imperatives { get; }

        /// <summary>
        /// Determines whether the goal can activate with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if this goal is able to activate, False otherwise.</returns>
        bool CanActivate(IGameState state);

        /// <summary>
        /// Determines whether this goal is completed with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if the goal is completed, False otherwise.</returns>
        bool IsSatisfied(IGameState state);
    }
}
