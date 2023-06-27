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
            InvokeImpulsesLoop();
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

                var canActivate = ConditionResult.Trace(() => imperative.CanActivate(GameStateProvider.Current));
                sb.AppendFormat("- Can Activate: {0}\n", canActivate.IsConditionMet);
                if (!canActivate)
                {
                    sb.AppendFormat("- - Reason: {0}\n", canActivate.ToString());
                }

                var isSatisfied = ConditionResult.Trace(() => imperative.IsSatisfied(GameStateProvider.Current));
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
                    var isConditionMet = ConditionResult.Trace(() => reaction.IsConditionMet(GameStateProvider.Current));
                    sb.AppendFormat("- - - Is Condition Met: {0}\n", isConditionMet.IsConditionMet);
                    if (!isConditionMet)
                    {
                        sb.AppendFormat("- - - - Reason: {0}\n", isConditionMet.ToString());
                    }
                }
            }

            return sb.ToString();
        }

        private static async void InvokeImpulsesLoop()
        {
            while (true)
            {
                // Note: We do not have to await beats or check if the bot is running as Cerebellum does that.
                await Cerebellum.Coordinate(
                    (cancellationToken) =>
                    {
                        if (!GameAPI.IsRunning)
                        {
                            return Task.CompletedTask;
                        }

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
                            OnIdle();
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
                            OnActive();
                        }

                        return Task.CompletedTask;
                    },
                    CancellationToken.None);

                if (!isActive)
                {
                    // Not doing anything, wait a frame
                    // Note: We used to not have this, which surely shoudl have meant an infinite loop deadlock.
                    // This implies that unity is / was scheduling our continuations on the next frame.
                    // We are now trying to eliminate that next-frame behavior, so this is important.
                    await MechanicalHeart.AwaitBeat(CancellationToken.None);
                }
            }
        }

        private static void OnActive()
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

        private static void OnIdle()
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
            return from reaction in GetAllImpulses()
                   where !RunningImpulses.Contains(reaction.Impulse)
                   where reaction.Impulse.IsConditionMet(GameStateProvider.Current)
                   select reaction;
        }

        /// <summary>
        /// Gets all reactions that are currently active, in order of priority.
        /// </summary>
        private static IEnumerable<EnumeratedImpulse> GetAllImpulses()
        {
            // Note: Imperatives come in an indeterminate order due to the HashSet... We should use a consistant order here.
            return
                from imperative in ActiveImperatives
                from reaction in imperative.GetImpulses(GameStateProvider.Current).Distinct().Select((r, index) => new { Value = r, Index = index })
                orderby reaction.Value.Priority descending, reaction.Index ascending
                select new EnumeratedImpulse { Imperative = imperative, Impulse = reaction.Value };
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
