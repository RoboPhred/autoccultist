namespace AutoccultistNS.Brain
{
    public class OperationEventArgs
    {
        public OperationEventArgs(IOperation operation)
        {
            this.Operation = operation;
        }

        public IOperation Operation { get; }
    }
}
