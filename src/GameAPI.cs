namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;
    using SecretHistories.Assets.Scripts.Application.UI;
    using SecretHistories.Entities;
    using SecretHistories.Entities.NullEntities;
    using SecretHistories.Enums;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
    using SecretHistories.Spheres.Choreographers;
    using SecretHistories.Tokens.Payloads;
    using SecretHistories.UI;
    using UnityEngine;

    /// <summary>
    /// A set of static functions for manipulating the game.
    /// </summary>
    public static class GameAPI
    {
        private static int pauseDepth = 0;

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
                if (!IsRunning)
                {
                    return false;
                }

                // Mansus sets delays using this, so this might be a good check instead of IsMansusInteractable
                if (Watchman.Get<LocalNexus>().PlayerInputDisabled())
                {
                    return false;
                }

                // Apparently the above isn't good enough to wait on the mansus, so let's wait for the cards to appear.
                if (IsInMansus)
                {
                    var ingress = GetActiveIngress();
                    var hasChoices = GetMansusChoices(out var _) != null;
                    var hasOutput = ingress != null && ingress.GetEgressOutputSphere().GetTokens().Any();
                    if (!hasChoices && !hasOutput)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mansus is active.
        /// </summary>
        public static bool IsInMansus
        {
            get
            {
                return Watchman.Get<Numa>().IsOtherworldActive();
            }
        }

        /// <summary>
        /// Gets the current user-chosen game speed.
        /// </summary>
        public static GameSpeed GameSpeed
        {
            get
            {
                var heartGo = GameObject.Find("Heart");
                if (heartGo == null)
                {
                    NoonUtility.LogWarning("Could not find Heart.");
                    return GameSpeed.Paused;
                }

                var heart = heartGo.GetComponent<Heart>();
                return heart.GetEffectiveGameSpeed();
            }
        }

        /// <summary>
        /// Gets the tabletop sphere.
        /// </summary>
        public static Sphere TabletopSphere
        {
            get
            {
                if (!IsRunning)
                {
                    throw new Exception("GameAPI.TabletopSphere: Game is not running.");
                }

                Sphere tabletopSphere = Watchman.Get<HornedAxe>().GetSpheres().OfType<TabletopSphere>().FirstOrDefault();

                // When resetting the game, it is possible for GameStateProvider to try and generate a game state during the dissolution process
                // of the current save before the scene is unloaded.  This results in no tabletop sphere.
                return tabletopSphere ?? new NullSphere();
            }
        }

        public static Sphere CodexSphere
        {
            get
            {
                if (!IsRunning)
                {
                    throw new Exception("GameAPI.CodexSphere: Game is not running.");
                }

                return Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(new FucinePath("*/codex")) ?? new NullSphere();
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

            // Unlock the game if the heart is stopped.  Re-lock on start.
            MechanicalHeart.OnStart += (_, __) =>
            {
                if (pauseDepth > 0)
                {
                    Watchman.Get<LocalNexus>().ForcePauseGame(false);
                }
            };
            MechanicalHeart.OnStop += (_, __) =>
            {
                if (pauseDepth > 0)
                {
                    Watchman.Get<LocalNexus>().PauseGame(false);
                    Watchman.Get<LocalNexus>().UnForcePauseGame(false);
                }
            };
        }

        public static Task AwaitInteractable(CancellationToken cancellationToken)
        {
            if (IsInteractable)
            {
                return Task.FromResult(true);
            }

            return new AwaitConditionTask(() => IsInteractable, cancellationToken).Task;
        }

        /// <summary>
        /// Gets all EnRoute spheres.
        /// </summary>
        public static IEnumerable<Sphere> GetEnRouteSpheres()
        {
            if (!IsRunning)
            {
                return Enumerable.Empty<Sphere>();
            }

            return Watchman.Get<HornedAxe>().GetSpheres().OfType<EnRouteSphere>();
        }

        /// <summary>
        /// Gets all situations.
        /// </summary>
        public static IEnumerable<Situation> GetSituations()
        {
            if (!IsRunning)
            {
                return Enumerable.Empty<Situation>();
            }

            return Watchman.Get<HornedAxe>().GetRegisteredSituations();
        }

        /// <summary>
        /// Gets the situation with the given id.
        /// </summary>
        /// <param name="situationId">The id of the situation to get.</param>
        /// <returns>The situation with the given id, or null if no such situation exists.</returns>
        public static Situation GetSituation(string situationId)
        {
            return GetSituations().FirstOrDefault(x => x.VerbId == situationId);
        }

        /// <summary>
        /// Gets the active ingress.
        /// </summary>
        /// <returns>The active ingress, or null if there is no active ingress.</returns>
        public static Ingress GetActiveIngress()
        {
            if (!IsRunning)
            {
                return null;
            }

            // Ingress exists on the tabletop as a token when the overworld concludes and we are presented with the results
            // of the visit.
            var tabletopIngress = GetTabletopIngress();
            if (tabletopIngress != null)
            {
                return tabletopIngress;
            }

            // Not on the table... If we have an active otherworld visit, we might have one there.
            var otherworld = GetActiveOtherworld();
            if (otherworld != null)
            {
                return Reflection.GetPrivateField<Ingress>(otherworld, "_activeIngress");
            }

            // Note: There is a small period of time between the overworld concluding and the tabletop ingress appearing.
            return null;
        }

        public static Ingress GetTabletopIngress()
        {
            if (!IsRunning)
            {
                return null;
            }

            // Ingress exists on the tabletop as a token when the overworld concludes and we are presented with the results
            // of the visit.
            return TabletopSphere.GetTokens().Select(x => x.Payload).OfType<Ingress>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the active ingress, awaiting it if necessary.
        /// </summary>
        public static async Task<Ingress> AwaitTabletopIngress(CancellationToken cancellationToken)
        {
            var existingIngress = GetTabletopIngress();
            if (existingIngress != null)
            {
                return existingIngress;
            }

            await new AwaitConditionTask(() => GetTabletopIngress() != null, cancellationToken).Task;
            return GetTabletopIngress();
        }

        public static Otherworld GetActiveOtherworld()
        {
            if (!IsRunning)
            {
                return null;
            }

            var numa = Watchman.Get<Numa>();

            return Reflection.GetPrivateField<Otherworld>(numa, "_currentOtherworld");
        }

        public static void SortTable()
        {
            if (!IsRunning)
            {
                return;
            }

            (TabletopSphere.Choreographer as TabletopChoreographer).GroupAllStacks();
        }

        /// <summary>
        /// Gets the various card choices available in the mansus, if any
        /// </summary>
        /// <param name="faceUpDeckName">The name of the face up deck.</param>
        /// <returns>A dictionary of deck name to card.  If the mansus is not open, `null` is returned.</returns>
        public static IReadOnlyDictionary<string, ElementStack> GetMansusChoices(out string faceUpDeckName)
        {
            faceUpDeckName = null;

            try
            {
                var spheres = GetMansusSpheres(out faceUpDeckName);
                var value = new Dictionary<string, ElementStack>();
                foreach (var sphere in spheres)
                {
                    var token = sphere.Value.GetTokens().FirstOrDefault();
                    if (token == null)
                    {
                        // Mansus isn't ready.
                        // This could be because we have not drawn yet, or because the user has made a choice and we are now idle.
                        return null;
                    }

                    var elementStack = token.Payload as ElementStack;
                    if (elementStack == null)
                    {
                        throw new Exception($"Could not get ElementStack from token with payload id {token.PayloadId}");
                    }

                    value.Add(sphere.Key, token.Payload as ElementStack);
                }

                return spheres.ToDictionary(x => x.Key, x => x.Value.GetTokens().First().Payload as ElementStack);
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning($"Exception in GameAPI.GetMansusChoices: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Chooses a card from the deck in the mansus.
        /// </summary>
        /// <param name="deckName">The name of the deck to choose from.</param>
        /// <returns>True if the card was chosen, False otherwise.</returns>
        public static bool ChooseMansusDeck(string deckName)
        {
            try
            {
                var otherworld = GetActiveOtherworld();
                var ingress = GetActiveIngress();

                if (ingress == null || otherworld == null)
                {
                    return false;
                }

                var spheres = GetMansusSpheres(out var _);
                if (!spheres.TryGetValue(deckName, out var sphere))
                {
                    return false;
                }

                var dominions = Reflection.GetPrivateField<List<OtherworldDominion>>(otherworld, "_dominions");

                var dominion = dominions.FirstOrDefault(x => x.MatchesEgress(ingress.GetEgressId()));
                if (dominion == null)
                {
                    throw new Exception($"Could not find dominion for ingress {ingress.Id} in otherworld {otherworld.Id}");
                }

                var token = sphere.GetTokens().First();

                // Is this its own special thing?  Can we just try to drag it to ingress.GetEgressSphere()?
                if (!dominion.EgressSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning($"Exception in GameAPI.ChooseMansusDeck: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Gets the output card sphere from the mansus.
        /// </summary>
        public static Sphere GetMansusEgressSphere()
        {
            var ingress = GetActiveIngress();
            if (ingress == null)
            {
                return null;
            }

            var output = ingress.GetEgressOutputSphere();
            if (output == null)
            {
                return null;
            }

            return output;
        }

        /// <summary>
        /// Marks the user speed level as being unpaused.
        /// </summary>
        public static void UserUnpause()
        {
            Watchman.Get<LocalNexus>().UnPauseGame(true);
        }

        /// <summary>
        /// Re-applies the current GameAPI pause state into the game.
        /// </summary>
        public static void ReassertPause()
        {
            if (pauseDepth > 0)
            {
                // FIXME: Use a higher level pause depth (probably 4) so we don't have to fight the mansus for control.
                Watchman.Get<LocalNexus>().ForcePauseGame(false);
            }
        }

        /// <summary>
        /// Sets the pause state of the game.
        /// </summary>
        /// <returns>A token to unpause the game.</returns>
        public static PauseToken Pause()
        {
            if (pauseDepth == 0)
            {
                if (IsRunning)
                {
                    Watchman.Get<LocalNexus>().ForcePauseGame(true);
                }
            }

            pauseDepth++;

            return new PauseToken();
        }

        /// <summary>
        /// Display a notification toast to the user.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message of the toast.</param>
        public static void Notify(string title, string message)
        {
            var notifier = Watchman.Get<Notifier>();
            if (notifier == null)
            {
                return;
            }

            notifier.ShowNotificationWindow(title, message, false);
        }

        private static IReadOnlyDictionary<string, Sphere> GetMansusSpheres(out string faceUpDeckName)
        {
            var ingress = GetActiveIngress();
            var otherworld = GetActiveOtherworld();
            faceUpDeckName = null;

            if (ingress == null || otherworld == null)
            {
                return null;
            }

            var numa = Watchman.Get<Numa>();
            var compendium = Watchman.Get<Compendium>();

            var spheres = otherworld.GetSpheres();

            var consequences = ingress.GetConsequences();

            var faceUpDeck = compendium.GetEntityById<Recipe>(consequences[0].Id);
            var deckOne = compendium.GetEntityById<Recipe>(consequences[1].Id);
            var deckTwo = compendium.GetEntityById<Recipe>(consequences[2].Id);

            if (faceUpDeck == null || faceUpDeck.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("GetMansusSpheres: FaceUpDeck is null or has no DeckEffects");
            }

            if (deckOne == null || deckOne.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("GetMansusSpheres: DeckOne is null or has no DeckEffects");
            }

            if (deckTwo == null || deckTwo.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("GetMansusSpheres: DeckTwo is null or has no DeckEffects");
            }

            faceUpDeckName = faceUpDeck.DeckEffects.Keys.First();
            var deckOneName = deckOne.DeckEffects.Keys.First();
            var deckTwoName = deckTwo.DeckEffects.Keys.First();

            // FIXME: This is nasty, but HornexAxe.GetSphereByPath(FromPath, overworld) doesn't find these.
            // Which is strange, as the error message it gives contains the exact same path as we see from sphere.GetAbsolutePath().Path
            var faceUpSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[0].ToPath.Path);
            var deckOneSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[1].ToPath.Path);
            var deckTwoSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[2].ToPath.Path);

            if (faceUpSphere == null || deckOneSphere == null || deckTwoSphere == null)
            {
                throw new Exception("GetMansusSpheres: Could not find all spheres.");
            }

            return new Dictionary<string, Sphere>
            {
                { faceUpDeckName, faceUpSphere },
                { deckOneName, deckOneSphere },
                { deckTwoName, deckTwoSphere },
            };
        }

        private static void OnGameStarted(object sender, EventArgs e)
        {
            IsRunning = true;
        }

        private static void OnGameEnded(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        /// <summary>
        /// A token representing a pause.
        /// </summary>
        public class PauseToken : IDisposable
        {
            private bool isDisposed = false;

            /// <summary>
            /// Finalizes an instance of the <see cref="PauseToken"/> class.
            /// </summary>
            ~PauseToken()
            {
                NoonUtility.LogWarning("Leaked PauseToken");
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.isDisposed = true;
                GC.SuppressFinalize(this);
                pauseDepth--;
                if (pauseDepth == 0)
                {
                    Watchman.Get<LocalNexus>().UnForcePauseGame(true);
                }
            }
        }
    }
}
