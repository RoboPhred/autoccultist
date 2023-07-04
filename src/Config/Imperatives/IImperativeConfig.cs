namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(MotivationalImperativeConfig))]
    [DuckTypeCandidate(typeof(GoalConfig))]
    [DuckTypeCandidate(typeof(LeafImperativeConfig))]
    [DuckTypeCandidate(typeof(OperationConfig))]
    public interface IImperativeConfig : IConfigObject, IImperative
    {
    }
}
