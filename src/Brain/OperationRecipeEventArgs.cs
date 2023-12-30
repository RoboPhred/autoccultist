namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    public class OperationRecipeEventArgs : OperationEventArgs
    {
        public OperationRecipeEventArgs(IOperation operation, string recipeName, IReadOnlyDictionary<string, ICardState> slottedCards)
            : base(operation)
        {
            this.RecipeName = recipeName;
            this.SlottedCards = slottedCards;
        }

        public string RecipeName { get; }

        public IReadOnlyDictionary<string, ICardState> SlottedCards { get; }
    }
}
