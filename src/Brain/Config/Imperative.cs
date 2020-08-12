
namespace Autoccultist.Brain.Config
{
    public class Imperative
    {
        public string Name { get; set; }
        public ImperativePriority Priority { get; set; } = ImperativePriority.Maintenance;
        public GameStateCondition RequiredCards { get; set; }
        public GameStateCondition ForbidWhenCardsPresent { get; set; }

        public Operation Operation { get; set; }

        public void Validate()
        {
            if (this.Operation == null)
            {
                throw new InvalidConfigException($"Imperative {this.Name} must have an operation.");
            }

            this.Operation.Validate();
        }

        public bool CanExecute(IGameState state)
        {
            // Optionally check required cards for starting the imperative
            if (this.RequiredCards != null && !this.RequiredCards.IsConditionMet(state))
            {
                return false;
            }

            // Sometimes, we want to stop an imperative if other cards are present.
            if (this.ForbidWhenCardsPresent != null && this.ForbidWhenCardsPresent.IsConditionMet(state))
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