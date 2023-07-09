namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class LayoutElementFactory : RectTransformFactory<LayoutElementFactory>
    {
        private readonly LayoutElement layoutElement;

        public LayoutElementFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.layoutElement = this.gameObject.AddComponent<LayoutElement>();
        }

        public LayoutElementFactory MinWidth(float width)
        {
            this.layoutElement.minWidth = width;
            return this;
        }

        public LayoutElementFactory MinHeight(float height)
        {
            this.layoutElement.minHeight = height;
            return this;
        }

        public LayoutElementFactory PreferredHeight(float height)
        {
            this.layoutElement.preferredHeight = height;
            return this;
        }

        public LayoutElementFactory PreferredWidth(float width)
        {
            this.layoutElement.preferredWidth = width;
            return this;
        }

        public new LayoutElement Build()
        {
            return this.layoutElement;
        }
    }
}
