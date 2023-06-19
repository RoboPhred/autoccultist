namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [LibraryConfigObject("arcs")]
    [DuckTypeCandidate(typeof(LinearMotivationalArcConfig))]
    public interface IArcConfig : INamedConfigObject, IArc
    {
    }
}
