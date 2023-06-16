namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Defines configuration for a motivation.
    /// </summary>
    public class MotivationConfig : IMotivation, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets the motivation name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the primary goals of this motivation.
        /// </summary>
        public List<GoalConfig> PrimaryGoals { get; set; } = new();

        /// <summary>
        /// Gets or sets the secondary goals of this motivation.
        /// </summary>
        public List<GoalConfig> SupportingGoals { get; set; } = new();

        /// <inheritdoc/>
        IReadOnlyList<IGoal> IMotivation.PrimaryGoals => this.PrimaryGoals;

        /// <inheritdoc/>
        IReadOnlyList<IGoal> IMotivation.SupportingGoals => this.SupportingGoals;

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.PrimaryGoals == null || this.PrimaryGoals.Count == 0)
            {
                throw new YamlException(start, end, "Motivation must have at least one primary goal.");
            }

            if (this.SupportingGoals == null)
            {
                // Can happen if the config includes supportingGoals but doesnt put anything in it.
                this.SupportingGoals = new List<GoalConfig>();
            }
        }
    }
}
