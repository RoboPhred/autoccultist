namespace AutoccultistNS.Brain
{
    using System;

    // FIXME: This is a bit of a hack shim between the old orchestration system and the new reaction system.
    public class OperationReaction : IReaction
    {
        private ISituationOrchestration orchestration;

        public OperationReaction(IOperation operation)
        {
            this.Operation = operation;
        }

        public event EventHandler Completed;

        public IOperation Operation { get; }

        public override string ToString()
        {
            return $"OperationReaction:{this.Operation}";
        }

        public void Abort()
        {
            if (this.orchestration == null)
            {
                return;
            }

            this.orchestration.Abort();
        }

        public void Start()
        {
            if (this.orchestration != null)
            {
                throw new InvalidOperationException("Cannot execute an operation reaction more than once.");
            }

            try
            {
                this.orchestration = SituationOrchestrator.RegisterOperation(this.Operation);
                this.orchestration.Completed += (_, __) => this.Completed?.Invoke(this, EventArgs.Empty);
                this.orchestration.Start();
            }
            catch (Exception ex)
            {
                throw new ReactionFailedException(ex.Message, ex);
            }
        }
    }
}
