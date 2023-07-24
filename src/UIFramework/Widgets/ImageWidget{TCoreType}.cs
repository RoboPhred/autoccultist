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

        public TCoreType SetSprite(string resourceName)
        {
            var sprite = ResourceResolver.GetSprite(resourceName);
            if (sprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite {resourceName}");
            }

            this.Image.sprite = sprite;

            return this as TCoreType;
        }

        public TCoreType SetSprite(Sprite sprite)
        {
            this.Image.sprite = sprite;
            return this as TCoreType;
        }

        public TCoreType SetColor(Color color)
        {
            this.Image.color = color;
            return this as TCoreType;
        }

        public TCoreType StretchImage()
        {
            this.Image.type = Image.Type.Simple;
            this.Image.preserveAspect = false;
            return this as TCoreType;
        }

        public TCoreType CenterImage()
        {
            this.Image.type = Image.Type.Simple;
            this.Image.preserveAspect = true;
            return this as TCoreType;
        }

        public TCoreType SlicedImage()
        {
            this.Image.type = Image.Type.Sliced;
            this.Image.preserveAspect = true;
            return this as TCoreType;
        }
    }
}
