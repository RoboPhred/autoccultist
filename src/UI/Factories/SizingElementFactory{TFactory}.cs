namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingElementFactory<TFactory> : RectTransformFactory<TFactory>
        where TFactory : SizingElementFactory<TFactory>
    {
        private LayoutElement layoutElement;
        private ContentSizeFitter contentSizeFitter;

        public SizingElementFactory(GameObject gameObject)
            : base(gameObject)
        {
        }

        public TFactory IgnoreLayout()
        {
            this.GetLayoutElement().ignoreLayout = true;
            return this as TFactory;
        }

        public TFactory MinWidth(float width)
        {
            this.GetLayoutElement().minWidth = width;
            this.GetContentSizeFitter().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            return this as TFactory;
        }

        public TFactory MinHeight(float height)
        {
            this.GetLayoutElement().minHeight = height;
            this.GetContentSizeFitter().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            return this as TFactory;
        }

        public TFactory PreferredWidth(float width)
        {
            this.GetLayoutElement().preferredWidth = width;
            this.GetContentSizeFitter().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            return this as TFactory;
        }

        public TFactory PreferredHeight(float height)
        {
            this.GetLayoutElement().preferredHeight = height;
            this.GetContentSizeFitter().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            return this as TFactory;
        }

        public TFactory FillContentWidth()
        {
            this.GetLayoutElement().preferredWidth = -1;
            this.GetContentSizeFitter().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TFactory;
        }

        public TFactory FillContentHeight()
        {
            this.GetLayoutElement().preferredHeight = -1;
            this.GetContentSizeFitter().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TFactory;
        }

        public TFactory ShrinkContentWidth()
        {
            this.GetLayoutElement().preferredWidth = -1;
            this.GetContentSizeFitter().horizontalFit = ContentSizeFitter.FitMode.MinSize;
            return this as TFactory;
        }

        public TFactory ShrinkContentHeight()
        {
            this.GetLayoutElement().preferredHeight = -1;
            this.GetContentSizeFitter().verticalFit = ContentSizeFitter.FitMode.MinSize;
            return this as TFactory;
        }

        public new LayoutElement Build()
        {
            return this.layoutElement;
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
