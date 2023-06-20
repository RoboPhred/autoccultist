namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(GoalConfig))]
    public interface IImperativeConfig : IConfigObject, IImperative
    {
    }
}
