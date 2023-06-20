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
        // - impulses, so we know not to run them multiple times
        // - reactions by imperative, so we can abort them.
        // - impulses by reaction, so we can remove them from RunningImpulses on complete.
        //   I dont think we can rely on lambda hooks here as that will lead to memory leaks... I think... I can't remember if our static class will keep our handler around.
        private static readonly HashSet<IImpulse> RunningImpulses = new();
        private static readonly Dictionary<IReaction, IImpulse> ImpulsesByReaction = new();
        private static readonly Dictionary<IImperative, HashSet<IReaction>> ActiveReactionsByImperative = new();

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
            ActiveReactionsByImperative.Add(imperative, new HashSet<IReaction>());
        }

        /// <summary>
        /// Removes an imperative from ongoing execution.  This will abort all reactions associated with this imperative.
        /// </summary>
        /// <param name="imperative">The imperative to remove.</param>
        public static void RemoveImperative(IImperative imperative)
        {
            ActiveImperatives.Remove(imperative);

            foreach (var execution in ActiveReactionsByImperative[imperative])
            {
                execution.Abort();
            }

            ActiveReactionsByImperative.Remove(imperative);
        }

        /// <summary>
        /// Clears all goals and resets the goal driver.
        /// </summary>
        public static void Reset()
        {
            foreach (var execution in ActiveReactionsByImperative.Values.SelectMany(r => r).ToArray())
            {
                execution.Abort();
            }

            ActiveImperatives.Clear();
            ActiveReactionsByImperative.Clear();
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
                var reactions = from pair in imperative.GetImpulses(GameStateProvider.Current).Select((r, i) => new { Reaction = r, Index = i })
                                orderby pair.Reaction.Priority descending, pair.Index ascending
                                select pair.Reaction;
                foreach (var reaction in reactions)
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
                    var pairing = PerfMonitor.Monitor($"GetFirstReadyImpulse", () => GetReadyImpulses().FirstOrDefault());
                    if (pairing == null)
                    {
                        return;
                    }

                    if (pauseToken == null)
                    {
                        pauseToken = GameAPI.Pause();
                    }

                    var execution = pairing.Reaction.Execute();

                    OnReactionStarted(pairing.Imperative, pairing.Reaction, execution);

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

        /// <summary>
        /// Gets all reactions that are ready to be invoked, in order of priority.
        /// </summary>
        private static IEnumerable<EnumeratedReaction> GetReadyImpulses()
        {
            return from reaction in GetAllImpulses()
                   where !RunningImpulses.Contains(reaction.Reaction)
                   where reaction.Reaction.IsConditionMet(GameStateProvider.Current)
                   select reaction;
        }

        /// <summary>
        /// Gets all reactions that are currently active, in order of priority.
        /// </summary>
        private static IEnumerable<EnumeratedReaction> GetAllImpulses()
        {
            // Note: Imperatives come in an indeterminate order due to the HashSet... We should use a consistant order here.
            return
                from imperative in ActiveImperatives
                from reaction in imperative.GetImpulses(GameStateProvider.Current).Distinct().Select((r, index) => new { Value = r, Index = index })
                orderby reaction.Value.Priority descending, reaction.Index ascending
                select new EnumeratedReaction { Imperative = imperative, Reaction = reaction.Value };
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
            // Clean up our mess of tracking maps.
            // FIXME: This is disgusting.  Track these better.
            if (ActiveReactionsByImperative.TryGetValue(imperative, out var executions))
            {
                foreach (var execution in executions.ToArray())
                {
                    execution.Completed -= HandleReactionCompleted;
                    execution.Abort();

                    if (ImpulsesByReaction.TryGetValue(execution, out var reaction))
                    {
                        RunningImpulses.Remove(reaction);
                    }

                    ImpulsesByReaction.Remove(execution);
                }

                ActiveReactionsByImperative.Remove(imperative);
            }

            if (!ActiveImperatives.Remove(imperative))
            {
                return;
            }

            OnImperativeCompleted?.Invoke(null, new ImperativeEventArgs(imperative));
        }

        private static void OnReactionStarted(IImperative imperative, IImpulse reaction, IReaction execution)
        {
            // FIXME: We keep so many variations of the same imperative => reaction => execution mapping.  Clean this up
            RunningImpulses.Add(reaction);
            ActiveReactionsByImperative[imperative].Add(execution);
            ImpulsesByReaction.Add(execution, reaction);

            execution.Completed += HandleReactionCompleted;
        }

        private static void HandleReactionCompleted(object sender, EventArgs e)
        {
            var execution = (IReaction)sender;
            execution.Completed -= HandleReactionCompleted;

            // FIXME: More disgusting tracking maps.
            if (ImpulsesByReaction.TryGetValue(execution, out var reaction))
            {
                RunningImpulses.Remove(reaction);
            }
            else
            {
                Autoccultist.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find reaction for execution {execution}");
            }

            ImpulsesByReaction.Remove(execution);

            // Shame we don't know what imperative this was part of, but we shouldn't have very many parallel imperatives
            foreach (var executions in ActiveReactionsByImperative.Values)
            {
                if (executions.Remove(execution))
                {
                    return;
                }
            }

            Autoccultist.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find imperative for execution {execution}");
        }

        private class EnumeratedReaction
        {
            public IImperative Imperative { get; set; }

            public IImpulse Reaction { get; set; }

            public int Index { get; set; }
        }
    }
}
