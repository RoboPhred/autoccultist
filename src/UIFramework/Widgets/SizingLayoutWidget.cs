namespace AutoccultistNS.UI
{
    using System;
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
            // Always do this, in case we find ourselves in a dreaded childControlsWidth/Height group.
            this.layoutElement = this.GameObject.GetOrAddComponent<LayoutElement>();
            this.layoutElement.flexibleWidth = 0;
            this.layoutElement.flexibleHeight = 0;
            this.layoutElement.minHeight = -1;
            this.layoutElement.minWidth = -1;
            this.layoutElement.preferredHeight = -1;
            this.layoutElement.preferredWidth = -1;
        }

        public LayoutElement LayoutElement
        {
            get
            {
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
                    this.contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    this.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }

                return this.contentSizeFitter;
            }
        }

        public bool IgnoreLayout
        {
            get
            {
                return this.LayoutElement.ignoreLayout;
            }

            set
            {
                this.LayoutElement.ignoreLayout = value;
            }
        }

        public float MinWidth
        {
            get
            {
                return this.LayoutElement.minWidth;
            }

            set
            {
                this.LayoutElement.minWidth = value;
            }
        }

        public float MinHeight
        {
            get
            {
                return this.LayoutElement.minHeight;
            }

            set
            {
                this.LayoutElement.minHeight = value;
            }
        }

        public float PreferredWidth
        {
            get
            {
                return this.LayoutElement.preferredWidth;
            }

            set
            {
                this.LayoutElement.preferredWidth = value;
            }
        }

        public float PreferredHeight
        {
            get
            {
                return this.LayoutElement.preferredHeight;
            }

            set
            {
                this.LayoutElement.preferredHeight = value;
            }
        }

        public SizingLayoutWidget SetIgnoreLayout(bool value = true)
        {
            this.LayoutElement.ignoreLayout = value;
            return this;
        }

        public SizingLayoutWidget SetMinWidth(float width)
        {
            this.LayoutElement.minWidth = width;
            return this;
        }

        public SizingLayoutWidget SetMinHeight(float height)
        {
            this.LayoutElement.minHeight = height;
            return this;
        }

        public SizingLayoutWidget SetPreferredWidth(float width)
        {
            this.LayoutElement.preferredWidth = width;
            return this;
        }

        public SizingLayoutWidget SetPreferredHeight(float height)
        {
            this.LayoutElement.preferredHeight = height;
            return this;
        }

        public SizingLayoutWidget SetExpandWidth()
        {
            this.LayoutElement.flexibleWidth = 1;
            return this;
        }

        public SizingLayoutWidget SetExpandHeight()
        {
            this.LayoutElement.flexibleHeight = 1;
            return this;
        }

        public SizingLayoutWidget SetFitContentWidth()
        {
            this.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this;
        }

        public SizingLayoutWidget SetFitContentHeight()
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
