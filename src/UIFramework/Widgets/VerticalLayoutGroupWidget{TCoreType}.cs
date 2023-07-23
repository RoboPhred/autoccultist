namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class VerticalLayoutGroupWidget<TCoreType> : SizingLayoutWidget<TCoreType>
        where TCoreType : VerticalLayoutGroupWidget<TCoreType>
    {
        public VerticalLayoutGroupWidget(string key)
            : this(new GameObject(key))
        {
        }

        public VerticalLayoutGroupWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.LayoutGroup = this.GameObject.GetOrAddComponent<VerticalLayoutGroup>();

            this.SetSpacing(0);

            // These absolutely must be on, or all our children will be zero-sized.
            // Yes, this also fucks up our own size.  Too bad.
            this.EnableLayoutSystemHorizontal(true);
            this.EnableLayoutSystemVertical(true);

            this.SetSpreadChildrenHorizontally(false);
            this.SetSpreadChildrenVertically(false);
        }

        public VerticalLayoutGroup LayoutGroup { get; }

        public TCoreType SetChildAlignment(TextAnchor value)
        {
            this.LayoutGroup.childAlignment = value;
            return this as TCoreType;
        }

        // Fucking stupid name, but this property does both.
        public TCoreType EnableLayoutSystemHorizontal(bool value = true)
        {
            this.LayoutGroup.childControlWidth = value;
            return this as TCoreType;
        }

        // Fucking stupid name, but this property does both.
        public TCoreType EnableLayoutSystemVertical(bool value = true)
        {
            this.LayoutGroup.childControlHeight = value;
            return this as TCoreType;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public TCoreType SetSpreadChildrenHorizontally(bool value = true)
        {
            this.LayoutGroup.childForceExpandWidth = value;
            return this as TCoreType;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public TCoreType SetSpreadChildrenVertically(bool value = true)
        {
            this.LayoutGroup.childForceExpandHeight = value;
            return this as TCoreType;
        }

        public TCoreType SetSpacing(float value)
        {
            this.LayoutGroup.spacing = value;
            return this as TCoreType;
        }

        public TCoreType SetPadding(int left, int top, int right, int bottom)
        {
            this.LayoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this as TCoreType;
        }

        public TCoreType SetPadding(int x, int y)
        {
            this.LayoutGroup.padding = new RectOffset(x, x, y, y);
            return this as TCoreType;
        }

        public TCoreType SetPadding(int value)
        {
            this.LayoutGroup.padding = new RectOffset(value, value, value, value);
            return this as TCoreType;
        }

        public TCoreType AddContent(GameObject gameObject)
        {
            gameObject.transform.SetParent(this.GameObject.transform, false);
            return this as TCoreType;
        }

        public TCoreType AddContent(Action<WidgetMountPoint> factory)
        {
            factory(new WidgetMountPoint(this.GameObject.transform));
            return this as TCoreType;
        }
    }
}
