namespace AutoccultistNS.UI
{
    using System.IO;
    using SecretHistories.Infrastructure.Modding;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageWidget : SizingLayoutWidget<ImageWidget>
    {
        public ImageWidget(string key)
            : this(new GameObject(key))
        {
        }

        public ImageWidget(GameObject gameObject)
            : base(gameObject)
        {
        }

        public virtual Image ImageBehavior => this.GameObject.GetOrAddComponent<Image>();

        public ImageWidget Sprite(string resourceName)
        {
            // This is a partial reimplementation of ResourcesManager.GetSpriteForUI
            // We want to know if the sprite is not found, to use our fallback lookup.
            string uiPath = Path.Combine(Watchman.Get<SecretHistories.Services.Config>().GetConfigValue("imagesdir"), "ui", resourceName);

            var sprite = Watchman.Get<ModManager>().GetSprite(uiPath);
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(uiPath);
            }

            if (sprite == null)
            {
                sprite = ResourceHack.FindSprite(resourceName);
            }

            if (sprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite {resourceName}");
            }

            this.ImageBehavior.sprite = sprite;

            return this;
        }

        public ImageWidget Sprite(Sprite sprite)
        {
            this.ImageBehavior.sprite = sprite;
            return this;
        }

        public ImageWidget StretchImage()
        {
            this.ImageBehavior.type = Image.Type.Simple;
            this.ImageBehavior.preserveAspect = false;
            return this;
        }

        public ImageWidget CenterImage()
        {
            this.ImageBehavior.type = Image.Type.Simple;
            this.ImageBehavior.preserveAspect = true;
            return this;
        }

        public ImageWidget SlicedImage()
        {
            this.ImageBehavior.type = Image.Type.Sliced;
            return this;
        }

        public ImageWidget Color(Color color)
        {
            this.ImageBehavior.color = color;
            return this;
        }
    }
}
