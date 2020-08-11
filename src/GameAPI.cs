using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using TabletopUi.Scripts.Interfaces;

namespace Autoccultist
{
    public static class GameAPI
    {
        private static TabletopManager TabletopManager
        {
            get
            {
                var tabletopManager = (TabletopManager)Registry.Retrieve<ITabletopManager>();
                if (tabletopManager == null)
                {
                    AutoccultistPlugin.Instance.Fatal("Could not retrieve ITabletopManager");
                }
                return tabletopManager;
            }
        }
        private static TabletopTokenContainer TabletopTokenContainer
        {
            get
            {
                return TabletopManager._tabletop;
            }
        }

        public static bool IsInteractable
        {
            get
            {
                return !TabletopManager.IsInMansus();
            }
        }

        public static void SetPaused(bool paused)
        {
            TabletopManager.SetPausedState(paused);
        }

        public static SituationController GetSituation(string situationId)
        {
            return Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations().FirstOrDefault(x => x.situationToken.EntityId == situationId);
        }

        public static ICollection<SituationController> GetAllSituations()
        {
            return Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
        }

        public static IElementStack GetSingleCard(string elementId)
        {
            var candidates =
                from token in TabletopTokenContainer.GetTokens()
                let card = token as ElementStackToken
                where card != null
                where card.EntityId == elementId
                orderby card.LifetimeRemaining
                select card;

            var choice = candidates.FirstOrDefault();
            if (choice == null)
            {
                return null;
            }

            if (choice.Quantity == 1)
            {
                return choice;
            }

            return choice.SplitAllButNCardsToNewStack(choice.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
        }

        public static IReadOnlyCollection<IElementStack> GetTabletopCards()
        {
            var candidates =
                from token in TabletopTokenContainer.GetTokens()
                let card = token as ElementStackToken
                where card != null && !card.IsBeingAnimated && !card.IsInAir && !card.Defunct
                select card;
            return candidates.ToArray();
        }

        public static void SlotCard(RecipeSlot slot, IElementStack card)
        {
            if (card.Quantity > 1)
            {
                var newStack = card.SplitAllButNCardsToNewStack(card.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
                slot.AcceptStack(newStack, new Context(Context.ActionSource.PlayerDrag));
            }
            else if (card.Quantity == 1)
            {
                slot.AcceptStack(card, new Context(Context.ActionSource.PlayerDrag));
            }
        }

        public static void Notify(string title, string message)
        {
            Registry.Retrieve<INotifier>().ShowNotificationWindow(title, message);
        }
    }
}