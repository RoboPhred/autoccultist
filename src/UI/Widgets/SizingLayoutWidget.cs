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
            // Always do this, in case we find ourselves in a dreaded childControlsWidth/Height group.
            this.layoutElement = this.GameObject.GetOrAddComponent<LayoutElement>();
            this.layoutElement.flexibleWidth = 0;
            this.layoutElement.flexibleHeight = 0;
            this.layoutElement.minHeight = -1;
            this.layoutElement.minWidth = -1;
            this.layoutElement.preferredHeight = -1;
            this.layoutElement.preferredWidth = -1;
        }

        public LayoutElement LayoutElementBehavior
        {
            get
            {
                return this.layoutElement;
            }
        }

        public ContentSizeFitter ContentSizeFitterBehavior
        {
            get
            {
                if (this.contentSizeFitter == null)
                {
                    this.contentSizeFitter = this.GameObject.GetOrAddComponent<ContentSizeFitter>();
                    this.contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    this.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
            this.LayoutElementBehavior.ignoreLayout = true;
            return this;
        }

        public SizingLayoutWidget MinWidth(float width)
        {
            this.LayoutElementBehavior.minWidth = width;
            return this;
        }

        public SizingLayoutWidget MinHeight(float height)
        {
            this.LayoutElementBehavior.minHeight = height;
            return this;
        }

        public SizingLayoutWidget PreferredWidth(float width)
        {
            this.LayoutElementBehavior.preferredWidth = width;
            return this;
        }

        public SizingLayoutWidget PreferredHeight(float height)
        {
            this.LayoutElementBehavior.preferredHeight = height;
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
            this.LayoutElementBehavior.flexibleWidth = 1;
            return this;
        }

        public SizingLayoutWidget ExpandHeight()
        {
            this.LayoutElementBehavior.flexibleHeight = 1;
            return this;
        }

        public SizingLayoutWidget FillContentWidth()
        {
            this.ContentSizeFitterBehavior.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }

        public SizingLayoutWidget FillContentHeight()
        {
            this.ContentSizeFitterBehavior.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
