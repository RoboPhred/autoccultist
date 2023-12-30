namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Defines configuration for a motivation.
    /// A motivation is a type of imperative that is complete when its primary imperatives are complete, but can simultaniously run supporting imperatives.
    /// <para>
    /// For legacy reasons, a motivation can always activate regardless of its goals.
    /// </summary>
    public class MotivationConfig : ImperativeConfigBase, IMotivationConfig
    {
        /// <summary>
        /// Gets or sets the primary goals of this motivation.
        /// </summary>
        public FlatList<ObjectOrLibraryEntry<GoalConfig>> PrimaryGoals { get; set; } = new();

        /// <summary>
        /// Gets or sets the secondary goals of this motivation.
        /// </summary>
        public FlatList<ObjectOrLibraryEntry<GoalConfig>> SupportingGoals { get; set; } = new();

        public override IReadOnlyCollection<IImperative> Children => this.PrimaryGoals.Concat(this.SupportingGoals).Select(x => x.Value).ToArray();

        IReadOnlyList<GoalConfig> IMotivationConfig.PrimaryGoals => this.PrimaryGoals.Select(x => x.Value).ToArray();

        IReadOnlyList<GoalConfig> IMotivationConfig.SupportingGoals => this.SupportingGoals.Select(x => x.Value).ToArray();

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.PrimaryGoals == null || this.PrimaryGoals.Count == 0)
            {
                throw new YamlException(start, end, "Motivation must have at least one primary goal.");
            }

            if (this.SupportingGoals == null)
            {
                // Can happen if the config includes supportingGoals but doesn't put anything in it.
                this.SupportingGoals = new();
            }
        }

        /// <inheritdoc/>
        public override ConditionResult IsSatisfied(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.IsSatisfied), state, () =>
            {
                foreach (var goal in this.PrimaryGoals.Select(x => x.Value))
                {
                    var result = goal.IsSatisfied(state);
                    if (!result)
                    {
                        return AddendedConditionResult.Addend(result, $"Primary goal {goal} unsatisfied.");
                    }
                }

                return ConditionResult.Success;
            });
        }

        /// <inheritdoc/>
        public override IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            yield return $"[Motivation]: {this.Name}";

            foreach (var goal in this.PrimaryGoals.Select(x => x.Value).Where(g => !g.IsSatisfied(state)).SelectMany(x => x.DescribeCurrentGoals(state)))
            {
                yield return $"[Primary]: {goal}";
            }

            foreach (var goal in this.SupportingGoals.Select(x => x.Value).Where(g => !g.IsSatisfied(state)).SelectMany(x => x.DescribeCurrentGoals(state)))
            {
                yield return $"[Supporting]: {goal}";
            }
        }

        public override IEnumerable<IImperative> GetActiveChildren(IGameState state)
        {
            return this.PrimaryGoals.Select(x => x.Value).Concat(this.SupportingGoals.Select(x => x.Value)).Where(x => !x.IsSatisfied(state));
        }

        /// <inheritdoc/>
        public override IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            // This uses the default implementation which checks IsSatisfied
            if (!this.IsConditionMet(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            var primaryImpulses =
                from goalEntry in this.PrimaryGoals
                let goal = goalEntry.Value
                where !goal.IsSatisfied(state)
                from impulse in goal.GetImpulses(state)
                select impulse;

            var supportingImpulses =
                from goalEntry in this.SupportingGoals
                let goal = goalEntry.Value
                where !goal.IsSatisfied(state)
                from impulse in goal.GetImpulses(state)
                select impulse;

            // We must make this an array, as the cache might make it be enumerated several times.
            return primaryImpulses.Concat(supportingImpulses);
        }
    }
}
