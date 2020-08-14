
namespace Autoccultist.Brain.Config
{
    /**
     * An Imperative represents an action that cannot ever be truly satisfied.
     * As long as the requirements of the Imperative allow for its execution, the task should execute.
     */
    public class Imperative
    {
        public string Name { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Maintenance;
        public IGameStateConditionConfig Requirements { get; set; }
        public IGameStateConditionConfig Forbidders { get; set; }

        public Operation Operation { get; set; }

        public void Validate()
        {
            if (this.Operation == null)
            {
                throw new InvalidConfigException($"Imperative {this.Name} must have an operation.");
            }

            if (this.Requirements != null)
            {
                this.Requirements.Validate();
            }

            if (this.Forbidders != null)
            {
                this.Forbidders.Validate();
            }

            this.Operation.Validate();
        }

        public bool CanExecute(IGameState state)
        {
            // Optionally check required cards for starting the imperative
            if (this.Requirements != null && !this.Requirements.IsConditionMet(state))
            {
                return false;
            }

            // Sometimes, we want to stop an imperative if other cards are present.
            if (this.Forbidders != null && this.Forbidders.IsConditionMet(state))
            {
                return false;
            }

            if (!this.Operation.CanExecute(state))
            {
                return false;
            }

            return true;
        }
    }
}