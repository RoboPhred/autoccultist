namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingLayoutWidget<TCoreType> : RectTransformWidget<TCoreType>
        where TCoreType : SizingLayoutWidget<TCoreType>
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

        public ContentSizeFitter.FitMode VerticalFit
        {
            get
            {
                return this.ContentSizeFitter.verticalFit;
            }

            set
            {
                this.ContentSizeFitter.verticalFit = value;
            }
        }

        public ContentSizeFitter.FitMode HorizontalFit
        {
            get
            {
                return this.ContentSizeFitter.horizontalFit;
            }

            set
            {
                this.ContentSizeFitter.horizontalFit = value;
            }
        }

        public static implicit operator SizingLayoutWidget(SizingLayoutWidget<TCoreType> widget)
        {
            return new SizingLayoutWidget(widget.GameObject);
        }

        public TCoreType SetIgnoreLayout()
        {
            this.LayoutElement.ignoreLayout = true;
            return this as TCoreType;
        }

        public TCoreType SetMinWidth(float width)
        {
            this.LayoutElement.minWidth = width;
            return this as TCoreType;
        }

        public TCoreType SetMinHeight(float height)
        {
            this.LayoutElement.minHeight = height;
            return this as TCoreType;
        }

        public TCoreType SetPreferredWidth(float width)
        {
            this.LayoutElement.preferredWidth = width;
            return this as TCoreType;
        }

        public TCoreType SetPreferredHeight(float height)
        {
            this.LayoutElement.preferredHeight = height;
            return this as TCoreType;
        }

        /*
        public TCoreType MaxWidth(float width)
        {
            this.ConstrainedLayoutElement.MaxWidth = width;
            return this as TCoreType;
        }

        public TCoreType MaxHeight(float height)
        {
            this.ConstrainedLayoutElement.MaxHeight = height;
            return this as TCoreType;
        }
        */

        public TCoreType SetExpandWidth()
        {
            this.LayoutElement.flexibleWidth = 1;
            return this as TCoreType;
        }

        public TCoreType SetExpandHeight()
        {
            this.LayoutElement.flexibleHeight = 1;
            return this as TCoreType;
        }

        public TCoreType FitContentWidth()
        {
            this.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }

        public TCoreType FitContentHeight()
        {
            this.ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }
    }
}
