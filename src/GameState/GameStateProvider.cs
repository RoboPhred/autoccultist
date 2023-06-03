namespace AutoccultistNS.GameState
{
    using System;
    using System.Linq;
    using AutoccultistNS.GameState.Impl;
    using SecretHistories.Assets.Scripts.Application.UI;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

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

            try
            {
                var tabletopCards =
                    from stack in GameAPI.TabletopSphere.GetElementStacks()
                    from cardState in CardStateImpl.CardStatesFromStack(stack, CardLocation.Tabletop)
                    select cardState;

                // Things whizzing around.
                var enRouteCards =
                    from enroute in GameAPI.GetEnRouteSpheres()
                    from stack in enroute.GetElementStacks()
                    from cardState in CardStateImpl.CardStatesFromStack(stack, CardLocation.EnRoute)
                    select cardState;

                var situations =
                    from situation in GameAPI.GetSituations()
                    let state = new SituationStateImpl(situation)
                    select state;

                return new GameStateImpl(tabletopCards.ToArray(), enRouteCards.ToArray(), situations.ToArray(), PortalStateImpl.FromCurrentState());
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning($"Exception in GameStateProvider.FromCurrentState: {ex.ToString()}");
                NoonUtility.LogException(ex);
                throw;
            }
        }
    }
}
