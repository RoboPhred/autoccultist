namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class LayoutElementFactory<TFactory> : RectTransformFactory<TFactory>
        where TFactory : LayoutElementFactory<TFactory>
    {
        private readonly LayoutElement layoutElement;
        private readonly ContentSizeFitter fitter;

        public LayoutElementFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.layoutElement = this.gameObject.AddComponent<LayoutElement>();
        }

        public TFactory MinWidth(float width)
        {
            this.layoutElement.minWidth = width;
            return this as TFactory;
        }

        public TFactory MinHeight(float height)
        {
            this.layoutElement.minHeight = height;
            return this as TFactory;
        }

        public TFactory PreferredHeight(float height)
        {
            this.layoutElement.preferredHeight = height;
            return this as TFactory;
        }

        public TFactory PreferredWidth(float width)
        {
            this.layoutElement.preferredWidth = width;
            return this as TFactory;
        }

        public new LayoutElement Build()
        {
            return this.layoutElement;
        }
    }
}
