namespace AutoccultistNS.Brain
{
    using System;
    using System.Threading.Tasks;

    // FIXME: This is a bit of a hack shim between the old orchestration system and the new reaction system.
    public class OperationReaction : IReaction
    {
        private readonly ISituationOrchestration orchestration;
        private readonly Task startTask;

        public OperationReaction(IOperation operation)
        {
            this.Operation = operation;

            try
            {
                this.orchestration = SituationOrchestrator.RegisterOperation(operation);
                this.orchestration.Completed += (_, __) => this.Completed?.Invoke(this, EventArgs.Empty);

                this.startTask = this.orchestration.Start();
            }
            catch (Exception ex)
            {
                throw new ReactionFailedException(ex.Message, ex);
            }
        }

        public event EventHandler Completed;

        public IOperation Operation { get; }

        public override string ToString()
        {
            return $"OperationReaction:{this.Operation}";
        }

        public void Abort()
        {
            this.orchestration.Abort();
        }

        public Task AwaitStarted()
        {
            return this.startTask;
        }
    }
}
