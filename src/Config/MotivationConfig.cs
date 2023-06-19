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
    public class MotivationConfig : NamedConfigObject, IImperativeConfig, IImperative
    {
        // FIXME: LibraryConfigObject not working with imports here...  !import probably takes priority then fails to import to LibraryConfigObject

        /// <summary>
        /// Gets or sets the primary goals of this motivation.
        /// </summary>
        public FlatList<IImperativeConfig> PrimaryGoals { get; set; } = new();

        /// <summary>
        /// Gets or sets the secondary goals of this motivation.
        /// </summary>
        public FlatList<IImperativeConfig> SupportingGoals { get; set; } = new();

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

        public ConditionResult CanActivate(IGameState state)
        {
            return ConditionResult.Success;
        }

        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            foreach (var goal in this.PrimaryGoals.Where(g => !g.IsSatisfied(state)).SelectMany(x => x.DescribeCurrentGoals(state)))
            {
                yield return $"[Primary]: {goal}";
            }

            foreach (var goal in this.SupportingGoals.Where(g => !g.IsSatisfied(state)).SelectMany(x => x.DescribeCurrentGoals(state)))
            {
                yield return $"[Supporting]: {goal}";
            }
        }

        public IEnumerable<IReaction> GetReactions(IGameState state)
        {
            foreach (var goal in this.PrimaryGoals.Where(g => !g.IsSatisfied(state)).SelectMany(x => x.GetReactions(state)))
            {
                yield return goal;
            }

            foreach (var goal in this.SupportingGoals.Where(g => !g.IsSatisfied(state)).SelectMany(x => x.GetReactions(state)))
            {
                yield return goal;
            }
        }

        public ConditionResult IsSatisfied(IGameState state)
        {
            foreach (var goal in this.PrimaryGoals)
            {
                var result = goal.IsSatisfied(state);
                if (!result)
                {
                    return new AddendedConditionFailure(result, "Primary goal unsatisfied");
                }
            }

            return ConditionResult.Success;
        }
    }
}
