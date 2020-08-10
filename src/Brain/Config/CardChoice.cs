using System.Collections.Generic;
using Assets.Core.Interfaces;

namespace Autoccultist.Brain.Config
{
    public class CardChoice : ICardMatcher
    {
        public string ElementId { get; set; }

        public Dictionary<string, int> Aspects { get; set; }

        public bool CardMatches(IElementStack card)
        {
            if (this.ElementId != null && card.EntityId != this.ElementId)
            {
                return false;
            }

            if (this.Aspects != null)
            {
                var cardAspects = card.GetAspects();
                foreach (var aspectPair in Aspects)
                {
                    int cardAspect;
                    if (!cardAspects.TryGetValue(aspectPair.Key, out cardAspect))
                    {
                        return false;
                    }

                    // TODO: For now, just looking for aspects that have at least that amount.
                    //  We should be choosing the least matching card of all possible cards, to
                    //  leave higher aspect cards for other usages.
                    // May want to return a match weight where lower values get chosen over higher values
                    if (cardAspect < aspectPair.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(new[] { this });
        }

        public override string ToString()
        {
            return string.Format("[cardMatch ElementId = {0}]", this.ElementId);
        }
    }
}