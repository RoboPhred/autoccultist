using System.Linq;
using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public class AutoccultistBrain : IGameState
    {
        private BrainConfig config;

        private Goal currentInstinct;   // A "critical-level" Goal: any imperatives through this goal are always considered top priority.
        private Goal currentGoal;       // A "goal-level" Goal: this is important, and prioritized over the aspiration.
        private Goal currentAspiration; // A "maintenance-level" Goal: this is a Goal to work on with spare available resources.

        public AutoccultistBrain(BrainConfig config)
        {
            this.config = config;
        }

        public void Start()
        {
            if (!this.CanGoalActivate())
            {
                this.currentGoal = null;
            }

            this.currentInstinct = this.currentInstinct ?? this.ObtainNextGoal(TaskPriority.Critical);
            this.currentGoal = this.currentGoal ?? this.ObtainNextGoal();
            this.currentAspiration = this.currentAspiration ?? this.ObtainNextGoal(TaskPriority.Maintenance);
        }

        public void Stop()
        {
        }

        public void Update()
        {
            Start();
            if (this.currentGoal == null)
            {
                return;
            }

            foreach(Goal currGoal in new Goal[]{ currentInstinct, currentGoal, currentAspiration })
            {
                // Scan through all possible imperatives and invoke the ones that can start.
                //  Where multiple imperatives try for the same verb, invoke the highest priority
                var candidateGroups =
                    from imperative in this.GetSatisfiableImperatives()
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

                    SituationOrchestrator.ExecuteOperation(operation);
                }
            }
        }

        public void LogStatus()
        {
            AutoccultistPlugin.Instance.LogInfo(string.Format("My goal is {0}", this.currentGoal != null ? this.currentGoal.Name : "<none>"));
            AutoccultistPlugin.Instance.LogInfo(string.Format("I have {0} satisfiable imperatives", this.GetSatisfiableImperatives().Count));
            if (this.currentGoal != null)
            {
                foreach (var imperative in this.currentGoal.Imperatives.OrderByDescending(x => x.Priority))
                {
                    AutoccultistPlugin.Instance.LogInfo($"Imperative - {imperative.Name}");
                    AutoccultistPlugin.Instance.LogInfo($"-- Situation {imperative.Operation.Situation} available: {this.SituationIsAvailable(imperative.Operation.Situation)}");
                    foreach (var choice in imperative.Operation.StartingRecipe.Slots)
                    {
                        AutoccultistPlugin.Instance.LogInfo($"-- Slot {choice.Key} satisfied: {this.CardsCanBeSatisfied(new[] { choice.Value })}");
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
                        AutoccultistPlugin.Instance.LogInfo("-- Required cards (" + goal.CompletedWhen.mode.ToString() + "):");
                        foreach (CardChoice card in goal.RequiredCards.Requirements)
                        {
                            AutoccultistPlugin.Instance.LogInfo("-- -- " + card + " satisfied " + this.CardsCanBeSatisfied(new[] { card }));
                        }

                        AutoccultistPlugin.Instance.LogInfo("-- Required cards (" + goal.CompletedWhen.mode.ToString() + "):");
                        foreach (CardChoice card in goal.CompletedWhen.Requirements)
                        {
                            AutoccultistPlugin.Instance.LogInfo("-- -- " + card + " satisfied " + this.CardsCanBeSatisfied(new[] { card }));
                        }
                    }
                }
            }
        }

        public bool SituationIsAvailable(string situationId)
        {
            return SituationOrchestrator.SituationIsAvailable(situationId);
        }

        public bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices)
        {
            return CardManager.CardsCanBeSatisfied(choices);
        }

        private IList<Imperative> GetSatisfiableImperatives()
        {
            foreach(Goal g in new Goal[] { this.currentInstinct, this.currentGoal, this.currentAspiration })
            {
                if(g == null)
                {
                    continue;
                }

                IEnumerable<Imperative> imperatives =
                    from imperative in (g.Imperatives)
                    where imperative.CanExecute(this)
                    select imperative;

                if(imperatives.Count() > 0)
                {
                    return imperatives.ToList();
                }
            }

            return new Imperative[0];
        }

        private bool IsGoalSatisfied()
        {
            if (this.currentGoal == null)
            {
                return true;
            }
            return this.currentGoal.IsSatisfied(this);
        }

        private bool CanGoalActivate()
        {
            if (this.currentGoal == null)
            {
                return false;
            }
            return this.currentGoal.CanActivate(this);
        }

        private Goal ObtainNextGoal(TaskPriority goalLevel = TaskPriority.Goal)
        {
            var goals =
                from goal in this.config.Goals
                where goal.CanActivate(this)
                where goal.Priority == goalLevel
                select goal;
            var ReturnValue = goals.FirstOrDefault();
            AutoccultistPlugin.Instance.LogTrace($"Next {goalLevel.ToString()} goal is {ReturnValue?.Name ?? "[none]"}");
            return ReturnValue;
        }
    }
}