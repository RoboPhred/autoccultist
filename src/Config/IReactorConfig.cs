namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(OperationReactorConfig))]
    [DuckTypeCandidate(typeof(RememberReactorConfig))]
    public interface IReactorConfig : INamedConfigObject, IReactor
    {
    }
}
