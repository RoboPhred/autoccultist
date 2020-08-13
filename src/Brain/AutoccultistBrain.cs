using System.Linq;
using System.Collections.Generic;
using Autoccultist.Brain.Config;
using Autoccultist.GameState;

namespace Autoccultist.Brain
{
    public class AutoccultistBrain
    {
        private BrainConfig config;

        private Goal currentGoal;

        public AutoccultistBrain(BrainConfig config)
        {
            this.config = config;
        }

        public void Start(IGameState state)
        {
            if (this.IsGoalSatisfied(state))
            {
                this.currentGoal = null;
            }

            if (this.currentGoal == null)
            {
                this.ObtainNextGoal(state);
            }
        }

        public void Stop()
        {
        }

        public void Update(IGameState state)
        {
            if (this.IsGoalSatisfied(state))
            {
                this.currentGoal = null;
            }

            if (this.currentGoal == null)
            {
                this.ObtainNextGoal(state);
                if (this.currentGoal == null)
                {
                    return;
                }
            }

            // Scan through all possible imperatives and invoke the ones that can start.
            //  Where multiple imperatives try for the same verb, invoke the highest priority
            var candidateGroups =
                from imperative in this.GetSatisfiableImperatives(state)
                orderby imperative.Priority descending
                group imperative.Operation by imperative.Operation.Situation into situationGroup
                select situationGroup;

            foreach (var group in candidateGroups)
            {
                var operation = group.FirstOrDefault();
                if (operation == null)
                {
                    continue;
                }

                if (!SituationOrchestrator.SituationIsAvailable(operation.Situation))
                {
                    continue;
                }

                SituationOrchestrator.ExecuteOperation(operation, state);
            }
        }

        public void LogStatus(IGameState state)
        {
            AutoccultistPlugin.Instance.LogInfo(string.Format("My goal is {0}", this.currentGoal != null ? this.currentGoal.Name : "<none>"));
            AutoccultistPlugin.Instance.LogInfo(string.Format("I have {0} satisfiable imperatives", this.GetSatisfiableImperatives(state).Count));
            if (this.currentGoal != null)
            {
                foreach (var imperative in this.currentGoal.Imperatives.OrderByDescending(x => x.Priority))
                {
                    AutoccultistPlugin.Instance.LogInfo($"Imperative - {imperative.Name}");
                    AutoccultistPlugin.Instance.LogInfo($"-- Situation {imperative.Operation.Situation} available: {state.GetSituation(imperative.Operation.Situation)?.IsBusy}");
                    var stateScope = state.CreateConsumptionScope();
                    foreach (var choice in imperative.Operation.StartingRecipe.Slots)
                    {
                        AutoccultistPlugin.Instance.LogInfo($"-- Slot {choice.Key} satisfied: {choice.Value.TryConsume(stateScope, out var _)}");
                    }
                }
            }
            else
            {
                foreach (var goal in this.config.Goals)
                {
                    AutoccultistPlugin.Instance.LogInfo("Goal " + goal.Name);
                    if (goal.CompletedWhen.Requirements != null)
                    {
                        AutoccultistPlugin.Instance.LogInfo("-- Required cards (" + goal.RequiredCards.Mode.ToString() + "):");
                        var requireScope = state.CreateConsumptionScope();
                        foreach (CardChoice card in goal.RequiredCards.Requirements)
                        {
                            AutoccultistPlugin.Instance.LogInfo("-- -- " + card + " satisfied " + card.TryConsume(requireScope, out var _));
                        }

                        AutoccultistPlugin.Instance.LogInfo("-- Completion cards (" + goal.CompletedWhen.Mode.ToString() + "):");
                        var completedScope = state.CreateConsumptionScope();
                        foreach (CardChoice card in goal.CompletedWhen.Requirements)
                        {
                            AutoccultistPlugin.Instance.LogInfo("-- -- " + card + " satisfied " + card.TryConsume(completedScope, out var _));
                        }
                    }
                }
            }
        }

        private IList<Imperative> GetSatisfiableImperatives(IGameState state)
        {
            if (this.currentGoal == null)
            {
                return new Imperative[0];
            }

            var imperatives =
                from imperative in this.currentGoal.Imperatives
                where imperative.CanExecute(state)
                select imperative;
            return imperatives.ToList();
        }

        private bool IsGoalSatisfied(IGameState state)
        {
            if (this.currentGoal == null)
            {
                return true;
            }
            return this.currentGoal.IsSatisfied(state);
        }

        private void ObtainNextGoal(IGameState state)
        {
            var goals =
                from goal in this.config.Goals
                where goal.CanActivate(state)
                select goal;
            this.currentGoal = goals.FirstOrDefault();
            AutoccultistPlugin.Instance.LogTrace($"Next goal is {this.currentGoal?.Name ?? "[none]"}");
        }
    }
}