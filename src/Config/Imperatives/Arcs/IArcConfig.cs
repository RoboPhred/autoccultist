namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [LibraryConfigObject("arcs")]
    [DuckTypeCandidate(typeof(MotivationalArcConfig))]
    public interface IArcConfig : INamedConfigObject, IArc
    {
    }
}
