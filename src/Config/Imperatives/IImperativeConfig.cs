namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(GoalConfig))]
    [DuckTypeCandidate(typeof(NestedImperativeConfig))]
    [DuckTypeCandidate(typeof(OperationImperativeImpulseConfig))]
    public interface IImperativeConfig : IConfigObject, IImperative
    {
    }
}
