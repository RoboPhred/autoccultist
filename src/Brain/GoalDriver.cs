namespace Autoccultist.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;

    /// <summary>
    /// A static class responsible for managing goal execution.
    /// </summary>
    public static class GoalDriver
    {
        private static readonly HashSet<IGoal> ActiveGoals = new HashSet<IGoal>();

        /// <summary>
        /// Raised when a goal is completed.
        /// </summary>
        public static event EventHandler<GoalCompletedEventArgs> OnGoalCompleted;

        /// <summary>
        /// Gets a list of the current active goals.
        /// </summary>
        public static IReadOnlyList<IGoal> CurrentGoals
        {
            get
            {
                return ActiveGoals.ToArray();
            }
        }

        /// <summary>
        /// Add a goal to the active goals.
        /// </summary>
        /// <param name="goal">The goal to make active.</param>
        public static void AddGoal(IGoal goal)
        {
            ActiveGoals.Add(goal);
            BrainEventSink.OnGoalStarted(goal);
        }

        /// <summary>
        /// Remove a goal and make it inactive.
        /// </summary>
        /// <param name="goal">The goal to remove.</param>
        public static void RemoveGoal(IGoal goal)
        {
            ActiveGoals.Remove(goal);
        }

        /// <summary>
        /// Clears all goals and resets the goal driver.
        /// </summary>
        public static void Reset()
        {
            ActiveGoals.Clear();
        }

        /// <summary>
        /// Update and execute goals.
        /// </summary>
        public static void Update()
        {
            var state = GameStateProvider.Current;
            TryCompleteGoals(state);
            TryStartImperatives(state);
        }

        /// <summary>
        /// Write out status to the console.
        /// </summary>
        public static void DumpStatus()
        {
            var state = GameStateProvider.Current;
            AutoccultistPlugin.Instance.LogInfo("Active Goals:");
            foreach (var goal in ActiveGoals)
            {
                AutoccultistPlugin.Instance.LogInfo("- " + goal.Name);
                AutoccultistPlugin.Instance.LogInfo("- Imperatives");
                foreach (var imperative in goal.Imperatives.OrderBy(x => x.Priority))
                {
                    AutoccultistPlugin.Instance.LogInfo("- - " + imperative.Name);
                    AutoccultistPlugin.Instance.LogInfo("- - - Requirements met: " + (imperative.Requirements?.IsConditionMet(state) != false));
                    AutoccultistPlugin.Instance.LogInfo("- - - Forbidders in place: " + (imperative.Forbidders?.IsConditionMet(state) == true));
                    AutoccultistPlugin.Instance.LogInfo("- - - Operation ready: " + imperative.Operation.IsConditionMet(state));
                }
            }
        }

        private static void TryCompleteGoals(IGameState state)
        {
            var completedGoals =
                from goal in ActiveGoals
                where goal.IsSatisfied(state)
                select goal;

            foreach (var goal in completedGoals.ToArray())
            {
                CompleteGoal(goal);
            }
        }

        private static void CompleteGoal(IGoal goal)
        {
            ActiveGoals.Remove(goal);
            OnGoalCompleted?.Invoke(null, new GoalCompletedEventArgs(goal));
            BrainEventSink.OnGoalCompleted(goal);
        }

        private static void TryStartImperatives(IGameState state)
        {
            // Scan through all possible imperatives and invoke the ones that can start.
            //  Where multiple imperatives try for the same verb, invoke the highest priority
            var operations =
                from goal in ActiveGoals
                from imperative in goal.Imperatives
                where SituationOrchestrator.IsSituationAvailable(imperative.Operation.Situation)
                where imperative.CanExecute(state)
                orderby imperative.Priority descending
                group imperative.Operation by imperative.Operation.Situation into situationGroup
                select situationGroup.FirstOrDefault();

            foreach (var operation in operations)
            {
                SituationOrchestrator.ExecuteOperation(operation);
            }
        }
    }
}
