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

            var hornedAxe = Watchman.Get<HornedAxe>();

            var tabletop = hornedAxe.GetSpheres().OfType<TabletopSphere>().First();
            var tabletopCards =
                from stack in tabletop.GetElementStacks()
                from cardState in CardStateImpl.CardStatesFromStack(stack, CardLocation.Tabletop)
                select cardState;

            var situations =
                from situation in hornedAxe.GetRegisteredSituations()
                let state = new SituationStateImpl(situation)
                select state;

            var numa = Watchman.Get<Numa>();
            var otherworld = Reflection.GetPrivateField<Otherworld>(numa, "_currentOtherworld");
            var mansus = new MansusStateImpl(otherworld);

            return new GameStateImpl(tabletopCards.ToArray(), situations.ToArray(), mansus);
        }
    }
}
