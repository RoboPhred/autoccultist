namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// A set of instructions to operate a situation from start to completion.
    /// </summary>
    public interface IOperation : IGameStateCondition
    {
        /// <summary>
        /// Gets the human-friendly display name of this goal.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the situation id to target for this operation.
        /// </summary>
        string Situation { get; }

        /// <summary>
        /// Gets the recipe used to start this situation.
        /// </summary>
        IRecipeSolution StartingRecipe { get; }

        /// <summary>
        /// Gets a dictionary of recipe ids to recipe solutions for each ongoing recipe the situation may encounter.
        /// </summary>
        IReadOnlyDictionary<string, IRecipeSolution> OngoingRecipes { get; }
    }
}
