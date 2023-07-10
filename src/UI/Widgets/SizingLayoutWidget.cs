namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingLayoutWidget : RectTransformWidget<SizingLayoutWidget>
    {
        private LayoutElement layoutElement;
        private ContentSizeFitter contentSizeFitter;

        public SizingLayoutWidget(string key)
            : this(new GameObject(key))
        {
        }

        public SizingLayoutWidget(GameObject gameObject)
            : base(gameObject)
        {
        }

        public LayoutElement LayoutElement
        {
            get
            {
                if (this.layoutElement == null)
                {
                    this.layoutElement = this.GameObject.GetOrAddComponent<LayoutElement>();
                    this.layoutElement.flexibleWidth = -1;
                    this.layoutElement.flexibleHeight = -1;
                    this.LayoutElement.minHeight = -1;
                    this.LayoutElement.minWidth = -1;
                    this.LayoutElement.preferredHeight = -1;
                    this.LayoutElement.preferredWidth = -1;
                }

                return this.layoutElement;
            }
        }

        public ContentSizeFitter ContentSizeFitter
        {
            get
            {
                if (this.contentSizeFitter == null)
                {
                    this.contentSizeFitter = this.GameObject.GetOrAddComponent<ContentSizeFitter>();
                    this.ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    this.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }

                return this.contentSizeFitter;
            }
        }

        public SizingLayoutWidget IgnoreLayout()
        {
            this.LayoutElement.ignoreLayout = true;
            return this;
        }

        public SizingLayoutWidget MinWidth(float width)
        {
            this.LayoutElement.minWidth = width;
            return this;
        }

        public SizingLayoutWidget MinHeight(float height)
        {
            this.LayoutElement.minHeight = height;
            return this;
        }

        public SizingLayoutWidget PreferredWidth(float width)
        {
            this.LayoutElement.preferredWidth = width;
            return this;
        }

        public SizingLayoutWidget PreferredHeight(float height)
        {
            this.LayoutElement.preferredHeight = height;
            return this;
        }

        public SizingLayoutWidget FlexibleWidth(float width)
        {
            this.LayoutElement.flexibleWidth = width;
            return this;
        }

        public SizingLayoutWidget FlexibleHeight(float height)
        {
            this.LayoutElement.flexibleHeight = height;
            return this;
        }

        public SizingLayoutWidget FillContentWidth()
        {
            this.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }

        public SizingLayoutWidget FillContentHeight()
        {
            this.ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }
    }
}
