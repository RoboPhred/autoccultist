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
    /// An imperative that can wrap
    /// As long as the requirements of the Impulse allow for its execution, the task should execute.
    /// </summary>
    public class NestedImperativeConfig : NamedConfigObject, IImperativeConfig
    {
        /// <summary>
        /// Gets or sets the impulse that this impulse inherits from.
        /// </summary>
        public NestedImperativeConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the priority for this impulse.
        /// Impulses from higher priority imperatives will run before lower priority imperatives.
        /// </summary>
        /// <remarks>
        /// This is here for legacy reasons.  NestedImperativeConfig is our OG impulse, back when impulses
        /// did a single thing and were conditional.
        /// As such, priority originally existed only for this class.
        /// </remarks>
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
        /// <remarks>
        /// This is here for legacy reasons.
        /// </remarks>
        public OperationImperativeImpulseConfig Operation { get; set; }

        /// <summary>
        /// Gets or sets a list of reactions to perform when this impulse is triggered.
        /// </summary>
        /// <remarks>
        /// This is here for legacy reasons.
        /// </remarks>
        public List<IReactorConfig> Reactions { get; set; } = new();

        IReadOnlyCollection<IImperative> IImperative.Children => this.Operation != null ? new IImperative[] { this.Operation } : new IImperative[0];

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
        public ConditionResult CanActivate(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.CanActivate), state, () =>
            {
                var requirements = this.Requirements ?? this.Extends?.Requirements;
                var forbidders = this.Forbidders ?? this.Extends?.Forbidders;

                if (requirements != null)
                {
                    var reqsMet = requirements.IsConditionMet(state);
                    if (!reqsMet)
                    {
                        return AddendedConditionResult.Addend(reqsMet, "Imperative requirements not met.");
                    }
                }

                if (forbidders != null)
                {
                    var forbidsMet = forbidders.IsConditionMet(state);
                    if (forbidsMet)
                    {
                        return AddendedConditionResult.Addend(GameStateConditionResult.ForFailure(forbidders, forbidsMet), "Imperative forbidders are present.");
                    }
                }

                var operation = this.Operation ?? this.Extends?.Operation;
                if (operation != null)
                {
                    var operationState = operation.CanActivate(state);
                    if (!operationState)
                    {
                        return AddendedConditionResult.Addend(operationState, "Operation imperative cannot activate.");
                    }
                }

                return ConditionResult.Success;
            });
        }

        /// <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
        {
            var canActivate = this.CanActivate(state);
            if (!canActivate)
            {
                // Nothing to do if we can't activate.
                return ConditionResult.Success;
            }

            return AddendedConditionResult.Addend(ConditionResult.Failure, "Imperative is available for execution.");
        }

        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            var operation = this.Operation ?? this.Extends?.Operation;
            return operation.DescribeCurrentGoals(state);
        }

        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (!this.CanActivate(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            var operation = this.Operation ?? this.Extends?.Operation;
            var impulses = operation.GetImpulses(state);

            // if we have a priority, override our operation priority.
            if (this.Priority.HasValue)
            {
                impulses = impulses.Select(x => new ReactionImpulse(this.Priority.Value, x.GetReaction()));
            }

            if (this.Reactions != null && this.Reactions.Count > 0)
            {
                impulses = impulses.Concat(this.Reactions.Select(x => new ReactionImpulse(this.Priority ?? TaskPriority.Normal, x.GetReaction())));
            }

            return impulses;
        }
    }
}
