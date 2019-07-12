using Assets.Core.Interfaces;

namespace Autoccultist.Brain.Config
{
    public class CardChoice : ICardMatcher
    {
        public string ElementID;

        public bool CardMatches(IElementStack card)
        {
            return card.EntityId == this.ElementID;
        }
    }
}