namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    public static class SuperEgo
    {
        static SuperEgo()
        {
            Ego.OnMotivationCompleted += HandleMotivationComplete;
        }

        public static Queue<IMotivation> Motivations { get; } = new();

        public static IMotivation CurrentMotivation { get; private set; }

        public static void SetMotivations(IEnumerable<IMotivation> motivations)
        {
            Clear();

            foreach (var motivation in motivations)
            {
                Motivations.Enqueue(motivation);
            }

            TryNextMotivation();
        }

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
            AutoccultistPlugin.Instance.LogTrace($"SuperEgo: Transitioning to motivation \"{CurrentMotivation.Name}\".");
            Ego.SetMotivation(CurrentMotivation);
        }

        private static void HandleMotivationComplete(object sender, MotivationCompletedEventArgs e)
        {
            if (e.CompletedMotivation == CurrentMotivation)
            {
                AutoccultistPlugin.Instance.LogTrace($"SuperEgo: Motivation \"{CurrentMotivation.Name}\" complete.");
                CurrentMotivation = null;
                TryNextMotivation();
            }
            else
            {
                AutoccultistPlugin.Instance.LogTrace($"SuperEgo: Not handling motivation \"{e.CompletedMotivation.Name}\" because it is not the current one.");
            }
        }
    }
}
