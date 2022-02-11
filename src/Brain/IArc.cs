namespace Autoccultist.Brain
{
    using System.Collections.Generic;

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
    }
}
