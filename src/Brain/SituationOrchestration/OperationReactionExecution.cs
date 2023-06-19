using System;
using System.Threading.Tasks;

namespace AutoccultistNS.Brain
{
    // FIXME: This is a bit of a hack shim between the old orchestration system and the new reaction system.
    public class OperationReactionExecution : IReactionExecution
    {
        private readonly ISituationOrchestration orchestration;
        private readonly Task startTask;

        public OperationReactionExecution(IOperation operation)
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

        public IOperation Operation { get; }

        public event EventHandler Completed;

        public override string ToString()
        {
            return $"OperationReactionExecution:{this.Operation}";
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