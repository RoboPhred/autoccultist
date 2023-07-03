namespace AutoccultistNS.Brain
{
    public class ReactionImpulse : IImpulse
    {
        public ReactionImpulse(TaskPriority priority, IReaction reaction)
        {
            this.Priority = priority;
            this.Reaction = reaction;
        }

        public TaskPriority Priority { get; }

        public IReaction Reaction { get; }

        public IReaction GetReaction()
        {
            return this.Reaction;
        }

        public override string ToString()
        {
            return $"[Priority: {this.Priority}] {this.Reaction}";
        }
    }
}
