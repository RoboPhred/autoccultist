using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Autoccultist
{
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.Autoccultist", "Autoccultist", "0.0.1")]
    public class AutoccultistMod : BepInEx.BaseUnityPlugin
    {

        private TabletopTokenContainer TabletopTokenContainer
        {
            get
            {
                {
                    var tabletopManager = (TabletopManager)Registry.Retrieve<ITabletopManager>();
                    if (tabletopManager == null)
                    {
                        this.Logger.LogError("Could not fetch TabletopManager");
                    }

                    return tabletopManager._tabletop;
                }
            }
        }

        void Start()
        {
            this.Logger.LogInfo("Autoccultist initialized.");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                this.AutofillSituation();
            }
        }

        void AutofillSituation()
        {
            if (TabletopManager.IsInMansus())
            {
                return;
            }

            var situation = this.GetOpenSituation();
            if (situation == null)
            {
                return;
            }

            // situationToken is typed ISituationAnchor, but its core type is SituationToken which is a DraggableToken
            var situationDraggable = situation.situationToken as DraggableToken;
            var candidates = this.GetElementsOrderedByDistance(situationDraggable.RectTransform.anchoredPosition).ToArray();

            var window = situation.situationWindow;
            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    this.TryFillSlots(() => window.GetStartingSlots(), candidates);
                    break;
                case SituationState.Ongoing:
                    this.TryFillSlots(() => window.GetOngoingSlots(), candidates);
                    break;
            }
        }

        void TryFillSlots(Func<IList<RecipeSlot>> slotsResolver, IList<ElementStackToken> candidates)
        {
            // We handle the first slot independently, as chooing a first slot will
            //  usually determine what other slots are available.
            var primarySlot = slotsResolver().FirstOrDefault();
            if (primarySlot == null)
            {
                return;
            }

            if (!this.TrySatisfySlot(primarySlot, candidates))
            {
                return;
            }

            // Fill the remaining slots.
            //  We need to re-fetch the slots, as slotting the primary may
            //  provide us with new slots.
            var slots = slotsResolver();
            foreach (var slot in slots.Skip(1))
            {
                this.TrySatisfySlot(slot, candidates);
            }
        }

        bool TrySatisfySlot(RecipeSlot slot, IEnumerable<ElementStackToken> candidates)
        {
            if (slot.GetElementStackInSlot() != null)
            {
                // Already something in the slot.
                return true;
            }

            var candidate = candidates.FirstOrDefault(x => slot.GetSlotMatchForStack(x).MatchType == SlotMatchForAspectsType.Okay);
            if (candidate == null)
            {
                return false;
            }

            this.PopulateSlot(slot, candidate);
            return true;
        }

        void PopulateSlot(IRecipeSlot slot, ElementStackToken stack)
        {
            stack.lastTablePos = new Vector2?(stack.RectTransform.anchoredPosition);
            if (stack.Quantity != 1)
            {
                var newStack = stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
                slot.AcceptStack(newStack, new Context(Context.ActionSource.PlayerDrag));
            }
            else
            {
                slot.AcceptStack(stack, new Context(Context.ActionSource.PlayerDrag));
            }
        }

        IEnumerable<ElementStackToken> GetElementsOrderedByDistance(Vector2 fromPoint)
        {
            var elements =
                from token in this.TabletopTokenContainer.GetTokens()
                let stack = token as ElementStackToken
                where stack != null
                orderby CalcDistance(stack.RectTransform.anchoredPosition, fromPoint)
                select stack;
            return elements;
        }

        float CalcDistance(Vector2 a, Vector2 b)
        {
            var xDist = a.x - b.x;
            var yDist = a.y - b.y;
            return Math.Abs(xDist * xDist + yDist * yDist);
        }

        RecipeSlot ValidRecipeSlotOrNull(RecipeSlot slot)
        {
            if (slot.Defunct || slot.IsGreedy || slot.IsBeingAnimated)
            {
                return null;
            }
            return slot;
        }

        SituationController GetOpenSituation()
        {
            var situation = Registry.Retrieve<SituationsCatalogue>().GetOpenSituation();
            var token = situation.situationToken as SituationToken;
            if (token.Defunct || token.IsBeingAnimated)
            {
                return null;
            }

            return situation;
        }
    }
}