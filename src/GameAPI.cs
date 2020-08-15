namespace Autoccultist
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Entities;
    using Assets.Core.Interfaces;
    using Assets.CS.TabletopUI;
    using Assets.TabletopUi;
    using TabletopUi.Scripts.Interfaces;

    /// <summary>
    /// A set of static functions for manipulating the game.
    /// </summary>
    public static class GameAPI
    {
        /// <summary>
        /// Gets a value indicating whether the game is interactable.
        /// </summary>
        public static bool IsInteractable
        {
            get
            {
                return !TabletopManager.IsInMansus();
            }
        }

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

        /// <summary>
        /// Sets the pause state of the game.
        /// </summary>
        /// <param name="paused">True if the game should pause, or False if it should unpause.</param>
        public static void SetPaused(bool paused)
        {
            TabletopManager.SetPausedState(paused);
        }

        /// <summary>
        /// Gets a situation by a situation id.
        /// </summary>
        /// <param name="situationId">The situation id to retrieve the situation for.</param>
        /// <returns>The situation for the given situation id, or null.</returns>
        public static SituationController GetSituation(string situationId)
        {
            return Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations().Find(x => x.situationToken.EntityId == situationId);
        }

        /// <summary>
        /// Gets all situations currently existing.
        /// </summary>
        /// <returns>A collection of all situations.</returns>
        public static ICollection<SituationController> GetAllSituations()
        {
            return Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
        }

        /// <summary>
        /// Gets all cards on the tabletop.
        /// </summary>
        /// <returns>A collection of all cards on the tabletop.</returns>
        public static IReadOnlyCollection<IElementStack> GetTabletopCards()
        {
            var candidates =
                from token in TabletopTokenContainer.GetTokens()
                let card = token as ElementStackToken
                where card != null && IsCardAccessable(card)
                select card;
            return candidates.ToArray();
        }

        /// <summary>
        /// Slots a card into the given slot.
        /// If card is a stack of cards, only one card will be slotted.
        /// </summary>
        /// <param name="slot">The slot to place the card into.</param>
        /// <param name="card">The card stack to pick a card from.</param>
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

        /// <summary>
        /// Display a notification toast to the user.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message of the toast.</param>
        public static void Notify(string title, string message)
        {
            Registry.Retrieve<INotifier>().ShowNotificationWindow(title, message);
        }

        private static bool IsCardAccessable(ElementStackToken card)
        {
            if (card.IsBeingAnimated)
            {
                return false;
            }

            if (card.IsInAir)
            {
                return false;
            }

            if (card.Defunct)
            {
                return false;
            }

            return true;
        }
    }
}
