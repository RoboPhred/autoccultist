namespace Autoccultist.Brain.Config
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A condition dealing with matching the state of situations.
    /// </summary>
    public class SituationCondition : IGameStateConditionConfig
    {
        /// <summary>
        /// Gets or sets the situation id for which all the other condition properties will target.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the state the situation is required to be in.
        /// If null, the state will not be checked.
        /// </summary>
        public SituationStateConfig? State { get; set; }

        /// <summary>
        /// Gets or sets the recipe the situation is required to be processing.
        /// If null, this condition will not be checked.
        /// </summary>
        public string Recipe { get; set; }

        /// <summary>
        /// Gets or sets a time comparison to the time remaining for the situation's current recipe.
        /// If null, this condition will not be checked.
        /// </summary>
        public TimeComparison TimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the card condition matching cards stored inside this condition, excluding slotted cards.
        /// If null, this condition will not be checked.
        /// </summary>
        public CardSetCondition StoredCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets the card set condition matching cards slotted into the current ongoing slots of the situation.
        /// If null, this condition will not be checked.
        /// </summary>
        public CardSetCondition SlottedCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets the card set condition matching all cards in play by this situation, including slotted and stored cards.
        /// If null, this condition will not be checked.
        /// </summary>
        public CardSetCondition ContainedCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets a dictionary containing asset ids to degrees that must be satisfied in this situation.
        /// Aspects are taken from all contained cards, both slotted and stored.
        /// If null, this condition will not be checked.
        /// </summary>
        public Dictionary<string, int> ContainedAspects { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException("SituationCondition must have a situationId.");
            }

            this.TimeRemaining?.Validate();
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            // TODO: Using GameAPI...  Probably should get this data from IGameState
            var situation = GameAPI.GetSituation(this.Situation);
            if (situation == null)
            {
                return this.State == SituationStateConfig.Missing;
            }

            if (this.Recipe != null && situation.SituationClock.RecipeId != this.Recipe)
            {
                return false;
            }

            if (this.TimeRemaining?.IsComparisonTrue(situation.SituationClock.TimeRemaining) == false)
            {
                return false;
            }

            switch (this.State)
            {
                case SituationStateConfig.Ongoing:
                    if (!situation.IsOngoing)
                    {
                        return false;
                    }

                    break;
                case SituationStateConfig.Unstarted:
                    if (situation.SituationClock.State != SituationState.Unstarted)
                    {
                        return false;
                    }

                    break;
            }

            if (this.StoredCardsMatch != null)
            {
                var cards = situation.GetStoredStacks().ToList();
                if (!this.StoredCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.SlottedCardsMatch != null)
            {
                var cards = situation.GetOngoingStacks().ToList();
                if (!this.SlottedCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.ContainedCardsMatch != null)
            {
                var cards = situation.GetStoredStacks().Concat(situation.GetOngoingStacks()).ToList();
                if (!this.ContainedCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.ContainedAspects != null)
            {
                var aspects = situation.situationWindow.GetAspectsFromAllSlottedAndStoredElements(true);
                foreach (var entry in this.ContainedAspects)
                {
                    if (!aspects.TryGetValue(entry.Key, out var value) || value < entry.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
