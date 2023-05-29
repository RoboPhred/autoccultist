
namespace AutoccultistNS.Actor.Actions
{
    public abstract class ActionBase : IAutoccultistAction
    {
        private static int currentId = 0;

        public int Id { get; } = currentId++;

        private bool executed = false;

        public ActionBase()
        {
        }

        /// <inheritdoc/>
        public abstract void Execute();

        protected void VerifyNotExecuted()
        {
            if (this.executed)
            {
                throw new ActionFailureException(this, "This action has already been executed.");
            }
        }

        override public string ToString()
        {
            return $"ActionBase {Id}";
        }
    }
}