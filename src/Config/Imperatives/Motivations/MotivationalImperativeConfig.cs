namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    public class MotivationalImperativeConfig : ImperativeConfigBase
    {
        /// <summary>
        /// Gets or sets a list of motivations that will drive the execution of this arc.
        /// </summary>
        public MotivationCollectionConfig Motivations { get; set; }

        /// <inheritdoc/>
        public override IReadOnlyCollection<IImperative> Children => this.Motivations.Children;

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.Motivations == null || this.Motivations.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one motivation.");
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

            return this.Motivations.IsConditionMet(state);
        }

        /// <inheritdoc/>
        public override ConditionResult IsSatisfied(IGameState state)
        {
            return this.Motivations.IsSatisfied(state);
        }

        public override IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return this.Motivations.DescribeCurrentGoals(state);
        }

        public override IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            // This uses the base implementation which checks IsSatisfied.
            if (!this.IsConditionMet(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            return this.Motivations.GetImpulses(state);
        }
    }
}
