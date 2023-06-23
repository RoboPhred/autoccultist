namespace AutoccultistNS.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class AwaitConditionTask : GlobalUpdateTask<bool>
    {
        private readonly Func<bool> condition;

        protected AwaitConditionTask(Func<bool> condition, CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            this.condition = condition;
            GameEventSource.GameEnded += this.HandleGameEnded;
        }

        public static Task From(Func<bool> condition, CancellationToken cancellationToken)
        {
            return new AwaitConditionTask(condition, cancellationToken).Task;
        }

        protected override void Update()
        {
            if (this.condition())
            {
                GameEventSource.GameEnded -= this.HandleGameEnded;
                this.SetResult(true);
            }
        }

        protected override void OnDisposed()
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
        }

        private void HandleGameEnded(object sender, EventArgs e)
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
            this.SetException(new Exception("Game ended while awaiting GameAPI condition."));
        }
    }
}
