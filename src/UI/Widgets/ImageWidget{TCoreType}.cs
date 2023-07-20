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
            NoonUtility.LogWarning("Loading sprite " + resourceName);
            // This is a partial reimplementation of ResourcesManager.GetSpriteForUI
            // We want to know if the sprite is not found, to use our fallback lookup.
            string uiPath = Path.Combine(Watchman.Get<SecretHistories.Services.Config>().GetConfigValue("imagesdir"), "ui", resourceName);
            NoonUtility.LogWarning(" - path " + uiPath);

            var sprite = Watchman.Get<ModManager>().GetSprite(uiPath);
            if (sprite == null)
            {
                NoonUtility.LogWarning(" - not found in mod manager, trying basic sprite at " + uiPath);
                sprite = Resources.Load<Sprite>(uiPath);
            }

            if (sprite == null)
            {
                NoonUtility.LogWarning(" - Still not found, trying ResourceHack");
                sprite = ResourceHack.FindSprite(resourceName);
            }

            if (sprite == null)
            {
                NoonUtility.LogWarning(" - Still not found, giving up");
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
