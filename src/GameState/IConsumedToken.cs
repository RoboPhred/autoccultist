using Assets.CS.TabletopUI;

namespace Autoccultist.GameState
{
    public interface IConsumedToken
    {
        IGameItemState TokenState { get; }

        // TODO: What if the player uses a card before our consumer makes use of it?
        //  We need to detect this and throw an exception.
        DraggableToken GetToken();

        bool IsReleased { get; }
        void Release();
    }
}