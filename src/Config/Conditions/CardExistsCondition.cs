namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardExistsCondition : CardChooserConfig, ICardConditionConfig
    {
        public string Name { get; set; }

        /// <inheritdoc/>
        public ConditionResult IsConditionMet(IGameState state)
        {
            if (this.ChooseCard(state.GetAllCards()) == null)
            {
                return new CardChoiceNotSatisfiedFailure(this);
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards)
        {
            var card = this.ChooseCard(cards);
            if (card == null)
            {
                return new CardChoiceNotSatisfiedFailure(this);
            }

            return ConditionResult.Success;
        }

        protected override void OnAfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            base.OnAfterDeserialized(start, end);
        }
    }
}
