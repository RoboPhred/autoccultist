namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingElementFactory : RectTransformFactory<SizingElementFactory>
    {
        private LayoutElement layoutElement;
        private ContentSizeFitter contentSizeFitter;

        public SizingElementFactory(GameObject gameObject)
            : base(gameObject)
        {
        }

        public SizingElementFactory IgnoreLayout()
        {
            this.GetLayoutElement().ignoreLayout = true;
            return this;
        }

        public SizingElementFactory MinWidth(float width)
        {
            this.GetLayoutElement().minWidth = width;
            return this;
        }

        public SizingElementFactory MinHeight(float height)
        {
            this.GetLayoutElement().minHeight = height;
            return this;
        }

        public SizingElementFactory PreferredHeight(float height)
        {
            this.GetLayoutElement().preferredHeight = height;
            return this;
        }

        public SizingElementFactory PreferredWidth(float width)
        {
            this.GetLayoutElement().preferredWidth = width;
            return this;
        }

        public SizingElementFactory FillContentWidth()
        {
            this.GetLayoutElement().preferredWidth = -1;
            this.GetContentSizeFitter().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }

        public SizingElementFactory FillContentHeight()
        {
            this.GetLayoutElement().preferredHeight = -1;
            this.GetContentSizeFitter().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }

        private ContentSizeFitter GetContentSizeFitter()
        {
            if (this.contentSizeFitter == null)
            {
                this.contentSizeFitter = this.GameObject.GetOrAddComponent<ContentSizeFitter>();
            }
            return this.contentSizeFitter;
        }

        private LayoutElement GetLayoutElement()
        {
            if (this.layoutElement == null)
            {
                this.layoutElement = this.GameObject.GetOrAddComponent<LayoutElement>();
            }

            return this.layoutElement;
        }
    }
}
