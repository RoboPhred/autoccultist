namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the driving force behind an automated playthrough of Cultist Simulator.
    /// </summary>
    public interface IBrain
    {
        /// <summary>
        /// Gets the list of motivations for this playthrough.
        /// </summary>
        IReadOnlyList<IMotivation> Motivations { get; }
    }
}
