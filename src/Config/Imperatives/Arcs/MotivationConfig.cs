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
    public class MotivationConfig : NamedConfigObject, IMotivationConfig
    {
        /// <summary>
        /// Gets or sets the primary goals of this motivation.
        /// </summary>
        public FlatList<ObjectOrLibraryEntry<IImperativeConfig>> PrimaryGoals { get; set; } = new();

        /// <summary>
        /// Gets or sets the secondary goals of this motivation.
        /// </summary>
        public FlatList<ObjectOrLibraryEntry<IImperativeConfig>> SupportingGoals { get; set; } = new();

        IReadOnlyList<IImperativeConfig> IMotivationConfig.PrimaryGoals => this.PrimaryGoals.Select(x => x.Value).ToArray();

        IReadOnlyList<IImperativeConfig> IMotivationConfig.SupportingGoals => this.SupportingGoals.Select(x => x.Value).ToArray();

        string IImperative.Name => throw new System.NotImplementedException();

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
        public virtual ConditionResult CanActivate(IGameState state)
        {
            // For legacy reasons, we consider motivations to always be ready to start.
            // This is overriden in newer systems.
            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
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

        /// <inheritdoc/>
        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            return CacheUtils.Compute(this, state, state =>
            {
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
                var result = primaryImpulses.Concat(supportingImpulses).ToArray();

                NoonUtility.LogWarning($"MotivationConfig.GetImpulses cached {result.Length} impulses");

                return result;
            });
        }

        /// <inheritdoc/>
        public IEnumerable<IImperative> Flatten()
        {
            yield return this;

            var goals =
                from goal in this.PrimaryGoals.Select(x => x.Value).Concat(this.SupportingGoals.Select(x => x.Value))
                from flat in goal.Flatten()
                select flat;

            foreach (var goal in goals)
            {
                yield return goal;
            }
        }

        /// <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
        {
            return CacheUtils.Compute(this, state, state =>
            {
                foreach (var goal in this.PrimaryGoals)
                {
                    var result = goal.Value.IsSatisfied(state);
                    if (!result)
                    {
                        return AddendedConditionResult.Addend(result, "Primary goal unsatisfied");
                    }
                }

                return ConditionResult.Success;
            });
        }
    }
}
