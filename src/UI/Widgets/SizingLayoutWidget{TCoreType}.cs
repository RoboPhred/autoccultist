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
        }

        public LayoutElement LayoutElement
        {
            get
            {
                if (this.layoutElement == null)
                {
                    this.layoutElement = this.GameObject.GetOrAddComponent<LayoutElement>();
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

        public TCoreType IgnoreLayout()
        {
            this.LayoutElement.ignoreLayout = true;
            return this as TCoreType;
        }

        public TCoreType MinWidth(float width)
        {
            this.LayoutElement.minWidth = width;
            return this as TCoreType;
        }

        public TCoreType MinHeight(float height)
        {
            this.LayoutElement.minHeight = height;
            return this as TCoreType;
        }

        public TCoreType PreferredHeight(float height)
        {
            this.LayoutElement.preferredHeight = height;
            return this as TCoreType;
        }

        public TCoreType PreferredWidth(float width)
        {
            this.LayoutElement.preferredWidth = width;
            return this as TCoreType;
        }

        public TCoreType FillContentWidth()
        {
            this.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }

        public TCoreType FillContentHeight()
        {
            this.ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return this as TCoreType;
        }
    }
}
