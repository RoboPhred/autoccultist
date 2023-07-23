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

        public virtual Image Image => this.GameObject.GetOrAddComponent<Image>();

        public Sprite Sprite
        {
            get
            {
                return this.Image.sprite;
            }

            set
            {
                this.Image.sprite = value;
            }
        }

        public Color Color
        {
            get
            {
                return this.Image.color;
            }

            set
            {
                this.Image.color = value;
            }
        }

        public bool PreserveAspect
        {
            get
            {
                return this.Image.preserveAspect;
            }

            set
            {
                this.Image.preserveAspect = value;
            }
        }

        public Image.Type ImageType
        {
            get
            {
                return this.Image.type;
            }

            set
            {
                this.Image.type = value;
            }
        }

        public ImageWidget SetSprite(string resourceName)
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

            this.Image.sprite = sprite;

            return this;
        }

        public ImageWidget SetSprite(Sprite sprite)
        {
            this.Image.sprite = sprite;
            return this;
        }


        public ImageWidget SetColor(Color color)
        {
            this.Image.color = color;
            return this;
        }

        public ImageWidget StretchImage()
        {
            this.Image.type = Image.Type.Simple;
            this.Image.preserveAspect = false;
            return this;
        }

        public ImageWidget CenterImage()
        {
            this.Image.type = Image.Type.Simple;
            this.Image.preserveAspect = true;
            return this;
        }

        public ImageWidget SlicedImage()
        {
            this.Image.type = Image.Type.Sliced;
            this.Image.preserveAspect = true;
            return this;
        }
    }
}
