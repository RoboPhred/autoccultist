using System.Collections.Generic;

namespace Autoccultist.Brain.Config.Conditions
{
    public class CardSetCondition : ICardCondition
    {
        public List<CardChoice> CardSet { get; set; } = new List<CardChoice>();

        public void Validate()
        {
            if (this.CardSet == null || this.CardSet.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }

        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.CardSet);
        }

        public IEnumerator<CardChoice> GetEnumerator() => this.CardSet.GetEnumerator();

        public void Add(CardChoice cc) => CardSet.Add(cc);
        public void AddRange(IEnumerable<CardChoice> ecc) => CardSet.AddRange(ecc);
    }
}