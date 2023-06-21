namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    // We cannot allow OperationConfig as a reactor here until followups are capable of injecting themselves into the NucleusAccumbens queue.
    // Otherwise, their async start will happen simultaniously with other impulses.
    // [DuckTypeCandidate(typeof(OperationConfig))]
    [DuckTypeCandidate(typeof(RememberConfig))]
    public interface IReactorConfig : INamedConfigObject, IReactor
    {
    }
}
