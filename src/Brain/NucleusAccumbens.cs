namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Takes imperatives and manages the execution of their reactions.
    /// </summary>
    public static class NucleusAccumbens
    {
        private static readonly HashSet<IImperative> ActiveImperatives = new();

        // These are redundant, but we need efficient lookup of reactions.
        // This is a mess... We need to track:
        // - Reactions, so we know not to run them multiple times
        // - ReactionExecutions by imperative, so we can abort them.
        // - Reactions by ReactionExecution, so we can remove them from ActiveReactions on complete.
        //   I dont think we can rely on lambda hooks here as that will lead to memory leaks... I think... I can't remember if our static class will keep our handler around.
        private static readonly HashSet<IReaction> RunningReactions = new();
        private static readonly Dictionary<IReactionExecution, IReaction> ReactionsByExecution = new();
        private static readonly Dictionary<IImperative, HashSet<IReactionExecution>> ActiveImperativeExecutions = new();

        private static bool isCheckingReactions = false;
        private static GameAPI.PauseToken pauseToken;

        /// <summary>
        /// Raised when a goal is completed.
        /// </summary>
        public static event EventHandler<ImperativeEventArgs> OnImperativeCompleted;

        /// <summary>
        /// Gets a list of the current active goals.
        /// </summary>
        public static IReadOnlyList<IImperative> CurrentImperatives
        {
            get
            {
                return ActiveImperatives.ToArray();
            }
        }

        public static void Initialize()
        {
            MechanicalHeart.OnBeat += OnBeat;
        }

        /// <summary>
        /// Adds an imperative to manage.
        /// </summary>
        /// <param name="imperative">The imperative to add.</param>
        public static void AddImperative(IImperative imperative)
        {
            if (ActiveImperatives.Contains(imperative))
            {
                return;
            }

            if (imperative.IsSatisfied(GameStateProvider.Current))
            {
                return;
            }

            ActiveImperatives.Add(imperative);
            ActiveImperativeExecutions.Add(imperative, new HashSet<IReactionExecution>());
        }

        /// <summary>
        /// Removes an imperative from ongoing execution.  This will abort all reactions associated with this imperative.
        /// </summary>
        /// <param name="imperative">The imperative to remove.</param>
        public static void RemoveImperative(IImperative imperative)
        {
            ActiveImperatives.Remove(imperative);

            foreach (var execution in ActiveImperativeExecutions[imperative])
            {
                execution.Abort();
            }

            ActiveImperativeExecutions.Remove(imperative);
        }

        /// <summary>
        /// Clears all goals and resets the goal driver.
        /// </summary>
        public static void Reset()
        {
            foreach (var execution in ActiveImperativeExecutions.Values.SelectMany(r => r).ToArray())
            {
                execution.Abort();
            }

            ActiveImperatives.Clear();
            ActiveImperativeExecutions.Clear();
        }

        public static string DumpStatus()
        {
            var sb = new StringBuilder();

            foreach (var imperative in ActiveImperatives)
            {
                sb.AppendFormat("Imperative: {0}\n", imperative.ToString());

                var canActivate = imperative.CanActivate(GameStateProvider.Current);
                sb.AppendFormat("- Can Activate: {0}\n", canActivate.IsConditionMet);
                if (!canActivate)
                {
                    sb.AppendFormat("- - Reason: {0}\n", canActivate.ToString());
                }

                var isSatisfied = imperative.IsSatisfied(GameStateProvider.Current);
                sb.AppendFormat("- Is Satisfied: {0}\n", isSatisfied.IsConditionMet);
                if (!isSatisfied)
                {
                    sb.AppendFormat("- - Reason: {0}\n", isSatisfied.ToString());
                }

                sb.AppendLine("- Reactions");
                foreach (var reaction in imperative.GetReactions(GameStateProvider.Current))
                {
                    sb.AppendFormat("- - Reaction: {0}\n", reaction.ToString());
                    sb.AppendFormat("- - - Priority: {0}\n", reaction.Priority);
                    var isConditionMet = reaction.IsConditionMet(GameStateProvider.Current);
                    sb.AppendFormat("- - - Is Condition Met: {0}\n", isConditionMet.IsConditionMet);
                    if (!isConditionMet)
                    {
                        sb.AppendFormat("- - - - Reason: {0}\n", isConditionMet.ToString());
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
            // Might be a bit performance heavy to check these states every beat, but so far we don't have any slowdown.
            TryInvokeReactions();
            TryCompleteImperatives();
        }

        private static async void TryInvokeReactions()
        {
            if (isCheckingReactions)
            {
                return;
            }

            isCheckingReactions = true;

            try
            {
                while (true)
                {
                    // Scan through all possible reactions and invoke the highest priority one that can start
                    var reactions =
                        from p in GetActiveReactions()
                        let r = p.Value
                        where !RunningReactions.Contains(r)
                        where r.IsConditionMet(GameStateProvider.Current)
                        orderby r.Priority descending
                        select p;

                    var nextPair = reactions.FirstOrDefault();
                    var imperative = nextPair.Key;
                    var reaction = nextPair.Value;
                    if (reaction == null)
                    {
                        return;
                    }

                    NoonUtility.LogWarning("NucleusAccumbens.TryInvokeReactions: Got reaction" + reaction.ToString());

                    if (pauseToken == null)
                    {
                        pauseToken = GameAPI.Pause();
                    }

                    var execution = reaction.Execute();

                    OnReactionStarted(imperative, reaction, execution);

                    await execution.AwaitStarted();
                }
            }
            finally
            {
                pauseToken?.Dispose();
                pauseToken = null;
                isCheckingReactions = false;
            }
        }

        private static IEnumerable<KeyValuePair<IImperative, IReaction>> GetActiveReactions()
        {
            return ActiveImperatives.SelectMany(i => i.GetReactions(GameStateProvider.Current).Select(r => new KeyValuePair<IImperative, IReaction>(i, r)));
        }

        private static void TryCompleteImperatives()
        {
            foreach (var imperative in ActiveImperatives.ToArray())
            {
                if (imperative.IsSatisfied(GameStateProvider.Current))
                {
                    CompleteImperative(imperative);
                }
            }
        }

        private static void CompleteImperative(IImperative imperative)
        {
            if (!ActiveImperatives.Remove(imperative))
            {
                return;
            }

            OnImperativeCompleted?.Invoke(null, new ImperativeEventArgs(imperative));
        }

        private static void OnReactionStarted(IImperative imperative, IReaction reaction, IReactionExecution execution)
        {
            RunningReactions.Add(reaction);
            ActiveImperativeExecutions[imperative].Add(execution);
            ReactionsByExecution.Add(execution, reaction);

            execution.Completed += HandleReactionCompleted;
        }

        private static void HandleReactionCompleted(object sender, EventArgs e)
        {
            var execution = (IReactionExecution)sender;
            execution.Completed -= HandleReactionCompleted;

            if (ReactionsByExecution.TryGetValue(execution, out var reaction))
            {
                RunningReactions.Remove(reaction);
            }
            else
            {
                Autoccultist.Instance.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find reaction for execution {execution}");
            }

            ReactionsByExecution.Remove(execution);

            // Shame we don't know what imperative this was part of, but we shouldn't have very many parallel imperatives
            foreach (var executions in ActiveImperativeExecutions.Values)
            {
                if (executions.Remove(execution))
                {
                    break;
                }
            }

            Autoccultist.Instance.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find imperative for execution {execution}");
        }
    }
}
