namespace AutoccultistNS.Brain
{
    /// <summary>
    /// An impulse is a desired action by the bot.
    /// Impulses have priority, and higher priority impulses will be executed before lower priority impulses.
    /// </summary>
    public interface IImpulse : IReactor
    {
        /// <summary>
        /// Gets the priority of this impulse.
        /// </summary>
        TaskPriority Priority { get; }
    }
}
