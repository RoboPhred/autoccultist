namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(MotivationalArcConfig))]
    public interface IArcConfig : IImperativeConfig, IArc
    {
    }
}
