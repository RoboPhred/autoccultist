namespace AutoccultistNS.UI
{
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
            this.Image = this.GameObject.GetOrAddComponent<Image>();
        }

        public Image Image { get; private set; }

        public ImageWidget Sprite(string resourceName)
        {
            this.Image.sprite = ResourceHack.FindSprite(resourceName);
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
            return this;
        }

        public ImageWidget Color(Color color)
        {
            this.Image.color = color;
            return this;
        }
    }
}
