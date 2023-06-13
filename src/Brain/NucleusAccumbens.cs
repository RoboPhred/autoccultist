namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Manages goal execution.
    /// </summary>
    public static class NucleusAccumbens
    {
        private static readonly HashSet<IGoal> ActiveGoals = new();

        private static readonly Dictionary<IGoal, long> PendingCompletions = new();

        private static bool isStartingImpulse = false;
        private static GameAPI.PauseToken pauseToken;

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

        public static void Initialize()
        {
            MechanicalHeart.OnBeat += OnBeat;
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
        /// Dumps the status of all goals to a string.
        /// </summary>
        /// <returns>The status.</returns>
        public static string DumpStatus()
        {
            var sb = new StringBuilder();
            var state = GameStateProvider.Current;
            sb.Append("Active Goals:\n");
            foreach (var goal in ActiveGoals)
            {
                sb.AppendFormat("- {0}\n", goal.Name);
                sb.AppendFormat("- Impulses\n");
                foreach (var impulse in goal.Impulses.OrderBy(x => x.Priority))
                {
                    sb.AppendFormat("- - {0}\n", impulse.Name);
                    sb.AppendFormat("- - - Priority: {0}\n", impulse.Priority);

                    var reqMatch = impulse?.IsConditionMet(state);
                    sb.AppendFormat("- - - Requirements met: {0}\n", reqMatch.IsConditionMet);
                    if (!reqMatch)
                    {
                        sb.AppendFormat("- - - - {0}\n", reqMatch);
                    }

                    var opMatch = impulse.Operation.IsConditionMet(state);
                    sb.AppendFormat("- - - Operation ready: {0}\n", opMatch.IsConditionMet);
                    if (!opMatch)
                    {
                        sb.AppendFormat("- - - - {0}\n", opMatch);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Update and execute goals.
        /// </summary>
        private static void OnBeat(object sender, EventArgs e)
        {
            TryCompleteGoals();
            CheckImpulses();
        }

        private static void TryCompleteGoals()
        {
            var state = GameStateProvider.Current;

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

        private static async void CheckImpulses()
        {
            if (isStartingImpulse)
            {
                return;
            }

            isStartingImpulse = true;

            // FIXME: We have unpaused gaps when we should be immediately starting more operations.
            // Might be happening when an op ends then needs to immediately re-begin.
            // This is because, while we wrap starting ops in our own pause token, we cannot wrap the completion of an op to launching the new op.

            try
            {
                // Scan through all possible impulses and invoke the ones that can start.
                //  Where multiple impulses try for the same verb, invoke the highest priority
                var operations =
                    from goal in ActiveGoals
                    from impulse in goal.Impulses
                    where IsSituationAvailable(impulse.Operation.Situation)
                    where impulse.CanExecute(GameStateProvider.Current)
                    orderby impulse.Priority descending
                    group impulse.Operation by impulse.Operation.Situation into situationGroup
                    select situationGroup.FirstOrDefault();

                // We used to start all non-conflicting operations, but that can cause two ops to think they can start when they both
                // want the same single card.
                // Instead, start one at a time, then search again.
                var operation = operations.FirstOrDefault();
                if (operation != null)
                {
                    // We started doing things, pause.
                    // Note: The operation will try to pause/unpause too, but we want to pause outside of that until we are sure
                    // we have no more operations left to start.
                    if (pauseToken == null)
                    {
                        pauseToken = GameAPI.Pause();
                    }

                    var orchestration = SituationOrchestrator.StartOperation(operation);
                    await orchestration.AwaitCurrentTask();
                }
                else
                {
                    // Nothing more to do, let it unpause
                    pauseToken?.Dispose();
                    pauseToken = null;
                }
            }
            finally
            {
                isStartingImpulse = false;
            }
        }


        private static bool IsSituationAvailable(string situationId)
        {
            if (SituationOrchestrator.CurrentOrchestrations.Keys.Contains(situationId))
            {
                return false;
            }

            var situationState = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == situationId);
            if (situationState == null)
            {
                // Situation doesnt exist.
                return false;
            }

            if (situationState.State == SecretHistories.Enums.StateEnum.Unstarted)
            {
                // We can take over unstarted situations.
                return true;
            }

            if (situationState.State == SecretHistories.Enums.StateEnum.Ongoing)
            {
                // Only take over ongoing situations if they are empty.
                return situationState.RecipeSlots.All(x => x.Card == null);
            }

            // Don't know what the situation is doing, leave it alone.
            return false;
        }
    }
}
