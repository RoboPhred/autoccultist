namespace AutoccultistNS.Tokens
{
    using AutoccultistNS.UI;
    using SecretHistories;
    using SecretHistories.Abstract;
    using SecretHistories.Enums;
    using SecretHistories.Ghosts;
    using SecretHistories.Manifestations;
    using SecretHistories.Spheres;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public class AutomationManifestation : BasicManifestation, IManifestation
    {
        public bool RequestingNoDrag => false;

        public bool RequestingNoSplit => true;

        public bool NoPush => false;

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }

        public void Emphasise()
        {
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Initialise(IManifestable manifestable)
        {
            RectTransform.anchoredPosition = Vector2.zero;
            RectTransform.sizeDelta = new Vector2(100, 100);

            WidgetMountPoint.On(this.gameObject, mountPoint =>
            {
                mountPoint.AddImage()
                    .Size(new Vector2(100, 100))
                    .Sprite("autoccultist_gears");
            });
        }

        public OccupiesSpaceAs OccupiesSpaceAs()
        {
            return SecretHistories.Enums.OccupiesSpaceAs.PhysicalObject;
        }

        public void Shroud(bool instant)
        {
        }

        public void Understate()
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unshroud(bool instant)
        {
        }

        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
        }
    }
}
