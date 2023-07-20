namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class HorizontalLayoutGroupWidget : SizingLayoutWidget<HorizontalLayoutGroupWidget>
    {
        public HorizontalLayoutGroupWidget(string key)
            : this(new GameObject(key))
        {
        }

        public HorizontalLayoutGroupWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.LayoutGroupBehavior = this.GameObject.GetOrAddComponent<HorizontalLayoutGroup>();
            this.ChildAlignment(TextAnchor.UpperLeft);
            this.Spacing(0);

            // These absolutely must be on, or all our children will be zero-sized.
            // Yes, this also fucks up our own size.  Too bad.
            this.EnableChildLayoutWidthAndBreakSelfSizing(true);
            this.EnableChildLayoutHeightAndBreakSelfSizing(true);

            this.SpreadChildrenVertically(false);
            this.SpreadChildrenHorizontally(false);
        }

        public HorizontalLayoutGroup LayoutGroupBehavior { get; private set; }

        public HorizontalLayoutGroupWidget ChildAlignment(TextAnchor value)
        {
            this.LayoutGroupBehavior.childAlignment = value;
            return this;
        }

        // Fucking stupid name, but this property does both.
        public HorizontalLayoutGroupWidget EnableChildLayoutWidthAndBreakSelfSizing(bool value)
        {
            this.LayoutGroupBehavior.childControlWidth = value;
            return this;
        }

        // Fucking stupid name, but this property does both.
        public HorizontalLayoutGroupWidget EnableChildLayoutHeightAndBreakSelfSizing(bool value)
        {
            this.LayoutGroupBehavior.childControlHeight = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public HorizontalLayoutGroupWidget SpreadChildrenHorizontally(bool value = true)
        {
            this.LayoutGroupBehavior.childForceExpandWidth = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public HorizontalLayoutGroupWidget SpreadChildrenVertically(bool value = true)
        {
            this.LayoutGroupBehavior.childForceExpandHeight = value;
            return this;
        }

        public HorizontalLayoutGroupWidget Spacing(float value)
        {
            this.LayoutGroupBehavior.spacing = value;
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int left, int top, int right, int bottom)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int x, int y)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int value)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(value, value, value, value);
            return this;
        }

        public HorizontalLayoutGroupWidget AddContent(Action<WidgetMountPoint> contentFactory)
        {
            contentFactory(new WidgetMountPoint(this.GameObject.transform));
            return this;
        }
    }
}
