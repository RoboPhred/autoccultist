namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;

    public abstract class ImperativeConfigBase : NamedConfigObject, IImperativeConfig
    {
        public abstract IReadOnlyCollection<IImperative> Children { get; }

        /// <summary>
        /// Gets or sets a condition which must be met before this impulse can activate.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets a condition on which to prevent this impulse from activating.
        /// </summary>
        public IGameStateConditionConfig Forbidders { get; set; }

        /// <inheritdoc/>
        public virtual IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return this.Children.SelectMany(c => c.DescribeCurrentGoals(state));
        }

        /// <inheritdoc/>
        public virtual ConditionResult IsConditionMet(IGameState state)
        {
            return CacheUtils.Compute(this, $"{nameof(ImperativeConfigBase)}.{nameof(this.IsConditionMet)}", state, () =>
            {
                if (this.IsSatisfied(state))
                {
                    return AddendedConditionResult.Addend(ConditionResult.Failure, "Imperative is already satisfied.");
                }

                var requirements = this.GetRequirements();
                var forbidders = this.GetForbidders();

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
                        return AddendedConditionResult.Addend(forbidsMet, "Imperative forbidders met.");
                    }
                }

                return ConditionResult.Success;
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
