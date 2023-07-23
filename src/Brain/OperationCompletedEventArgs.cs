namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    public class OperationCompletedEventArgs : OperationEventArgs
    {
        public OperationCompletedEventArgs(IOperation operation, IReadOnlyCollection<ICardState> outputCards)
            : base(operation)
        {
            this.OutputCards = outputCards;
        }

        public IReadOnlyCollection<ICardState> OutputCards { get; }
    }
}
