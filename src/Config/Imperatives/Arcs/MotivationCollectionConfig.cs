namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;

    [CustomDeserializer(typeof(MotivationCollectionConfigDeserializer))]
    public abstract class MotivationCollectionConfig : NamedConfigObject, IImperativeConfig, IImperative
    {
        /// <summary>
        /// Gets the total number of motivations in this config
        /// </summary>
        public abstract int Count { get; }

        /// <inheritdoc/>
        public abstract ConditionResult CanActivate(IGameState state);

        /// <inheritdoc/>
        public virtual IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            var motivations = this.GetCurrentMotivations(state).ToArray();
            return motivations.SelectMany(x => x.DescribeCurrentGoals(state));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            var reactions = from motivation in this.GetCurrentMotivations(state)
                            let primaryGoals = motivation.PrimaryGoals
                            let secondaryGoals = motivation.SupportingGoals
                            let goals = primaryGoals.Concat(secondaryGoals)
                            from goal in goals
                            where !goal.IsSatisfied(state)
                            from impulse in goal.GetImpulses(state)
                            select impulse;
            return reactions.Distinct().Cast<IImpulse>();
        }

        /// <inheritdoc/>
        public virtual ConditionResult IsSatisfied(IGameState state)
        {
            var primaryGoals =
                from motivation in this.GetCurrentMotivations(state)
                from goal in motivation.PrimaryGoals
                select goal;

            foreach (var goal in primaryGoals)
            {
                var result = goal.IsSatisfied(state);
                if (!result)
                {
                    return result;
                }
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IImperative> Flatten();

        /// <summary>
        /// Gets the current active motivations from the game state.
        /// </summary>
        protected abstract IEnumerable<IMotivationConfig> GetCurrentMotivations(IGameState state);
    }
}
