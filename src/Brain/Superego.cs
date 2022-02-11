namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// Class responsible for running through the motivations.
    /// </summary>
    public static class Superego
    {
        static Superego()
        {
            Ego.OnMotivationCompleted += HandleMotivationComplete;
        }

        /// <summary>
        /// Gets the arc the superego is currently running.
        /// </summary>
        public static IArc CurrentArc { get; private set; }

        /// <summary>
        /// Gets the current motivation being tracked by the superego.
        /// </summary>
        public static IMotivation CurrentMotivation { get; private set; }

        private static Queue<IMotivation> Motivations { get; } = new();

        /// <summary>
        /// Set the driving arc for the superego.
        /// </summary>
        /// <param name="arc">The arc to drive the superego.</param>
        public static void SetArc(IArc arc)
        {
            Clear();

            CurrentArc = arc;

            if (arc == null)
            {
                return;
            }

            foreach (var motivation in arc.Motivations)
            {
                Motivations.Enqueue(motivation);
            }

            TryNextMotivation();
        }

        /// <summary>
        /// Skips the current motivation.
        /// </summary>
        public static void SkipCurrentMotivation()
        {
            if (CurrentMotivation != null)
            {
                Ego.SetMotivation(null);
            }

            TryNextMotivation();
        }

        /// <summary>
        /// Clears out all motivations.
        /// </summary>
        public static void Clear()
        {
            Ego.SetMotivation(null);
            CurrentArc = null;
            CurrentMotivation = null;
            Motivations.Clear();
        }

        private static void TryNextMotivation()
        {
            if (Motivations.Count == 0)
            {
                AutoccultistPlugin.Instance.LogTrace("SuperEgo: No more motivations to run.");
                return;
            }

            CurrentMotivation = Motivations.Dequeue();
            Ego.SetMotivation(CurrentMotivation);
        }

        private static void HandleMotivationComplete(object sender, MotivationCompletedEventArgs e)
        {
            if (e.CompletedMotivation != CurrentMotivation)
            {
                return;
            }

            CurrentMotivation = null;
            TryNextMotivation();
        }
    }
}
