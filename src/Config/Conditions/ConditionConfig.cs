namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    public abstract class ConditionConfig : ConfigObject, IGameStateConditionConfig
    {
        public string Name { get; set; }

        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            return this.IsConditionMet(state);
        }

        public abstract ConditionResult IsConditionMet(IGameState state);

        public override string ToString()
        {
            return $"{this.GetType().Name}(Name = \"{this.Name}\")";
        }
    }
}
