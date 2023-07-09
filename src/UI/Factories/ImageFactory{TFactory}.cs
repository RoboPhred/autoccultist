namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageFactory<TFactory> : LayoutElementFactory<TFactory>
        where TFactory : ImageFactory<TFactory>
    {
        private readonly Image image;

        public ImageFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.image = this.gameObject.GetOrAddComponent<Image>();
        }

        public TFactory Sprite(string resourceName)
        {
            this.image.sprite = ResourceHack.FindSprite(resourceName);
            return this as TFactory;
        }

        public TFactory StretchImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = false;
            return this as TFactory;
        }

        public TFactory CenterImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = true;
            return this as TFactory;
        }

        public TFactory SliceImage()
        {
            this.image.type = Image.Type.Sliced;
            return this as TFactory;
        }

        public TFactory Color(Color color)
        {
            this.image.color = color;
            return this as TFactory;
        }

        public new Image Build()
        {
            return this.image;
        }
    }
}
