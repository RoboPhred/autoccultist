namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Resources;
    using SecretHistories.Enums;

    public class DumpCompletedSituationImpulse : IImperative, IImpulse
    {
        public static readonly DumpCompletedSituationImpulse Instance = new();

        public TaskPriority Priority => TaskPriority.Critical;

        public string Name => "Dump Completed Situation";

        public IReadOnlyCollection<IImperative> Children => new IImperative[0];

        public ConditionResult CanActivate(IGameState state)
        {
            return this.GetCompletedSituations(state).Any() ? ConditionResult.Success : ConditionResult.Failure;
        }

        public ConditionResult IsSatisfied(IGameState state)
        {
            return this.GetCompletedSituations(state).Any() ? ConditionResult.Failure : ConditionResult.Success;
        }

        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (this.GetCompletedSituations(state).Any())
            {
                yield return this;
            }
        }

        public IReaction GetReaction()
        {
            // FIXME: Pass IGameState to GetReaction
            var toDump = this.GetCompletedSituations(GameStateProvider.Current).FirstOrDefault();
            if (toDump == null)
            {
                throw new ReactionFailedException("No completed situations to dump.");
            }

            return new DumpSituationReaction(toDump.SituationId);
        }

        private IEnumerable<ISituationState> GetCompletedSituations(IGameState state)
        {
            var situationResources = Resource.Of<ISituationState>();
            return
                from situation in state.Situations
                where situation.State == StateEnum.Complete
                where situationResources.IsAvailable(situation)
                select situation;
        }
    }
}
