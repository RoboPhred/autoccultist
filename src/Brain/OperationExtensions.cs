namespace AutoccultistNS.Brain
{
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    public static class OperationExtensions
    {
        /// <summary>
        /// Gets the recipe solution to use for the current ongoing recipe of this operation.
        /// </summary>
        /// <param name="operation">The operation to get the recipe solution for.</param>
        /// <param name="situationState">The current state of the situation.</param>
        /// <param name="gameState">The current game state.  If none is specified, the current game state will be used.</param>
        /// <returns>The recipe solution to use for the current ongoing recipe of this operation.</returns>
        public static IRecipeSolution GetOngoingRecipeSolution(this IOperation operation, ISituationState situationState, IGameState gameState = null)
        {
            if (situationState.State != StateEnum.Ongoing)
            {
                return null;
            }

            if (operation.OngoingRecipes != null && operation.OngoingRecipes.TryGetValue(situationState.CurrentRecipe, out var recipe))
            {
                return recipe;
            }

            if (operation.ConditionalOngoingRecipes != null)
            {
                return operation.ConditionalOngoingRecipes.FirstOrDefault(x => x.IsConditionMet(gameState));
            }

            return null;
        }
    }
}
