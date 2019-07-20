using System;
using System.Linq;
using System.Collections.Generic;
using Autoccultist.Brain.Config;
using Assets.Core.Interfaces;

namespace Autoccultist.Brain
{
    public class AutoccultistBrain : IGameState
    {
        private BrainConfig config;

        private SituationManager situationManager = new SituationManager();
        private CardManager cardManager = new CardManager();

        private Goal goal;

        public AutoccultistBrain(BrainConfig config)
        {
            this.config = config;
        }

        public void Start()
        {
            GameAPI.Heartbeat += OnHeartbeat;
            if (this.IsGoalSatisfied())
            {
                this.goal = null;
            }

            if (this.goal == null)
            {
                this.ObtainNextGoal();
            }
        }

        public void Stop()
        {
            GameAPI.Heartbeat -= OnHeartbeat;
        }

        public void LogStatus()
        {
            AutoccultistMod.Instance.Info(string.Format("My goal is {0}", this.goal != null ? this.goal.Name : "<none>"));
            AutoccultistMod.Instance.Info(string.Format("I have {0} satisfiable imperatives", this.GetSatisfiableImperatives().Count));
            if (this.goal != null)
            {
                foreach (var imperative in this.goal.Imperatives.OrderByDescending(x => x.Priority))
                {
                    AutoccultistMod.Instance.Info(string.Format("Imperative - {0}", imperative.Name));
                    AutoccultistMod.Instance.Info("-- Verb available: " + this.SituationIsAvailable(imperative.Verb));
                    foreach (var choice in imperative.StartingRecipe.Slots)
                    {
                        AutoccultistMod.Instance.Info(string.Format("-- Slot {0} satisfied: {1}", choice.Key, this.CardsCanBeSatisfied(new[] { choice.Value })));
                    }
                }
            }
            else
            {
                foreach (var goal in this.config.Goals)
                {
                    AutoccultistMod.Instance.Info("Goal " + goal.Name);
                    // TODO: dump goal requirements and completions
                }
            }
        }

        public bool SituationIsAvailable(string situationId)
        {
            return this.situationManager.SituationIsAvailable(situationId);
        }

        public bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices)
        {
            return this.cardManager.CardsCanBeSatisfied(choices);
        }

        private void OnHeartbeat(object sender, EventArgs e)
        {
            this.situationManager.ClearCompletedSituations();

            if (this.IsGoalSatisfied())
            {
                this.goal = null;
            }

            if (this.goal == null)
            {
                this.ObtainNextGoal();
                if (this.goal == null)
                {
                    return;
                }
                else
                {
                    AutoccultistMod.Instance.Info(string.Format("My goal is now {0}", this.goal.Name));
                }
            }

            // Scan through all possible imperatives and invoke the ones that can start.
            //  Where multiple imperatives try for the same verb, invoke the highest priority
            var candidateGroups =
                from imperative in this.GetSatisfiableImperatives()
                orderby imperative.Priority descending
                group imperative by imperative.Verb into situationGroup
                select situationGroup;


            foreach (var group in candidateGroups)
            {
                var candidate = group.FirstOrDefault();
                if (candidate == null)
                {
                    return;
                }

                AutoccultistMod.Instance.Trace(string.Format("Starting imperative {0}", candidate.Name));
                this.situationManager.ExecuteSituationSolution(candidate);
            }
        }

        private IList<Imperative> GetSatisfiableImperatives()
        {
            if (this.goal == null)
            {
                return new Imperative[0];
            }

            var imperatives =
                from imperative in this.goal.Imperatives
                where imperative.CanExecute(this)
                select imperative;
            return imperatives.ToList();
        }

        private bool IsGoalSatisfied()
        {
            if (this.goal == null)
            {
                return true;
            }
            return this.goal.IsSatisfied(this);
        }

        private void ObtainNextGoal()
        {
            var goals =
                from goal in this.config.Goals
                where goal.CanActivate(this)
                select goal;
            this.goal = goals.FirstOrDefault();
        }
    }
}