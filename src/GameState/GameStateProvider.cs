namespace Autoccultist.GameState
{
    using System;
    using System.Linq;
    using Autoccultist.GameState.Impl;

    /// <summary>
    /// A static class for producing <see cref="IGameState"/> states.
    /// </summary>
    public static class GameStateProvider
    {
        private static Lazy<IGameState> currentState;

        static GameStateProvider()
        {
            Invalidate();
        }

        /// <summary>
        /// Gets the current game state.
        /// </summary>
        public static IGameState Current
        {
            get
            {
                return currentState.Value;
            }
        }

        /// <summary>
        /// Invalidates the current game state.
        /// </summary>
        public static void Invalidate()
        {
            currentState = new Lazy<IGameState>(FromCurrentState);
        }

        /// <summary>
        /// Produce a game state from the current state of the game.
        /// </summary>
        /// <returns>The produced game state representing the current state of the game.</returns>
        private static IGameState FromCurrentState()
        {
            GameStateObject.CurrentStateVersion++;

            var tabletopCards =
                from stack in GameAPI.GetTabletopCards()
                from cardState in CardStateImpl.CardStatesFromStack(stack)
                select cardState;

            var situations =
                from controller in GameAPI.GetAllSituations()
                let state = new SituationStateImpl(controller)
                select state;

            return new GameStateImpl(tabletopCards.ToArray(), situations.ToArray());
        }
    }
}
