namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An imperative that triggers impulses based on a set of conditions.
    /// </summary>
    public class LeafImperativeConfig : ImperativeConfigBase
    {
        /// <summary>
        /// Gets or sets the impulse that this impulse inherits from.
        /// </summary>
        public LeafImperativeConfig Extends { get; set; }

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
        /// Gets or sets the operation to perform when this impulse is triggered.
        /// This takes an OperationImperative, that must also be able to activate in order
        /// for this imperative to activate.
        /// </summary>
        public OperationConfig Operation { get; set; }

        /// <summary>
        /// Gets or sets a list of reactions to perform when this impulse is triggered.
        /// </summary>
        public List<IReactorConfig> Reactions { get; set; } = new();

        public override IReadOnlyCollection<IImperative> Children
        {
            get
            {
                var operation = this.Operation ?? this.Extends?.Operation;
                if (operation != null)
                {
                    return new IImperative[] { operation };
                }
                else
                {
                    return new IImperative[0];
                }
            }
        }

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
        public override ConditionResult IsConditionMet(IGameState state)
        {
            var baseCondition = base.IsConditionMet(state);
            if (!baseCondition)
            {
                return baseCondition;
            }

            return CacheUtils.Compute(this, nameof(this.IsConditionMet), state, () =>
            {
                var operation = this.Operation ?? this.Extends?.Operation;
                if (operation != null)
                {
                    var operationState = operation.IsConditionMet(state);
                    if (!operationState)
                    {
                        return AddendedConditionResult.Addend(operationState, "Operation imperative cannot activate.");
                    }
                }

                return ConditionResult.Success;
            });
        }

        public override ConditionResult IsSatisfied(IGameState state)
        {
            return AddendedConditionResult.Addend(ConditionResult.Failure, "Leaf imperatives cannot be satisfied.");
        }

        public override IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (!this.IsConditionMet(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            var operation = this.Operation ?? this.Extends?.Operation;
            var impulses = operation != null ? operation.GetImpulses(state) : Enumerable.Empty<IImpulse>();

            // if we have a priority, override our operation priority.
            if (this.Priority.HasValue)
            {
                impulses = impulses.Select(x => new ReactorImpulse(this.Priority.Value, x));
            }

            if (this.Reactions != null && this.Reactions.Count > 0)
            {
                impulses = impulses.Concat(this.Reactions.Select(x => new ReactorImpulse(this.Priority ?? TaskPriority.Normal, x)));
            }

            return impulses;
        }

        protected override IGameStateCondition GetRequirements()
        {
            return base.GetRequirements() ?? this.Extends?.GetRequirements();
        }

        protected override IGameStateCondition GetForbidders()
        {
            return base.GetForbidders() ?? this.Extends?.GetForbidders();
        }
    }
}
