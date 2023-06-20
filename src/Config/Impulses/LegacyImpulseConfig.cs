namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An Impulse represents an action that cannot ever be truly satisfied.
    /// As long as the requirements of the Impulse allow for its execution, the task should execute.
    /// </summary>
    public class LegacyImpulseConfig : NamedConfigObject, IImpulseConfig
    {
        /// <summary>
        /// Gets or sets the impulse that this impulse inherits from.
        /// </summary>
        public LegacyImpulseConfig Extends { get; set; }

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
        TaskPriority IImpulse.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.Operation == null && this.Extends?.Operation == null)
            {
                throw new InvalidConfigException($"Impulse {this.Name} must have an operation.");
            }
        }

        public ConditionResult IsConditionMet(IGameState state)
        {
            var requirements = this.Requirements ?? this.Extends?.Requirements;
            var forbidders = this.Forbidders ?? this.Extends?.Forbidders;

            if (requirements != null)
            {
                var reqsMet = requirements.IsConditionMet(state);
                if (!reqsMet)
                {
                    return new AddendedConditionFailure(reqsMet, "Impulse requirements not met.");
                }
            }

            if (forbidders != null)
            {
                var forbidsMet = forbidders.IsConditionMet(state);
                if (forbidsMet)
                {
                    return new AddendedConditionFailure(new GameStateConditionFailure(forbidders, forbidsMet), "Impulse forbidders are present.");
                }
            }

            var operationState = (this.Operation ?? this.Extends.Operation).IsConditionMet(state);
            if (!operationState)
            {
                return new AddendedConditionFailure(operationState, "Operation condition not met.");
            }

            return ConditionResult.Success;
        }

        public IReaction Execute()
        {
            return (this.Operation ?? this.Extends.Operation).Execute();
        }
    }
}