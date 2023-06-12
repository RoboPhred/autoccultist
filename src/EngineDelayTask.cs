namespace AutoccultistNS
{
    using System;
    using System.Threading;

    public class EngineDelayTask : AsyncUpdateTask<bool>
    {
        private readonly TimeSpan delay;
        private readonly DateTime start = DateTime.Now;

        public EngineDelayTask(TimeSpan delay, CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            this.delay = delay;
        }

        protected override void Update()
        {
            if (this.start + this.delay < DateTime.Now)
            {
                this.SetComplete(true);
            }
        }
    }
}
