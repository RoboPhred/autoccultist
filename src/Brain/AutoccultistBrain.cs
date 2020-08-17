namespace Autoccultist.Brain
{
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;

    /// <summary>
    /// The AutoccultistBrain takes a list of goals and runs them to completion.
    /// </summary>
    public class AutoccultistBrain
    {
        private IReadOnlyList<IGoal> goals;

        private IGoal currentGoal;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoccultistBrain"/> class.
        /// </summary>
        /// <param name="goals">The list of goals to accomplish.</param>
        public AutoccultistBrain(IReadOnlyList<IGoal> goals)
        {
            this.goals = goals;
        }

        /// <summary>
        /// Gets a value indicating whether the brain is running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Starts the brain executing the configured plan.
        /// </summary>
        public void Start()
        {
            if (this.IsRunning)
            {
                return;
            }

            this.IsRunning = true;
            AutoccultistPlugin.Instance.LogInfo("Starting brain");
        }

        /// <summary>
        /// Stops the brain executing the configured plan.
        /// </summary>
        public void Stop()
        {
            AutoccultistPlugin.Instance.LogInfo("Stopping brain");
            this.IsRunning = false;
        }

        /// <summary>
        /// Clears the current goal, resets progress tracking, and tries to obtain the first possible goal.
        /// </summary>
        /// <param name="replacementGoals">The replacement list of goals to use, if desired.</param>
        public void Reset(IReadOnlyList<IGoal> replacementGoals = null)
        {
            this.currentGoal = null;
            this.goals = replacementGoals ?? this.goals;
        }

        /// <summary>
        /// Checks to see if the goal is still valid, and if any goal imperatives should be triggered.
        /// </summary>
        /// <param name="state">The game state to update on.</param>
        public void Update(IGameState state)
        {
            if (!this.IsRunning)
            {
                return;
            }

            this.ResetGoalIfSatisfiedOrNull(state);
            if (this.currentGoal != null)
            {
                this.TryStartImperatives(state);
            }
        }

        /// <summary>
        /// Dumps information on the state of the brain to the console.
        /// </summary>
        /// <param name="state">The state to log the status from.</param>
        public void LogStatus(IGameState state)
        {
            AutoccultistPlugin.Instance.LogInfo(string.Format("My goal is {0}", this.currentGoal?.Name ?? "<none>"));
            AutoccultistPlugin.Instance.LogInfo(string.Format("I have {0} satisfiable imperatives", this.GetSatisfiableImperatives(state).Count));
            if (this.currentGoal != null)
            {
                foreach (var imperative in this.currentGoal.Imperatives.OrderByDescending(x => x.Priority))
                {
                    AutoccultistPlugin.Instance.LogInfo($"Imperative - {imperative.Name}");
                    AutoccultistPlugin.Instance.LogInfo($"-- Requirements satisfied: {imperative.Requirements?.IsConditionMet(state) ?? true}");
                    AutoccultistPlugin.Instance.LogInfo($"-- Forbidders in place: {imperative.Forbidders?.IsConditionMet(state) ?? false}");
                    AutoccultistPlugin.Instance.LogInfo($"-- Situation {imperative.Operation.Situation} available: {state.IsSituationAvailable(imperative.Operation.Situation)}");
                    foreach (var choice in imperative.Operation.StartingRecipe.Slots)
                    {
                        AutoccultistPlugin.Instance.LogInfo($"-- Slot {choice.Key} satisfied: {state.CardsCanBeSatisfied(new[] { choice.Value })}");
                    }
                }
            }
            else
            {
                foreach (var goal in this.goals)
                {
                    AutoccultistPlugin.Instance.LogInfo("Goal " + goal.Name);
                }
            }
        }

        private void TryStartImperatives(IGameState state)
        {
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

                if (!SituationOrchestrator.IsSituationAvailable(operation.Situation))
                {
                    continue;
                }

                SituationOrchestrator.ExecuteOperation(operation);
            }
        }

        private void ResetGoalIfSatisfiedOrNull(IGameState state)
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

        private IList<IImperative> GetSatisfiableImperatives(IGameState state)
        {
            if (this.currentGoal == null)
            {
                return new IImperative[0];
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
                from goal in this.goals
                where goal.CanActivate(state)
                select goal;
            this.currentGoal = goals.FirstOrDefault();
            AutoccultistPlugin.Instance.LogTrace($"Next goal is {this.currentGoal?.Name ?? "[none]"}");
        }
    }
}
