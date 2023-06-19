namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(LinearMotivationalArcConfig))]
    public interface IArcConfig : INamedConfigObject, IArc
    {
    }
}
