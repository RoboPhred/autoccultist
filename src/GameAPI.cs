namespace Autoccultist
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Entities;
    using Assets.Core.Interfaces;
    using Assets.CS.TabletopUI;
    using Assets.TabletopUi;
    using TabletopUi.Scripts.Interfaces;
    using UnityEngine;

    /// <summary>
    /// A set of static functions for manipulating the game.
    /// </summary>
    public static class GameAPI
    {
        /// <summary>
        /// Gets a value indicating whether the game is running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game is interactable.
        /// </summary>
        public static bool IsInteractable
        {
            get
            {
                return IsRunning && DraggableToken.draggingEnabled && (!IsInMansus || IsMansusInteractable);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mansus is active.
        /// </summary>
        public static bool IsInMansus
        {
            get
            {
                return TabletopManager.IsInMansus();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mansus is ready for interaction.
        /// </summary>
        public static bool IsMansusInteractable
        {
            get
            {
                // We need to wait out mansus animations.  There are two of them.
                // 1: Mansus screen fade-in (turns off DraggableToken.draggingEnabled while fading)
                // 2: Mansus token container fade-in (detected by alpha changing)
                var fader = Reflection.GetPrivateField<CanvasGroupFader>(TabletopManager.mapTokenContainer, "canvasGroupFader");
                var faderGroup = fader.GetComponent<CanvasGroup>();
                return IsInMansus && DraggableToken.draggingEnabled && faderGroup.alpha == 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the game is paused.
        /// </summary>
        public static bool IsPaused
        {
            get
            {
                return TabletopManager.IsPaused();
            }
        }

        /// <summary>
        /// Gets the tabletop manager.
        /// </summary>
        public static TabletopManager TabletopManager
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
        /// Initialize the GameAPI.
        /// <para>
        /// Cannot be a static constructor, as this must run early, before GameAPI is naturally used.
        /// </summary>
        public static void Initialize()
        {
            GameEventSource.GameStarted += OnGameStarted;
            GameEventSource.GameEnded += OnGameEnded;
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
        /// Gets a recipe by recipe id.
        /// </summary>
        /// <param name="recipeId">The recipe id of the recipe to get.</param>
        /// <returns>The recipe matching the recipe id.</returns>
        public static Recipe GetRecipe(string recipeId)
        {
            return Registry.Retrieve<ICompendium>().GetRecipeById(recipeId);
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
        /// Takes a stack of a single card from an existing stack.
        /// </summary>
        /// <param name="stack">The stack to obtain a card from.</param>
        /// <returns>A stack of a single card.</returns>
        public static IElementStack TakeOneCard(IElementStack stack)
        {
            if (stack.Quantity > 1)
            {
                return stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
            }

            if (stack.Quantity == 1)
            {
                return stack;
            }

            return null;
        }

        /// <summary>
        /// Slots a card into the given slot.
        /// If card is a stack of cards, only one card will be slotted.
        /// </summary>
        /// <param name="slot">The slot to place the card into.</param>
        /// <param name="stack">The card stack to pick a card from.</param>
        public static void SlotCard(RecipeSlot slot, IElementStack stack)
        {
            var singleCard = TakeOneCard(stack);
            if (singleCard == null)
            {
                return;
            }

            slot.AcceptStack(singleCard, new Context(Context.ActionSource.PlayerDrag));
        }

        /// <summary>
        /// Display a notification toast to the user.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message of the toast.</param>
        public static void Notify(string title, string message)
        {
            try
            {
                Registry.Retrieve<INotifier>().ShowNotificationWindow(title, message);
            }
            catch (ApplicationException)
            {
                // INotifier is not available until the game fully starts up.
            }
        }

        private static void OnGameStarted(object sender, EventArgs e)
        {
            IsRunning = true;
        }

        private static void OnGameEnded(object sender, EventArgs e)
        {
            IsRunning = false;
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
