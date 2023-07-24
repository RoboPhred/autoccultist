namespace AutoccultistNS.Config
{
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(MotivationalImperativeConfig))]
    [DuckTypeCandidate(typeof(GoalConfig))]
    [DuckTypeCandidate(typeof(LeafImperativeConfig))]
    [DuckTypeCandidate(typeof(OperationConfig))]
    public interface IChildImperativeConfig : IImperativeConfig
    {
    }
}
