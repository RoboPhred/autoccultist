using System.Collections.Generic;
using System.Linq;
using Autoccultist.src.Brain.Util;

namespace Autoccultist.Brain.Config
{
    public class SituationCondition : IGameStateConditionConfig, IBaseCondition
    {
        public string SituationId { get; set; }


        public SituationStateConfig State { get; set; }

        public string Recipe { get; set; }
        public TimeComparison TimeRemaining { get; set; }

        // It would be nice if this could be IGameStateCondition, but 
        //  it would be an error for it to contain anything other than CardSetCondition and CardChoice objects.
        public CardSetCondition StoredCardsMatch;

        public Dictionary<string, int> StoredAspects;

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.SituationId))
            {
                throw new InvalidConfigException("SituationCondition must have a situationId.");
            }

            if (this.TimeRemaining != null)
            {
                this.TimeRemaining.Validate();
            }
        }

        public bool IsConditionMet(IGameState state)
        {
            // TODO: Using GameAPI...  Probably should get this data from IGameState
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                return this.State == SituationStateConfig.Missing;
            }

            if (this.Recipe != null && situation.SituationClock.RecipeId != this.Recipe)
            {
                return false;
            }

            if (this.TimeRemaining != null && !this.TimeRemaining.IsMatch(situation.SituationClock.TimeRemaining))
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
                foreach (var choice in this.StoredCardsMatch)
                {
                    var match = cards.FirstOrDefault(card => choice.CardMatches(card));
                    if (match == null)
                    {
                        return false;
                    }
                    cards.Remove(match);
                }
            }

            if (this.StoredAspects != null)
            {
                var aspects = situation.situationWindow.GetAspectsFromAllSlottedAndStoredElements(true);
                foreach (var entry in this.StoredAspects)
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