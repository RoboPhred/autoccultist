namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingLayoutWidget : RectTransformWidget<SizingLayoutWidget>
    {
        private LayoutElement layoutElement;
        private ContentSizeFitter contentSizeFitter;
        private ConstrainedLayoutElement constrainedLayoutElement;

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
                    this.layoutElement.flexibleWidth = 0;
                    this.layoutElement.flexibleHeight = 0;
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

        public ConstrainedLayoutElement ConstrainedLayoutElement
        {
            get
            {
                if (this.constrainedLayoutElement == null)
                {
                    this.constrainedLayoutElement = this.GameObject.GetOrAddComponent<ConstrainedLayoutElement>();
                }

                return this.constrainedLayoutElement;
            }
        }

        public static implicit operator WidgetMountPoint(SizingLayoutWidget widget)
        {
            return new WidgetMountPoint(widget.GameObject.transform);
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

        public SizingLayoutWidget MaxWidth(float width)
        {
            this.ConstrainedLayoutElement.MaxWidth = width;
            return this;
        }

        public SizingLayoutWidget MaxHeight(float height)
        {
            this.ConstrainedLayoutElement.MaxHeight = height;
            return this;
        }

        public SizingLayoutWidget ExpandWidth()
        {
            this.LayoutElement.flexibleWidth = 1;
            return this;
        }

        public SizingLayoutWidget ExpandHeight()
        {
            this.LayoutElement.flexibleHeight = 1;
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

        public SizingLayoutWidget AddContent(GameObject gameObject)
        {
            gameObject.transform.SetParent(this.GameObject.transform, false);
            return this;
        }

        public SizingLayoutWidget AddContent(Action<WidgetMountPoint> contentFactory)
        {
            contentFactory(new WidgetMountPoint(this.GameObject.transform));
            return this;
        }
    }
}
