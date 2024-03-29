namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    public class AspectValueCondition : ConditionConfig
    {
        public List<CardChooserConfig> FromEach { get; set; } = new();

        public List<CardChooserConfig> FromAll { get; set; } = new();

        public string Aspect { get; set; }

        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
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

            if (this.Value.IsConditionMet(aspectValue, state))
            {
                return ConditionResult.Success;
            }
            else
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"Matched cards did not have aspect {this.Aspect} of value {this.Value}");
            }
        }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.FromEach == null)
            {
                this.FromEach = new();
            }

            if (this.FromAll == null)
            {
                this.FromAll = new();
            }

            var hasFromEach = this.FromEach.Count > 0 ? 1 : 0;
            var hasFromAll = this.FromAll.Count > 0 ? 1 : 0;

            if (hasFromEach + hasFromAll != 1)
            {
                throw new SemanticErrorException(start, end, "AspectValueCondition must have exactly one of 'fromEach', or 'fromAll'.");
            }

            if (string.IsNullOrEmpty(this.Aspect))
            {
                throw new SemanticErrorException(start, end, "AspectValueCondition must have an 'aspect'.");
            }

            if (this.Value == null)
            {
                throw new SemanticErrorException(start, end, "AspectValueCondition must have a 'value'.");
            }
        }
    }
}
