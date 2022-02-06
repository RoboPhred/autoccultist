namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// Class responsible for running through the motivations.
    /// </summary>
    public static class SuperEgo
    {
        static SuperEgo()
        {
            Ego.OnMotivationCompleted += HandleMotivationComplete;
        }

        /// <summary>
        /// Gets the current motivation.
        /// </summary>
        public static IMotivation CurrentMotivation { get; private set; }

        private static Queue<IMotivation> Motivations { get; } = new();

        /// <summary>
        /// Set the motivation list to operate from.
        /// </summary>
        /// <param name="motivations">An enumerable of motivations to run.</param>
        public static void SetMotivations(IEnumerable<IMotivation> motivations)
        {
            Clear();

            foreach (var motivation in motivations)
            {
                Motivations.Enqueue(motivation);
            }

            TryNextMotivation();
        }

        /// <summary>
        /// Clears out all motivations.
        /// </summary>
        public static void Clear()
        {
            Ego.SetMotivation(null);
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
