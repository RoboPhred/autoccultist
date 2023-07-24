namespace AutoccultistNS.UI
{
    using SecretHistories.Manifestations;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class HoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ImageWidget glowImage;
        private GraphicFader glowFader;

        public void Awake()
        {
            // HACK: Cant get glow from resources, get it from a prefab.
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
                        .WithBehavior<GraphicFader>(fader =>
                        {
                            this.glowFader = fader;
                            fader.Hide(true);
                        });
                    this.glowImage.GameObject.transform.SetAsFirstSibling();
                }
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.glowFader.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.glowFader.Hide();
        }
    }
}
