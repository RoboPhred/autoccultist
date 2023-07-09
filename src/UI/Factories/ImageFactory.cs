namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageFactory : SizingElementFactory<ImageFactory>
    {
        private readonly Image image;

        public ImageFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.image = this.GameObject.GetOrAddComponent<Image>();
        }

        public ImageFactory Sprite(string resourceName)
        {
            this.image.sprite = ResourceHack.FindSprite(resourceName);
            return this;
        }

        public ImageFactory StretchImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = false;
            return this;
        }

        public ImageFactory CenterImage()
        {
            this.image.type = Image.Type.Simple;
            this.image.preserveAspect = true;
            return this;
        }

        public ImageFactory SlicedImage()
        {
            this.image.type = Image.Type.Sliced;
            return this;
        }

        public ImageFactory Color(Color color)
        {
            this.image.color = color;
            return this;
        }

        public new Image Build()
        {
            return this.image;
        }
    }
}
