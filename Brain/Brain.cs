using System;
using System.Linq;
using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public class Brain : IGameState
    {
        private bool isStarted = false;
        private Imperative testImperative;

        private SituationManager situationManager = new SituationManager();

        public Brain(Imperative testImperative)
        {
            this.testImperative = testImperative;
        }

        public void Start()
        {
            this.isStarted = true;
            GameAPI.Heartbeat += OnHeartbeat;
        }

        public bool SituationIsAvailable(string situationId)
        {
            return this.situationManager.SituationIsAvailable(situationId);
        }

        public bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices)
        {
            // TODO: Take into account card reservations
            var cards = GameAPI.GetTabletopCards();
            foreach (var choice in choices)
            {
                var match = choice.GetMatch(cards);
                if (match == null)
                {
                    return false;
                }
                // Remove the match so it is not double counted.
                cards.Remove(match);
            }

            return true;
        }

        private void OnHeartbeat(object sender, EventArgs e)
        {
            // Scan through all possible imperatives and invoke the ones that can start.
            //  Where multiple imperatives try for the same verb, invoke the highest priority
            var candidateGroups =
                from imperative in this.GetActiveImperatives()
                where imperative.CanExecute(this)
                orderby imperative.Priority
                group imperative by imperative.SituationID into situationGroup
                select situationGroup;

            foreach (var group in candidateGroups)
            {
                var candidate = group.FirstOrDefault();
                if (candidate == null)
                {
                    return;
                }
                this.situationManager.ExecuteSituationSolution(candidate);
            }
        }

        private IList<Imperative> GetActiveImperatives()
        {
            return new Imperative[] { this.testImperative };
        }
    }
}