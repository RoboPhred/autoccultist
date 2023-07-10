namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageWidget<TCoreType> : SizingLayoutWidget<TCoreType>
        where TCoreType : ImageWidget<TCoreType>
    {
        private readonly Image image;

        public ImageWidget(string key)
            : this(new GameObject(key))
        {
        }

        public ImageWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.image = this.GameObject.GetOrAddComponent<Image>();
        }

        public TCoreType Sprite(string resourceName)
        {
            this.image.sprite = ResourceHack.FindSprite(resourceName);
            return this as TCoreType;
        }

        public TCoreType StretchImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = false;
            return this as TCoreType;
        }

        public TCoreType CenterImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = true;
            return this as TCoreType;
        }

        public TCoreType SliceImage()
        {
            this.image.type = Image.Type.Sliced;
            return this as TCoreType;
        }

        public TCoreType Color(Color color)
        {
            this.image.color = color;
            return this as TCoreType;
        }
    }
}
