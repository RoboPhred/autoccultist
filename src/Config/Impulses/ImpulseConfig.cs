namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An Impulse represents an action that cannot ever be truly satisfied.
    /// As long as the requirements of the Impulse allow for its execution, the task should execute.
    /// </summary>
    public class ImpulseConfig : NamedConfigObject, IImpulseConfig
    {
        /// <summary>
        /// Gets or sets the impulse that this impulse inherits from.
        /// </summary>
        public ImpulseConfig Extends { get; set; }

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
        public OperationImpulseConfig Operation { get; set; }

        /// <summary>
        /// Gets or sets a list of reactions to perform when the conditions are met
        /// </summary>
        public List<IReactorConfig> Reactions { get; set; } = new();

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
        }

        /// <inheritdoc/>
        public ConditionResult IsConditionMet(IGameState state)
        {
            var requirements = this.Requirements ?? this.Extends?.Requirements;
            var forbidders = this.Forbidders ?? this.Extends?.Forbidders;

            if (requirements != null)
            {
                var reqsMet = requirements.IsConditionMet(state);
                if (!reqsMet)
                {
                    return AddendedConditionResult.Addend(reqsMet, "Impulse requirements not met.");
                }
            }

            if (forbidders != null)
            {
                var forbidsMet = forbidders.IsConditionMet(state);
                if (forbidsMet)
                {
                    return AddendedConditionResult.Addend(GameStateConditionResult.ForFailure(forbidders, forbidsMet), "Impulse forbidders are present.");
                }
            }

            var operation = this.Operation ?? this.Extends?.Operation;
            if (operation != null)
            {
                var operationState = operation.IsConditionMet(state);
                if (!operationState)
                {
                    return AddendedConditionResult.Addend(operationState, "Operation condition not met.");
                }
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public IReaction GetReaction()
        {
            var reactions = new List<IReaction>();

            var operation = this.Operation ?? this.Extends?.Operation;
            if (operation != null)
            {
                reactions.Add(operation.GetReaction());
            }

            if (this.Reactions != null)
            {
                reactions.AddRange(this.Reactions.Select(x => x.GetReaction()));
            }

            return new CompoundReaction(reactions);
        }
    }
}
