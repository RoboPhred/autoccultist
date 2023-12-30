namespace AutoccultistNS.Config.Values
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;

    public class AspectCountValueProviderConfig : ValueProviderObjectConfig
    {
        public List<CardChooserConfig> FromEach { get; set; } = new();

        public List<CardChooserConfig> FromAll { get; set; } = new();

        public string Aspect { get; set; }

        public override float GetValue(IGameState state)
        {
            var cards = Enumerable.Empty<ICardState>();

            if (this.FromAll.Count > 0)
            {
                // Do not bother sorting the cards as we are going to process all of them.
                cards = this.FromAll.SelectMany(c => c.SelectChoices(state.AllCards, state, CardChooserHints.IgnorePriority));
            }
            else if (this.FromEach.Count > 0)
            {
                // Still need to sort these cards, as we want the highest priority of each.
                cards = this.FromEach.Select(c => c.SelectChoices(state.AllCards, state).FirstOrDefault()).Where(x => x != null);
            }

            int aspectValue = 0;
            if (this.Aspect == "@count")
            {
                aspectValue = cards.Count();
            }
            else
            {
                foreach (var card in cards)
                {
                    card.Aspects.TryGetValue(this.Aspect, out var value);
                    aspectValue += value;
                }
            }

            return aspectValue;
        }
    }
}
