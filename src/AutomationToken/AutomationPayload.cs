namespace AutoccultistNS.Tokens
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.UI;
    using SecretHistories;
    using SecretHistories.Abstract;
    using SecretHistories.Commands;
    using SecretHistories.Commands.SituationCommands;
    using SecretHistories.Core;
    using SecretHistories.Entities;
    using SecretHistories.Entities.NullEntities;
    using SecretHistories.Enums;
    using SecretHistories.Events;
    using SecretHistories.Fucine;
    using SecretHistories.Logic;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using UnityEngine;

    [IsEncaustableClass(typeof(AutomationCreationCommand))]
    public class AutomationPayload : ITokenPayload
    {
        public static readonly string EntityPrefix = "automation_imperative_";

        private readonly List<AbstractDominion> dominions = new();

        private ImperativeAutomationWindow window;

        private Token token;

        public AutomationPayload(string id, string entityId)
        {
            this.Id = id;

            // meh...
            this.EntityId = entityId;

            var imperativeId = entityId;
            if (imperativeId.StartsWith(EntityPrefix))
            {
                imperativeId = imperativeId.Substring(EntityPrefix.Length);
            }
            else
            {
                throw new ArgumentException($"Invalid entity ID for automation: {entityId}");
            }

            this.Imperative = Library.GetById<IImperativeConfig>(imperativeId);

            this.window = AbstractWindow.CreateTabletopWindow<ImperativeAutomationWindow>($"Automation_${this.Imperative.Id}");
            this.window.Attach(this.Imperative);

            // TODO: Only add when we are on the tabletop sphere.
            // There doesn't seem to be any events telling us when we got placed in a sphere, however.
            NucleusAccumbens.AddImperative(this.Imperative);
        }

#pragma warning disable CS0067
        public event Action<TokenPayloadChangedArgs> OnChanged;

        public event Action<float> OnLifetimeSpent;
#pragma warning restore CS0067

        [DontEncaust]
        public IImperativeConfig Imperative { get; }

        [Encaust]
        public string Id { get; private set; }

        [Encaust]
        public string EntityId { get; private set; }

        [Encaust]
        public int Quantity => 1;

        [Encaust]
        public List<AbstractDominion> Dominions => this.dominions;

        [DontEncaust]
        public Token Token
        {
            get
            {
                {
                    if (this.token == null)
                    {
                        return NullToken.Create();
                    }

                    return this.token;
                }
            }
        }

        [DontEncaust]
        public string Label => this.Imperative.Name;

        [DontEncaust]
        public string Description => this.Imperative.Name;

        [DontEncaust]
        public string UniquenessGroup => this.Imperative.Id;

        [DontEncaust]
        public bool Unique => false;

        [DontEncaust]
        public string Icon => "memory";

        [DontEncaust]
        public bool Metafictional => false;

        [DontEncaust]
        public Dictionary<string, int> Mutations => throw new NotImplementedException();

        // TODO: What uses this?  Should we make use of it for our window?
        [DontEncaust]
        public bool IsOpen => this.window.IsOpen;

        [DontEncaust]
        public bool IsSealed => false;

        public bool ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            return false;
        }

        public string AudioRefinement(AudioEvent audioEvent)
        {
            return string.Empty;
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public void Close()
        {
            this.window.Close();
        }

        public void Conclude()
        {
            // TODO: Destroy self and end automation
        }

        public void AttachSphere(Sphere sphere)
        {
        }

        public void DetachSphere(Sphere sphere)
        {
        }

        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
        }

        public void FirstHeartbeat()
        {
        }

        public FucinePath GetAbsolutePath()
        {
            var pathAbove = this.Token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new();
        }

        public Sphere GetEnRouteSphere()
        {
            var enRouteSpherePath = this.Token.Sphere.GoverningSphereSpec.EnRouteSpherePath;
            if (enRouteSpherePath.IsValid() && !enRouteSpherePath.IsEmpty())
            {
                return Watchman.Get<HornedAxe>().GetSphereByPath(this.Token.Sphere, enRouteSpherePath);
            }

            return this.Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public string GetIllumination(string key)
        {
            return string.Empty;
        }

        public Dictionary<string, string> GetIlluminations()
        {
            return new();
        }

        public Type GetManifestationType(Sphere sphere)
        {
            return typeof(AutomationManifestation);
        }

        public RectTransform GetRectTransform()
        {
            return this.Token.TokenRectTransform;
        }

        public string GetSignature()
        {
            return this.Id;
        }

        public Sphere GetSphereById(string id)
        {
            return null;
        }

        public List<Sphere> GetSpheres()
        {
            return new();
        }

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return new();
        }

        public Timeshadow GetTimeshadow()
        {
            // TODO: Create infinite cyclic timeshadow, if possible.
            return Timeshadow.CreateTimelessShadow();
        }

        public Token GetToken()
        {
            return this.Token;
        }

        public FucinePath GetWildPath()
        {
            var wildPath = FucinePath.Wild();
            return wildPath.AppendingToken(this.Id);
        }

        public Sphere GetWindowsSphere()
        {
            // Wait, spheres are windows?  We spawn windows manually... check up on this.
            var windowSpherePath = this.Token.Sphere.GoverningSphereSpec.WindowsSpherePath;
            if (windowSpherePath.IsValid() && !windowSpherePath.IsEmpty())
            {
                return Watchman.Get<HornedAxe>().GetSphereByPath(this.Token.Sphere, windowSpherePath);
            }

            return this.Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.Initialise(this);
        }

        public void InteractWithIncoming(Token incomingToken)
        {
        }

        public bool IsPermanent()
        {
            return false;
        }

        public bool IsValid()
        {
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool ManifestationAcceptableForPayloadInSphere(IManifestation manifestation, Sphere sphere)
        {
            return !(manifestation.GetType() != this.GetManifestationType(sphere));
        }

        public void ModifyQuantity(int unsatisfiedChange)
        {
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
        }

        public void OpenAt(TokenLocation location)
        {
            this.window.OpenAt(location.LocalPosition);
        }

        public bool ReceiveNote(INotification notification, Context context)
        {
            return true;
        }

        public bool RegisterDominion(AbstractDominion dominion)
        {
            dominion.OnSphereAdded.AddListener(this.AttachSphere);
            dominion.OnSphereRemoved.AddListener(this.DetachSphere);

            if (this.dominions.Contains(dominion))
            {
                return false;
            }

            this.dominions.Add(dominion);

            return true;
        }

        public bool Retire(RetirementVFX vfx)
        {
            SoundManager.PlaySfx("SituationTokenRetire");
            NucleusAccumbens.RemoveImperative(this.Imperative);
            return true;
        }

        public void SetIllumination(string key, string value)
        {
        }

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
        }

        public void SetQuantity(int quantityToLeaveBehind)
        {
        }

        public void SetToken(Token token)
        {
            this.token = token;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
        }

        public void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand)
        {
        }
    }
}
