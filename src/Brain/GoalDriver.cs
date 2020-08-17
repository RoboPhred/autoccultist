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
        /// Add a goal to the active goals.
        /// </summary>
        /// <param name="goal">The goal to make active.</param>
        public static void AddGoal(IGoal goal)
        {
            ActiveGoals.Add(goal);
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
        /// Update and execute goals.
        /// </summary>
        /// <param name="state">The game state to update on.</param>
        public static void Update(IGameState state)
        {
            TryCompleteGoals(state);
            TryStartImperatives(state);
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
