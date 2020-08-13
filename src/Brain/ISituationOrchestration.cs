using System;
using Autoccultist.GameState;

namespace Autoccultist.Brain
{
    interface ISituationOrchestration
    {
        string SituationId { get; }
        void Start(IGameState state);
        void Update(IGameState state);

        event EventHandler Completed;
    }
}