namespace AutoccultistNS.Brain
{
    public interface IReactor
    {
        /// <summary>
        /// Performs the reaction.
        /// </summary>
        IReaction GetReaction();
    }
}
