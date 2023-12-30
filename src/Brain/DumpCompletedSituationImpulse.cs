namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    public class DumpCompletedSituationImpulse : IImperative, IImpulse
    {
        public static readonly DumpCompletedSituationImpulse Instance = new();

        // <inheritdoc/>
        public TaskPriority Priority => TaskPriority.Critical;

        // <inheritdoc/>
        public string Id => "DumpCompletedSituation";

        // <inheritdoc/>
        public string Name => "Dump Completed Situation";

        // <inheritdoc/>
        public IReadOnlyCollection<IImperative> Children => new IImperative[0];

        // <inheritdoc/>
        public ConditionResult IsConditionMet(IGameState state)
        {
            return this.GetCompletedSituations(state).Any() ? ConditionResult.Success : ConditionResult.Failure;
        }

        // <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
        {
            return AddendedConditionResult.Addend(ConditionResult.Failure, "Dumping completed situations is never satisfied.");
        }

        // <inheritdoc/>
        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return Enumerable.Empty<string>();
        }

        // <inheritdoc/>
        public IEnumerable<IImperative> GetActiveChildren(IGameState state)
        {
            return Enumerable.Empty<IImperative>();
        }

        // <inheritdoc/>
        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (this.GetCompletedSituations(state).Any())
            {
                yield return this;
            }
        }

        // <inheritdoc/>
        public IReaction GetReaction()
        {
            var toDump = this.GetCompletedSituations(GameStateProvider.Current).FirstOrDefault();
            if (toDump == null)
            {
                throw new ReactionFailedException("No completed situations to dump.");
            }

            return new DumpSituationReaction(toDump.SituationId);
        }

        private IEnumerable<ISituationState> GetCompletedSituations(IGameState state)
        {
            if (state.Mansus.State != PortalActiveState.Closed)
            {
                return Enumerable.Empty<ISituationState>();
            }

            return
                from situation in state.Situations
                where situation.State == StateEnum.Complete
                where situation.IsAvailable()
                select situation;
        }
    }
}
