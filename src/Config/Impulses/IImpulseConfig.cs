namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(OperationImpulseConfig))]
    [DuckTypeCandidate(typeof(ImpulseConfig))]
    public interface IImpulseConfig : IConfigObject, IImpulse
    {
    }
}
