namespace AutoccultistNS.Tokens
{
    using System;
    using AutoccultistNS.Brain;
    using AutoccultistNS.UI;
    using SecretHistories;
    using SecretHistories.Abstract;
    using SecretHistories.Enums;
    using SecretHistories.Ghosts;
    using SecretHistories.Manifestations;
    using SecretHistories.Services;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(RectTransform))]
    public class AutomationManifestation : BasicManifestation, IManifestation, IPointerEnterHandler, IPointerExitHandler
    {
        private const float RotationSpeedPerSecond = -360 / 16;

        private Spinner artworkSpinner;
        private int runningImpulses = 0;

        private Token token;

        private ImageWidget glowImage;
        private GraphicFader glowFader;

        public bool RequestingNoDrag => false;

        public bool RequestingNoSplit => true;

        public bool NoPush => false;

        private IImperative Imperative => ((AutomationPayload)this.token.Payload).Imperative;

        public void Initialise(IManifestable manifestable)
        {
            this.token = manifestable.GetToken();

            NucleusAccumbens.ImpulseStarted += this.OnImpulseStarted;
            NucleusAccumbens.ImpulseEnded += this.OnImpulseEnded;

            this.RectTransform.anchoredPosition = Vector2.zero;
            this.RectTransform.sizeDelta = new Vector2(140, 140);

            var prefab = Watchman.Get<PrefabFactory>().GetPrefabObjectFromResources<VerbManifestation>("manifestations");
            if (prefab == null)
            {
                NoonUtility.LogWarning("Could not find prefab for VerbManifestation");
                return;
            }

            WidgetMountPoint.On(this.gameObject, mountPoint =>
            {
                // We can't rely resourceHack here as we might get spawned in before any verbs.
                var tokenOutline = prefab.gameObject.transform.Find("Glow")?.GetComponent<Image>()?.sprite;
                if (tokenOutline == null)
                {
                    NoonUtility.LogWarning("Could not find token outline sprite");
                }
                else
                {
                    this.glowImage = mountPoint.AddImage("Glow")
                        .SetLeft(0, -8)
                        .SetRight(1, 8)
                        .SetTop(1, 8)
                        .SetBottom(0, -8)
                        .SetSprite(tokenOutline)
                        .SlicedImage()
                        .Behavior<GraphicFader>(fader =>
                        {
                            this.glowFader = fader;
                            fader.Hide(true);
                        });
                }

                var tokenBody = Reflection.GetPrivateField<Image>(prefab, "tokenBody").sprite;
                mountPoint.AddImage("Token")
                    .SetSprite(tokenBody);

                mountPoint.AddImage("Artwork")
                    .SetAnchor(new Vector3(0, 5.5f, -2))
                    .SetLeft(0.5f, -50)
                    .SetRight(0.5f, 50)
                    .SetTop(0.5f, 55.5f)
                    .SetBottom(0.5f, -44.5f)
                    .SetSize(new Vector2(100, 100))
                    .SetColor(new Color(1, 1, 1, 0.25f))
                    .SetSprite("autoccultist_imperative_artwork")
                    .Behavior<Spinner>(spinner =>
                    {
                        this.artworkSpinner = spinner;
                        spinner.Speed = RotationSpeedPerSecond;
                        NoonUtility.LogWarning("WTFV stopping spinner");
                        spinner.StopSpinning();
                    });

                mountPoint.AddText("Label")
                    .SetAnchor(new Vector3(0, 5.5f, -2))
                    .SetLeft(0.5f, -50)
                    .SetRight(0.5f, 50)
                    .SetTop(0.5f, 55.5f)
                    .SetBottom(0.5f, -44.5f)
                    .SetSize(new Vector2(100, 100))
                    .SetMinFontSize(16)
                    .SetMaxFontSize(32)
                    .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                    .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                    .SetOverflowMode(TMPro.TextOverflowModes.Ellipsis)
                    .SetText(this.Imperative.Name);
            });
        }

        public override Vector2 GetGridOffset(Vector2 gridCellSize, Sphere forSphere)
        {
            return new Vector2(0.0f, -gridCellSize.y / 2.0f);
        }

        public override void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            NucleusAccumbens.ImpulseStarted -= this.OnImpulseStarted;
            NucleusAccumbens.ImpulseEnded -= this.OnImpulseEnded;

            if (vfx == RetirementVFX.Default)
            {
                // TODO: Effect.  Check DoVanishFx on VerbManifestation.
                // Needs a prefab specified by the verb prefab...
            }

            UnityEngine.Object.Destroy(this.gameObject);
            callbackOnRetired();
        }

        public IGhost CreateGhost()
        {
            // TODO: This is just a no-content AbstractGhost, but we should make our own
            // in case it ever actually cares about what a Verb is.
            return Watchman.Get<PrefabFactory>().CreateGhostPrefab(typeof(VerbGhost), this.RectTransform);
        }

        public void Emphasise()
        {
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
            if (highlightType == HighlightType.Hover)
            {
                SoundManager.PlaySfx("TokenHover");
                this.glowImage.SetColor(UIStyle.GetGlowColor(UIStyle.GlowPurpose.OnHover));
                this.glowFader.Show();
            }
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            if (highlightType == HighlightType.Hover)
            {
                this.glowFader.Hide();
            }
        }

        public OccupiesSpaceAs OccupiesSpaceAs()
        {
            return SecretHistories.Enums.OccupiesSpaceAs.PhysicalObject;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.glowFader.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.glowFader.Hide();
        }

        public void Shroud(bool instant)
        {
        }

        public void Understate()
        {
        }

        public void Unshroud(bool instant)
        {
        }

        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
        }

        private void OnImpulseStarted(object sender, ImpulseEventArgs e)
        {
            if (e.Imperative != this.Imperative)
            {
                return;
            }

            NoonUtility.LogWarning("Impulse started " + e.Imperative.Name + " " + e.Impulse.ToString());

            this.runningImpulses++;

            if (this.runningImpulses == 1)
            {
                this.artworkSpinner.StartSpinning();
            }
        }

        private void OnImpulseEnded(object sender, ImpulseEventArgs e)
        {
            if (e.Imperative != this.Imperative)
            {
                return;
            }

            this.runningImpulses--;

            if (this.runningImpulses <= 0)
            {
                this.runningImpulses = 0;
                this.artworkSpinner.StopSpinning();
            }
        }
    }
}
