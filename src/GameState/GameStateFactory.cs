namespace Autoccultist.GameState
{
    public static class GameStateFactory
    {
        public static IGameState FromCurentState()
        {
            // Game state should be derived from the game every frame,
            //  but consumed or reserved cards need to persist between frames
        }
    }
}