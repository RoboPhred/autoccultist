namespace Autoccultist.Config
{
    using Autoccultist.Brain;
    using Autoccultist.Config.Conditions;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An Impulse represents an action that cannot ever be truly satisfied.
    /// As long as the requirements of the Impulse allow for its execution, the task should execute.
    /// </summary>
    public class ImpulseConfig : IConfigObject, IImpulse, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets the impulse that this impulse inherits from.
        /// </summary>
        public ImpulseConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the human-friendly display name for this impulse.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority for this impulse.
        /// Impulses with a higher priority will run before lower priority impulses.
        /// </summary>
        public TaskPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets a condition which must be met before this impulse can activate.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets a condition on which to prevent this impulse from activating.
        /// </summary>
        public IGameStateConditionConfig Forbidders { get; set; }

        /// <summary>
        /// Gets or sets the operation to perform when this impulse is triggered.
        /// </summary>
        public OperationConfig Operation { get; set; }

        /// <inheritdoc/>
        string IImpulse.Name => this.Name ?? this.Extends?.Name;

        /// <inheritdoc/>
        TaskPriority IImpulse.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        /// <inheritdoc/>
        IGameStateCondition IImpulse.Requirements => this.Requirements ?? this.Extends?.Requirements;

        /// <inheritdoc/>
        IGameStateCondition IImpulse.Forbidders => this.Forbidders ?? this.Extends?.Forbidders;

        /// <inheritdoc/>
        IOperation IImpulse.Operation => this.Operation ?? this.Extends?.Operation;

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.Operation == null && this.Extends?.Operation == null)
            {
                throw new InvalidConfigException($"Impulse {this.Name} must have an operation.");
            }
        }
    }
}
