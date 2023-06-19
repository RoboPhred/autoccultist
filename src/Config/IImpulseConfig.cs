namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(OperationConfig))]
    [DuckTypeCandidate(typeof(LegacyImpulseConfig))]
    public interface IImpulseConfig : IConfigObject, IImpulse
    {
    }
}
