namespace AutoccultistNS.Brain
{
    /// <summary>
    /// A recipe solution that is only valid if a condition is met.
    /// </summary>
    public interface IConditionalRecipeSolution : IRecipeSolution, IGameStateCondition
    {
    }
}
