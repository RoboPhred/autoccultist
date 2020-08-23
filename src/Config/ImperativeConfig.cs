namespace Autoccultist.Config
{
    using Autoccultist.Brain;
    using Autoccultist.Config.Conditions;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An Imperative represents an action that cannot ever be truly satisfied.
    /// As long as the requirements of the Imperative allow for its execution, the task should execute.
    /// </summary>
    public class ImperativeConfig : IConfigObject, IImperative, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets the imperative that this imperative inherits from.
        /// </summary>
        public ImperativeConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the human-friendly display name for this imperative.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority for this imperative.
        /// Imperatives with a higher priority will run before lower priority imperatives.
        /// </summary>
        public TaskPriority? Priority { get; set; } = TaskPriority.Normal;

        /// <summary>
        /// Gets or sets a condition which must be met before this imperative can activate.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets a condition on which to prevent this imperative from activating.
        /// </summary>
        public IGameStateConditionConfig Forbidders { get; set; }

        /// <summary>
        /// Gets or sets the operation to perform when this imperative is triggered.
        /// </summary>
        public OperationConfig Operation { get; set; }

        /// <inheritdoc/>
        string IImperative.Name => this.Name ?? this.Extends?.Name;

        /// <inheritdoc/>
        TaskPriority IImperative.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        /// <inheritdoc/>
        IGameStateCondition IImperative.Requirements => this.Requirements ?? this.Extends?.Requirements;

        /// <inheritdoc/>
        IGameStateCondition IImperative.Forbidders => this.Forbidders ?? this.Extends?.Forbidders;

        /// <inheritdoc/>
        IOperation IImperative.Operation => this.Operation ?? this.Extends?.Operation;

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.Operation == null && this.Extends?.Operation == null)
            {
                throw new InvalidConfigException($"Imperative {this.Name} must have an operation.");
            }
        }
    }
}
