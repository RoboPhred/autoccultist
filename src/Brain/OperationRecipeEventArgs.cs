namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;

    public class OperationRecipeEventArgs : OperationEventArgs
    {
        public OperationRecipeEventArgs(IOperation operation, IReadOnlyDictionary<string, string> slottedElements)
            : base(operation)
        {
        }
    }
}
