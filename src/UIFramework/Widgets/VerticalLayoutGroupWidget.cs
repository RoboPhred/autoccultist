namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class VerticalLayoutGroupWidget : SizingLayoutWidget<VerticalLayoutGroupWidget>
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

        public VerticalLayoutGroupWidget SetChildAlignment(TextAnchor value)
        {
            this.LayoutGroup.childAlignment = value;
            return this;
        }

        // Fucking stupid name, but this property does both.
        public VerticalLayoutGroupWidget EnableLayoutSystemHorizontal(bool value = true)
        {
            this.LayoutGroup.childControlWidth = value;
            return this;
        }

        // Fucking stupid name, but this property does both.
        public VerticalLayoutGroupWidget EnableLayoutSystemVertical(bool value = true)
        {
            this.LayoutGroup.childControlHeight = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public VerticalLayoutGroupWidget SetSpreadChildrenHorizontally(bool value = true)
        {
            this.LayoutGroup.childForceExpandWidth = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public VerticalLayoutGroupWidget SetSpreadChildrenVertically(bool value = true)
        {
            this.LayoutGroup.childForceExpandHeight = value;
            return this;
        }

        public VerticalLayoutGroupWidget SetSpacing(float value)
        {
            this.LayoutGroup.spacing = value;
            return this;
        }

        public VerticalLayoutGroupWidget SetPadding(int left, int top, int right, int bottom)
        {
            this.LayoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public VerticalLayoutGroupWidget SetPadding(int x, int y)
        {
            this.LayoutGroup.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public VerticalLayoutGroupWidget SetPadding(int value)
        {
            this.LayoutGroup.padding = new RectOffset(value, value, value, value);
            return this;
        }
    }
}
