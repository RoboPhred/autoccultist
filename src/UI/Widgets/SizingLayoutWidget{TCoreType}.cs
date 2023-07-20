namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SizingLayoutWidget<TCoreType> : RectTransformWidget<TCoreType>
        where TCoreType : SizingLayoutWidget<TCoreType>
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
            this.LayoutElementBehavior.minHeight = -1;
            this.LayoutElementBehavior.minWidth = -1;
            this.LayoutElementBehavior.preferredHeight = -1;
            this.LayoutElementBehavior.preferredWidth = -1;
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

        public static implicit operator SizingLayoutWidget(SizingLayoutWidget<TCoreType> widget)
        {
            return new SizingLayoutWidget(widget.GameObject);
        }

        public TCoreType IgnoreLayout()
        {
            this.LayoutElementBehavior.ignoreLayout = true;
            return this as TCoreType;
        }

        public TCoreType MinWidth(float width)
        {
            this.LayoutElementBehavior.minWidth = width;
            return this as TCoreType;
        }

        public TCoreType MinHeight(float height)
        {
            this.LayoutElementBehavior.minHeight = height;
            return this as TCoreType;
        }

        public TCoreType PreferredWidth(float width)
        {
            this.LayoutElementBehavior.preferredWidth = width;
            return this as TCoreType;
        }

        public TCoreType PreferredHeight(float height)
        {
            this.LayoutElementBehavior.preferredHeight = height;
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

        public TCoreType ExpandWidth()
        {
            this.LayoutElementBehavior.flexibleWidth = 1;
            return this as TCoreType;
        }

        public TCoreType ExpandHeight()
        {
            this.LayoutElementBehavior.flexibleHeight = 1;
            return this as TCoreType;
        }

        public TCoreType FitContentWidth()
        {
            this.ContentSizeFitterBehavior.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }

        public TCoreType FitContentHeight()
        {
            this.ContentSizeFitterBehavior.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }
    }
}
