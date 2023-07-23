namespace AutoccultistNS.Brain
{
    using AutoccultistNS.GameState;

    /// <summary>
    /// A set of instructions to operate a situation from start to completion.
    /// </summary>
    public interface IOperation : INamedObject, IIdObject
    {
        /// <summary>
        /// Gets the situation id to target for this operation.
        /// </summary>
        string Situation { get; }

        /// <summary>
        /// Gets the recipe solution for the current situation state.
        /// </summary>
        IRecipeSolution GetRecipeSolution(ISituationState situationState, IGameState gameState = null);

        string DebugRecipes(ISituationState situationState, IGameState gameState = null);
    }
}
