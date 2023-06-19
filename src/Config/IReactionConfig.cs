using AutoccultistNS.Yaml;

namespace AutoccultistNS.Config
{
    [DuckTypeCandidate(typeof(OperationConfig))]
    [DuckTypeCandidate(typeof(ImpulseConfig))]
    public interface IReactionConfig : IConfigObject { }
}