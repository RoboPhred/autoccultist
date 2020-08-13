using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using Autoccultist.GameState;

namespace Autoccultist.Brain.Config
{
    public class CardChoice
    {
        public string ElementId { get; set; }

        public Dictionary<string, int> Aspects { get; set; }

        public bool TryConsume(IGameState state, out ElementStackToken token)
        {
            var candidates =
                from card in state.GetTableCards()
                where this.ElementId == null || this.ElementId == this.ElementId
                select card;

            if (this.Aspects != null)
            {
                candidates =
                    from card in candidates
                    where card.Aspects.MatchesAspects(this.Aspects)
                    // Weight of this card is how many aspects remain unused if we choose it
                    let weight = card.Aspects.GetAspectWeight() - this.Aspects.GetAspectWeight()
                    orderby weight ascending
                    select card;
            }

            var target = candidates.FirstOrDefault();
            if (target == null)
            {
                token = null;
                return false;
            }

            token = target.Consume();
            return token != null;
        }

        public override string ToString()
        {
            return string.Format("[cardMatch ElementId = {0}]", this.ElementId);
        }
    }
}