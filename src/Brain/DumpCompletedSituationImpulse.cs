namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Resources;
    using SecretHistories.Enums;

    public class DumpCompletedSituationImpulse : IImpulse
    {
        public static readonly DumpCompletedSituationImpulse Instance = new();

        public TaskPriority Priority => TaskPriority.Critical;

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

        public ConditionResult IsConditionMet(IGameState state)
        {
            return this.GetCompletedSituations(state).Any() ? ConditionResult.Success : ConditionResult.Failure;
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
