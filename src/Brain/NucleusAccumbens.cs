namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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

        private static bool isInitialized = false;

        private static bool isActive = false;

        private static GameAPI.PauseToken pauseToken;

        /// <summary>
        /// Raised when a goal is completed.
        /// </summary>
        public static event EventHandler<ImperativeEventArgs> OnImperativeCompleted;

        /// <summary>
        /// Gets a list of the current active goals.
        /// </summary>
        public static IEnumerable<IImperative> CurrentImperatives
        {
            get
            {
                return ActiveImperatives;
            }
        }

        public static IEnumerable<IReaction> CurrentReactions
        {
            get
            {
                return ImpulsesByReaction.Keys;
            }
        }

        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            ImmediateSynchronizationContext.Run(() =>
            {
                // Capture the loop in the sync context.
                InvokeImpulsesLoop();
            });
        }

        /// <summary>
        /// Adds an imperative to manage.
        /// </summary>
        /// <param name="imperative">The imperative to add.</param>
        public static void AddImperative(IImperative imperative)
        {
            if (imperative == null)
            {
                throw new ArgumentNullException(nameof(imperative));
            }

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
            foreach (var reaction in ActiveReactionsByImperative[imperative])
            {
                ImpulsesByReaction.Remove(reaction);
                reaction.Completed -= HandleReactionCompleted;
                reaction.Abort();
            }

            ActiveImperatives.Remove(imperative);
            ActiveReactionsByImperative.Remove(imperative);
        }

        /// <summary>
        /// Clears all goals and resets the goal driver.
        /// </summary>
        public static void Reset()
        {
            foreach (var execution in ActiveReactionsByImperative.Values.SelectMany(r => r).ToArray())
            {
                execution.Completed -= HandleReactionCompleted;
                execution.Abort();
            }

            RunningImpulses.Clear();
            ImpulsesByReaction.Clear();
            ActiveImperatives.Clear();
            ActiveReactionsByImperative.Clear();
        }

        public static string DumpStatus()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Imperatives");
            foreach (var imperative in ActiveImperatives)
            {
                DumpImperative(imperative, sb, 1);
            }

            sb.AppendLine("Available Impulses");
            foreach (var imperative in ActiveImperatives)
            {
                var impulses = from pair in imperative.GetImpulses(GameStateProvider.Current).Select((r, i) => new { Reaction = r, Index = i })
                               orderby pair.Reaction.Priority descending, pair.Index ascending
                               select pair.Reaction;
                foreach (var reaction in impulses)
                {
                    sb.AppendFormat("- Impulse: {0}\n", reaction.ToString());
                    sb.AppendFormat("- - Priority: {0}\n", reaction.Priority);
                }
            }

            return sb.ToString();
        }

        private static void DumpImperative(IImperative imperative, StringBuilder sb, int depth)
        {
            var prefix = string.Join(" ", Enumerable.Repeat("-", depth));

            sb.AppendFormat("{0} Imperative: {1}\n", prefix, imperative.ToString());

            var canActivate = ConditionResult.Trace(() => imperative.IsConditionMet(GameStateProvider.Current));
            sb.AppendFormat("{0} - Condition Met: {1}\n", prefix, canActivate.IsConditionMet);
            sb.AppendFormat("{0} - - Reason: {1}\n", prefix, canActivate.ToString());

            var isSatisfied = ConditionResult.Trace(() => imperative.IsSatisfied(GameStateProvider.Current));
            sb.AppendFormat("{0} - Is Satisfied: {1}\n", prefix, isSatisfied.IsConditionMet);
            sb.AppendFormat("{0} - - Reason: {1}\n", prefix, isSatisfied.ToString());

            sb.AppendFormat("{0} - Impulses:\n", prefix);
            foreach (var impulse in imperative.GetImpulses(GameStateProvider.Current))
            {
                sb.AppendFormat("{0} - - Impulse: {1}\n", prefix, impulse.ToString());
            }

            if (imperative.Children.Count > 0)
            {
                sb.AppendFormat("{0} - Children:\n", prefix);
                foreach (var child in imperative.Children)
                {
                    DumpImperative(child, sb, depth + 2);
                }
            }
        }

        private static async void InvokeImpulsesLoop()
        {
            var lastHash = 0;

            while (true)
            {
                if (!GameAPI.IsRunning || !MechanicalHeart.IsRunning)
                {
                    TryIdle();

                    // Reset our loop state, just in case the new board state is identical, as it may be for rapid
                    // restarts of the same legacy.
                    lastHash = 0;

                    // Wait until it is time to run.
                    await MechanicalHeart.AwaitStart(CancellationToken.None);
                    continue;
                }
                else
                {
                    // Always wait between impulses.
                    // We used to drain all impulses in one frame, but that opens the door to infinite loops
                    // with zero-time reactions (such as memory set).
                    // TODO: Detect runaway every-beat impulses and log a warning.
                    await MechanicalHeart.AwaitBeat(CancellationToken.None);
                }

                var currentHash = GameStateProvider.Current.GetHashCode();
                if (!isActive && currentHash == lastHash)
                {
                    // Not doing anything and nothing has changed, continue.
                    // This is much more effective than shoving a cache in all the impulses.
                    // Note: Funnily enough, this makes the GetFirstReadyImpulse performance monitor show a higher average time since
                    // its called less but doing more work when it is called.
                    continue;
                }

                lastHash = currentHash;
                TryCompleteImperatives();

                await InvokeNextImpulse();
            }
        }

        private static async Task InvokeNextImpulse()
        {
            try
            {
                // Note that this task is synchronous.
                // We still coordinate as we do not want to start a new impulse until all ops are idle, in case
                // we start an impulse that wants a card that is going to be used in another already running impulse.
                await Cerebellum.Coordinate(
                    (cancellationToken) =>
                    {
                        EnumeratedImpulse chosenImpulse = null;
                        try
                        {
                            // Scan through all possible reactions and invoke the highest priority one that can start
                            chosenImpulse = PerfMonitor.Monitor($"GetFirstReadyImpulse", () => GetReadyImpulses().FirstOrDefault());
                        }
                        catch (Exception ex)
                        {
                            Autoccultist.LogWarn(ex, "NucleusAccumbens failed when finding an impulse to execute.");
                        }

                        if (chosenImpulse == null)
                        {
                            TryIdle();
                            return Task.CompletedTask;
                        }

                        var shouldPause = false;
                        try
                        {
                            // Note: at one point, we awaited the start of every impulse.
                            // That is no longer required, as if this impulse wants to coordinate, it will schedule with the Cerebellum
                            // and our next attempt at starting impulses will wait for it to complete.
                            shouldPause = StartImulse(chosenImpulse.Imperative, chosenImpulse.Impulse);
                        }
                        catch (Exception ex)
                        {
                            Autoccultist.LogWarn(ex, "NucleusAccumbens failed when starting an impulse.");
                        }

                        if (shouldPause)
                        {
                            TryActive();
                        }

                        return Task.CompletedTask;
                    },
                    CancellationToken.None);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static void TryActive()
        {
            if (isActive)
            {
                return;
            }

            isActive = true;
            if (pauseToken == null)
            {
                pauseToken = GameAPI.Pause();
            }
        }

        private static void TryIdle()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;
            if (pauseToken != null)
            {
                pauseToken.Dispose();
                pauseToken = null;
            }

            if (AutoccultistSettings.SortTableOnIdle)
            {
                GameAPI.SortTable();
            }
        }

        /// <summary>
        /// Gets all reactions that are ready to be invoked, in order of priority.
        /// </summary>
        private static IEnumerable<EnumeratedImpulse> GetReadyImpulses()
        {
            return PerfMonitor.Monitor(
                nameof(GetReadyImpulses),
                () =>
                (from imperative in ActiveImperatives
                 from impulse in imperative.GetImpulses(GameStateProvider.Current).Distinct()
                 orderby impulse.Priority descending
                 select new EnumeratedImpulse { Imperative = imperative, Impulse = impulse }).ToArray());
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
            if (ActiveReactionsByImperative.TryGetValue(imperative, out var reactions))
            {
                foreach (var reaction in reactions.ToArray())
                {
                    reaction.Completed -= HandleReactionCompleted;
                    reaction.Abort();

                    if (ImpulsesByReaction.TryGetValue(reaction, out var impulse))
                    {
                        RunningImpulses.Remove(impulse);
                    }

                    ImpulsesByReaction.Remove(reaction);
                }

                ActiveReactionsByImperative.Remove(imperative);
            }

            if (!ActiveImperatives.Remove(imperative))
            {
                return;
            }

            OnImperativeCompleted?.Invoke(null, new ImperativeEventArgs(imperative));
        }

        /// <summary>
        /// Try to start an impulse
        /// </summary>
        /// <returns>true if the impulse starts and is ongoing, false if it did not start or completed synchronously.</returns>
        private static bool StartImulse(IImperative imperative, IImpulse impulse)
        {
            if (!RunningImpulses.Add(impulse))
            {
                return false;
            }

            var reaction = impulse.GetReaction();

            // FIXME: We keep so many variations of the same imperative => reaction => execution mapping.  Clean this up
            ActiveReactionsByImperative[imperative].Add(reaction);
            ImpulsesByReaction.Add(reaction, impulse);

            reaction.Completed += HandleReactionCompleted;

            reaction.Start();

            // The impulse might complete synchronously, so return false if it did.
            return RunningImpulses.Contains(impulse);
        }

        private static void HandleReactionCompleted(object sender, EventArgs e)
        {
            var reaction = (IReaction)sender;
            reaction.Completed -= HandleReactionCompleted;

            // FIXME: More disgusting tracking maps.
            if (ImpulsesByReaction.TryGetValue(reaction, out var impulse))
            {
                RunningImpulses.Remove(impulse);
            }
            else
            {
                Autoccultist.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find reaction for execution {reaction}");
            }

            ImpulsesByReaction.Remove(reaction);

            // Shame we don't know what imperative this was part of, but we shouldn't have very many parallel imperatives
            foreach (var executions in ActiveReactionsByImperative.Values)
            {
                if (executions.Remove(reaction))
                {
                    return;
                }
            }

            Autoccultist.LogWarn($"NucleusAccumbens.HandleReactionCompleted: Could not find imperative for execution {reaction}");
        }

        private class EnumeratedImpulse
        {
            public IImperative Imperative { get; set; }

            public IImpulse Impulse { get; set; }

            public int Index { get; set; }
        }
    }
}
