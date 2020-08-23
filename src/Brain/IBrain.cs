namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// /// Defines the driving force behind an automated playthrough of Cultist Simulator.
    /// </summary>
    public interface IBrain
    {
        /// <summary>
        /// Gets the list of goals for this playthrough.
        /// </summary>
        /// <remarks>
        /// Goals will be individually executed in order.
        /// </value>
        IReadOnlyList<IGoal> Goals { get; }
    }
}
