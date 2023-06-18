namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Defines configuration for a motivation.
    /// </summary>
    public class MotivationConfig : NamedConfigObject, IMotivation
    {
        /// <summary>
        /// Gets or sets the primary goals of this motivation.
        /// </summary>
        public List<LibraryConfigObject<GoalConfig>> PrimaryGoals { get; set; } = new();

        /// <summary>
        /// Gets or sets the secondary goals of this motivation.
        /// </summary>
        public List<LibraryConfigObject<GoalConfig>> SupportingGoals { get; set; } = new();

        /// <inheritdoc/>
        IReadOnlyList<IGoal> IMotivation.PrimaryGoals => this.PrimaryGoals.ConvertAll(g => g.Value);

        /// <inheritdoc/>
        IReadOnlyList<IGoal> IMotivation.SupportingGoals => this.SupportingGoals.ConvertAll(g => g.Value);

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.PrimaryGoals == null || this.PrimaryGoals.Count == 0)
            {
                throw new YamlException(start, end, "Motivation must have at least one primary goal.");
            }

            if (this.SupportingGoals == null)
            {
                // Can happen if the config includes supportingGoals but doesn't put anything in it.
                this.SupportingGoals = new();
            }
        }
    }
}
