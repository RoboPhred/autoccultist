namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    public abstract class ImperativeConfigBase : NamedConfigObject, IImperativeConfig
    {
        public abstract IReadOnlyCollection<IImperative> Children { get; }

        /// <inheritdoc/>
        [YamlMember(Alias = "ui")]
        public UISettingsConfig UI { get; set; } = null;

        /// <summary>
        /// Gets or sets a condition which must be met before this impulse can activate.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets a condition on which to prevent this impulse from activating.
        /// </summary>
        public IGameStateConditionConfig Forbidders { get; set; }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.UI == null)
            {
                var operations = this.GetChildrenDeep().OfType<OperationConfig>().ToArray();
                var situationCount = operations.Select(x => x.Situation).Distinct().Count();
                var copyFromOp = situationCount == 1 ? operations[0] : null;
                this.UI = new UISettingsConfig
                {
                    Visible = true,
                    HomeSituation = copyFromOp?.Situation,
                    Icon = copyFromOp != null ? $"verb:{copyFromOp.Situation}" : null,
                };
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return this.Children.SelectMany(c => c.DescribeCurrentGoals(state));
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IImperative> GetActiveChildren(IGameState state);

        /// <inheritdoc/>
        public virtual ConditionResult IsConditionMet(IGameState state)
        {
            return CacheUtils.Compute(this, $"{nameof(ImperativeConfigBase)}.{nameof(this.IsConditionMet)}", state, () =>
            {
                if (this.IsSatisfied(state))
                {
                    return AddendedConditionResult.Addend(ConditionResult.Failure, $"{this.GetType().Name} is already satisfied.");
                }

                var requirements = this.GetRequirements();
                var forbidders = this.GetForbidders();

                ConditionResult requirementsResult;
                if (requirements != null)
                {
                    var reqsMet = requirements.IsConditionMet(state);
                    if (!reqsMet)
                    {
                        return AddendedConditionResult.Addend(reqsMet, $"{this.GetType().Name} requirements not met.");
                    }
                    else
                    {
                        requirementsResult = AddendedConditionResult.Addend(reqsMet, $"{this.GetType().Name} requirements met");
                    }
                }
                else
                {
                    requirementsResult = AddendedConditionResult.Addend(ConditionResult.Success, $"{this.GetType().Name} has no requirements.");
                }

                ConditionResult forbiddersResult;
                if (forbidders != null)
                {
                    var forbidsMet = forbidders.IsConditionMet(state);
                    if (forbidsMet)
                    {
                        return AddendedConditionResult.Addend(GameStateConditionResult.ForFailure(forbidders, forbidsMet), $"{this.GetType().Name} forbidders met.");
                    }
                    else
                    {
                        forbiddersResult = AddendedConditionResult.Addend(forbidsMet, $"{this.GetType().Name} forbidders not met.");
                    }
                }
                else
                {
                    forbiddersResult = AddendedConditionResult.Addend(ConditionResult.Success, $"{this.GetType().Name} has no forbidders.");
                }

                return CompoundConditionResult.ForSuccess(new[] { requirementsResult, forbiddersResult });
            });
        }

        /// <inheritdoc/>
        public abstract ConditionResult IsSatisfied(IGameState state);

        /// <inheritdoc/>
        public virtual IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (!this.IsConditionMet(state) || this.IsSatisfied(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            return this.Children.SelectMany(c => c.GetImpulses(state));
        }

        protected virtual IGameStateCondition GetRequirements()
        {
            return this.Requirements;
        }

        protected virtual IGameStateCondition GetForbidders()
        {
            return this.Forbidders;
        }
    }
}
