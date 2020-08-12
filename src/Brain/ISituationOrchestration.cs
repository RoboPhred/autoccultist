using System;

namespace Autoccultist.Brain
{
    interface ISituationOrchestration
    {
        string SituationId { get; }
        void Start();
        void Update();

        event EventHandler Completed;
    }
}