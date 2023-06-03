namespace AutoccultistNS.Actor.Actions
{
    public abstract class ActionBase : IAutoccultistAction
    {
        private static int currentId = 0;
        private bool executed = false;

        public ActionBase()
        {
        }

        public int Id { get; } = currentId++;

        /// <inheritdoc/>
        public abstract void Execute();

        public override string ToString()
        {
            return $"{this.GetType().Name}(Id = {this.Id})";
        }

        protected void VerifyNotExecuted()
        {
            if (this.executed)
            {
                throw new ActionFailureException(this, "This action has already been executed.");
            }
        }
    }
}
