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

        private IEnumerator<IGoal> goalEnumerator;

        private IGoal currentGoal;

        private bool announceGoalChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoccultistBrain"/> class.
        /// </summary>
        /// <param name="goals">The list of goals to accomplish.</param>
        public AutoccultistBrain(IReadOnlyList<IGoal> goals)
        {
            this.Reset(goals);
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
        }

        /// <summary>
        /// Stops the brain executing the configured plan.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
        }

        /// <summary>
        /// Clears the current goal, resets progress tracking, and tries to obtain the first possible goal.
        /// </summary>
        /// <param name="replacementGoals">The replacement list of goals to use, if desired.</param>
        public void Reset(IReadOnlyList<IGoal> replacementGoals = null)
        {
            this.announceGoalChange = true;
            this.goals = replacementGoals ?? this.goals;
            this.goalEnumerator = this.goals.GetEnumerator();
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

            this.UpdateCurrentGoal(state);
            this.TryStartImperatives(state);
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

        private void UpdateCurrentGoal(IGameState state)
        {
            if (this.currentGoal?.IsSatisfied(state) == false)
            {
                // Still working on the current goal.
                return;
            }

            try
            {
                if (this.currentGoal != null)
                {
                    AutoccultistPlugin.Instance.LogInfo($"Current goal {this.currentGoal.Name} is now satisfied.");
                }

                if (this.currentGoal != null)
                {
                    // Goal is changing, we should announce it.
                    this.announceGoalChange = true;
                }

                // Goal is satisfied, null out the current goal.
                this.currentGoal = null;

                // Find the next goal that is not satisfied.
                // For the first iteration after the first goal, Current should be equal to the previous currentGoal, which is satisfied.
                while (this.goalEnumerator.Current?.IsSatisfied(state) != false)
                {
                    if (!this.goalEnumerator.MoveNext())
                    {
                        // Done with all goals
                        GameAPI.Notify("The tasks are done", "All goals are completed.");
                        this.Stop();
                        return;
                    }
                }

                // TODO: Rethink CanActivate; doesn't make sense with linear goals.  May still be useful with more advanced goal determination
                /*
                if (!this.goalEnumerator.Current.CanActivate(state))
                {
                    // Next goal cannot activate yet.
                    if (this.announceGoalChange)
                    {
                        AutoccultistPlugin.Instance.LogTrace($"Next goal is {this.goalEnumerator.Current.Name}, but it is not yet available.");
                    }

                    return;
                }
                */

                this.currentGoal = this.goalEnumerator.Current;

                // This only happens once, we do not need to check announceGoalChange
                AutoccultistPlugin.Instance.LogInfo($"Starting goal {this.currentGoal.Name}.");
            }
            finally
            {
                this.announceGoalChange = false;
            }
        }
    }
}
