namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a package of primary and supporting goals.
    /// </summary>
    public interface IMotivation
    {
        /// <summary>
        /// Gets the motivation name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the primary goals of this motivation.
        /// </summary>
        IReadOnlyList<IGoal> PrimaryGoals { get; }

        /// <summary>
        /// Gets the supporting goals of this motivation.
        /// </summary>
        IReadOnlyList<IGoal> SupportingGoals { get; }
    }
}
