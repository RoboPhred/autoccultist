namespace AutoccultistNS.Brain
{
    using AutoccultistNS.GameState;

    public class ReactorImpulse : IImpulse
    {
        public ReactorImpulse(TaskPriority priority, IReactor reactor)
        {
            this.Priority = priority;
            this.Reactor = reactor;
        }

        public TaskPriority Priority { get; }

        public IReactor Reactor { get; }

        public ConditionResult IsConditionMet(IGameState game)
        {
            return this.Reactor.IsConditionMet(game);
        }

        public IReaction GetReaction()
        {
            return this.Reactor.GetReaction();
        }

        public override string ToString()
        {
            return $"[Priority: {this.Priority}] {this.Reactor}";
        }
    }
}
