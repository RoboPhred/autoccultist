namespace AutoccultistNS.Tasks
{
    using System;
    using System.Threading;

    public class AwaitConditionTask : GlobalUpdateTask<bool>
    {
        private readonly Func<bool> condition;

        public AwaitConditionTask(Func<bool> condition, CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            this.condition = condition;
            GameEventSource.GameEnded += this.HandleGameEnded;
        }

        protected override void Update()
        {
            if (this.condition())
            {
                GameEventSource.GameEnded -= this.HandleGameEnded;
                this.SetResult(true);
            }
        }

        private void HandleGameEnded(object sender, EventArgs e)
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
            this.SetException(new Exception("Game ended while awaiting GameAPI condition."));
        }
    }
}
