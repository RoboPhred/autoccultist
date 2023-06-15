namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    public abstract class ConditionConfig : IGameStateConditionConfig, IAfterYamlDeserialization
    {
        public string Name { get; set; }

        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            return this.IsConditionMet(state);
        }

        public abstract ConditionResult IsConditionMet(IGameState state);

        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            this.OnAfterDeserialized(start, end);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}(Name = \"{this.Name}\")";
        }

        protected virtual void OnAfterDeserialized(Mark start, Mark end)
        {
        }
    }
}