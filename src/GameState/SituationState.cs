namespace Autoccultist.GameState
{
    public class SituationState : ITokenState, ITokenContainer, IHasAspects, IConsumableStateToken
    {
        public string TokenId => throw new System.NotImplementedException();

        public bool IsBusy { get; }
    }
}