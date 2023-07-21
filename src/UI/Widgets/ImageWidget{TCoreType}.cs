namespace AutoccultistNS.UI
{
    using System.IO;
    using SecretHistories.Infrastructure.Modding;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageWidget<TCoreType> : SizingLayoutWidget<TCoreType>
        where TCoreType : ImageWidget<TCoreType>
    {
        public ImageWidget(string key)
            : this(new GameObject(key))
        {
        }

        public ImageWidget(GameObject gameObject)
            : base(gameObject)
        {
        }

        public virtual Image ImageBehavior
        {
            get
            {
                return this.GameObject.GetOrAddComponent<Image>();
            }
        }

        public TCoreType Sprite(string resourceName)
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

            this.ImageBehavior.sprite = sprite;

            return this as TCoreType;
        }

        public TCoreType Sprite(Sprite sprite)
        {
            this.ImageBehavior.sprite = sprite;
            return this as TCoreType;
        }

        public TCoreType StretchImage()
        {
            this.ImageBehavior.type = Image.Type.Simple;
            this.ImageBehavior.preserveAspect = false;
            return this as TCoreType;
        }

        public TCoreType CenterImage()
        {
            this.ImageBehavior.type = Image.Type.Simple;
            this.ImageBehavior.preserveAspect = true;
            return this as TCoreType;
        }

        public TCoreType SliceImage()
        {
            this.ImageBehavior.type = Image.Type.Sliced;
            return this as TCoreType;
        }

        public TCoreType Color(Color color)
        {
            this.ImageBehavior.color = color;
            return this as TCoreType;
        }
    }
}
