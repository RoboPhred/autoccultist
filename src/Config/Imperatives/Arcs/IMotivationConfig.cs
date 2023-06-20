namespace AutoccultistNS.Config
{
    using System.Collections.Generic;

    public interface IMotivationConfig : INamedConfigObject, IImperativeConfig
    {
        /// <summary>
        /// Get the primary goals for this motivation.
        /// All goals must be completed for this motivation to be completed.
        /// </summary>
        IReadOnlyList<IImperativeConfig> PrimaryGoals { get; }

        /// <summary>
        /// Get the supporting goals for this motivation.
        /// These goals will be run in parallel with the primary goals.
        /// These goals do not need to be completed for this motivation to be completed.
        /// </summary>
        IReadOnlyList<IImperativeConfig> SupportingGoals { get; }
    }
}
