namespace Autoccultist.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;

    /// <summary>
    /// Manages goal execution.
    /// </summary>
    public static class NucleusAccumbens
    {
        private static readonly HashSet<IGoal> ActiveGoals = new();

        private static readonly Dictionary<IGoal, long> PendingCompletions = new();

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
            TryStartImpulses(state);
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
                AutoccultistPlugin.Instance.LogInfo("- Impulses");
                foreach (var impulse in goal.Impulses.OrderBy(x => x.Priority))
                {
                    AutoccultistPlugin.Instance.LogInfo("- - " + impulse.Name);
                    AutoccultistPlugin.Instance.LogInfo("- - - Priority: " + impulse.Priority);
                    AutoccultistPlugin.Instance.LogInfo("- - - Requirements met: " + (impulse.Requirements?.IsConditionMet(state) != false));
                    AutoccultistPlugin.Instance.LogInfo("- - - Forbidders in place: " + (impulse.Forbidders?.IsConditionMet(state) == true));
                    AutoccultistPlugin.Instance.LogInfo("- - - Operation ready: " + impulse.Operation.IsConditionMet(state));
                }
            }
        }

        private static void TryCompleteGoals(IGameState state)
        {
            // note: This PendingCompletions logic was an attempt to fix an issue where goals were completing prematurely.
            // The root issue was that the cards in situation output stacks were not being counted, and has been fixed.

            // note: ToHashSet throws a MissingMethodException in unity as it cant find the right HashSet ctor.
            var completedGoals =
                new HashSet<IGoal>(from goal in ActiveGoals
                                   where goal.IsSatisfied(state)
                                   select goal);

            foreach (var goal in completedGoals)
            {
                if (PendingCompletions.TryGetValue(goal, out var timeStamp))
                {
                    if (timeStamp + 200 < DateTime.Now.Ticks)
                    {
                        CompleteGoal(goal);
                    }
                }
                else
                {
                    PendingCompletions.Add(goal, DateTime.Now.Ticks);
                }
            }

            foreach (var goal in PendingCompletions.Keys.ToArray())
            {
                if (!completedGoals.Contains(goal))
                {
                    PendingCompletions.Remove(goal);
                }
            }
        }

        private static void CompleteGoal(IGoal goal)
        {
            ActiveGoals.Remove(goal);
            PendingCompletions.Remove(goal);
            OnGoalCompleted?.Invoke(null, new GoalCompletedEventArgs(goal));
            BrainEventSink.OnGoalCompleted(goal);
        }

        private static void TryStartImpulses(IGameState state)
        {
            // Scan through all possible impulses and invoke the ones that can start.
            //  Where multiple impulses try for the same verb, invoke the highest priority
            var operations =
                from goal in ActiveGoals
                from impulse in goal.Impulses
                where impulse.CanExecute(state)
                orderby impulse.Priority descending
                group impulse.Operation by impulse.Operation.Situation into situationGroup
                select situationGroup.FirstOrDefault();

            foreach (var operation in operations)
            {
                SituationOrchestrator.ExecuteOperation(operation);
            }
        }
    }
}
