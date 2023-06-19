namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(OperationConfig))]
    [DuckTypeCandidate(typeof(ImpulseConfig))]
    public interface IReactionConfig : IConfigObject, IReaction
    {
    }
}
