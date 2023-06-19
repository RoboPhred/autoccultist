namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(GoalConfig))]
    [DuckTypeCandidate(typeof(MotivationConfig))]
    public interface IImperativeConfig : IConfigObject, IImperative
    {
    }
}
