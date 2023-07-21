namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;

    /// <summary>
    /// An imperative that will be satisfied the first time any of its impulses completes a reaction.
    /// </summary>
    public class UITriggeredImperative : IImperative
    {
        private bool isSatisfied;

        private Dictionary<IImpulse, MonitoredImpulse> monitoredImpulses = new();

        public UITriggeredImperative(IImperative imperative)
        {
            this.Imperative = imperative;
        }

        public IImperative Imperative { get; private set; }

        public string Name => $"{this.Imperative.Name} (One Shot)";

        public IReadOnlyCollection<IImperative> Children => new[] { this.Imperative };

        public void Abort()
        {
            foreach (var impulse in this.monitoredImpulses.Values)
            {
                impulse.Completed -= this.OnCompleted;
                impulse.Abort();
            }

            this.monitoredImpulses.Clear();

            this.isSatisfied = true;
        }

        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return this.Imperative.DescribeCurrentGoals(state);
        }

        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            this.monitoredImpulses.Clear();

            var seen = new HashSet<IImpulse>();
            foreach (var impulse in this.Imperative.GetImpulses(state))
            {
                seen.Add(impulse);
                if (!this.monitoredImpulses.TryGetValue(impulse, out var monitored))
                {
                    monitored = new MonitoredImpulse(impulse);
                    monitored.Completed += this.OnCompleted;
                    this.monitoredImpulses.Add(impulse, monitored);
                }

                yield return monitored;
            }

            foreach (var impulse in this.monitoredImpulses.Keys)
            {
                if (!seen.Contains(impulse))
                {
                    this.monitoredImpulses[impulse].Completed -= this.OnCompleted;
                    this.monitoredImpulses[impulse].Abort();
                    this.monitoredImpulses.Remove(impulse);
                }
            }
        }

        public ConditionResult IsConditionMet(IGameState state)
        {
            return this.Imperative.IsConditionMet(state);
        }

        public ConditionResult IsSatisfied(IGameState state)
        {
            return this.isSatisfied ? ConditionResult.Success : ConditionResult.Failure;
        }

        private void OnCompleted(object sender, EventArgs e)
        {
            NoonUtility.Log("A monitored impulse completed");
            this.isSatisfied = true;
        }

        private class MonitoredImpulse : IImpulse
        {
            private readonly IImpulse impulse;
            private IReaction reaction;

            public MonitoredImpulse(IImpulse impulse)
            {
                this.impulse = impulse;
            }

            public event EventHandler Completed;

            public TaskPriority Priority => this.impulse.Priority;

            public void Abort()
            {
                if (this.reaction == null)
                {
                    return;
                }

                this.reaction.Abort();
            }

            public IReaction GetReaction()
            {
                if (this.reaction != null)
                {
                    throw new ReactionFailedException("OneShotImperative tried to react more than once.");
                }

                this.reaction = this.impulse.GetReaction();
                this.reaction.Ended += this.OnCompleted;
                NoonUtility.Log("Got monitored reaction");
                return this.reaction;
            }

            public ConditionResult IsConditionMet(IGameState state)
            {
                if (this.reaction != null)
                {
                    return AddendedConditionResult.Addend(ConditionResult.Failure, "OneShot impulse already fired.");
                }

                return this.impulse.IsConditionMet(state);
            }

            private void OnCompleted(object sender, EventArgs e)
            {
                NoonUtility.Log("Completed monitored reaction");
                this.Completed?.Invoke(this, e);
            }
        }
    }
}
