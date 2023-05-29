namespace AutoccultistNS.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// A solution to a situation recipe.
    /// </summary>
    public interface IRecipeSolution
    {
        /// <summary>
        /// Gets a dictionary of slot names to card choices.
        /// </summary>
        IReadOnlyDictionary<string, ICardChooser> SlotSolutions { get; }

        /// <summary>
        /// Gets the solution for the mansus choice of this recipe, if any.
        /// </summary>
        IMansusSolution MansusChoice { get; }

        /// <summary>
        /// Gets a collection of card choices that must all be satisfied for this recipe solution to start.
        /// </summary>
        /// <returns>A collection of card choices that must be satisified to start this recipe solution.</returns>
        IEnumerable<ICardChooser> GetRequiredCards();
    }
}
