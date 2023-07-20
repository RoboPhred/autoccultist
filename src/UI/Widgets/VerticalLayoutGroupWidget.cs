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
            this.LayoutGroupBehavior = this.GameObject.GetOrAddComponent<VerticalLayoutGroup>();
            this.Spacing(0);

            // These absolutely must be on, or all our children will be zero-sized.
            // Yes, this also fucks up our own size.  Too bad.
            this.EnableChildLayoutWidthAndBreakSelfSizing(true);
            this.EnableChildLayoutHeightAndBreakSelfSizing(true);

            this.SpreadChildrenHorizontally(false);
            this.SpreadChildrenVertically(false);
        }

        public VerticalLayoutGroup LayoutGroupBehavior { get; private set; }

        // Fucking stupid name, but this property does both.
        public VerticalLayoutGroupWidget EnableChildLayoutWidthAndBreakSelfSizing(bool value = true)
        {
            this.LayoutGroupBehavior.childControlWidth = value;
            return this;
        }

        // Fucking stupid name, but this property does both.
        public VerticalLayoutGroupWidget EnableChildLayoutHeightAndBreakSelfSizing(bool value = true)
        {
            this.LayoutGroupBehavior.childControlHeight = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public VerticalLayoutGroupWidget SpreadChildrenHorizontally(bool value = true)
        {
            this.LayoutGroupBehavior.childForceExpandWidth = value;
            return this;
        }

        // what the fuck does this even do.  It centers things but leaves other things zero sized.
        public VerticalLayoutGroupWidget SpreadChildrenVertically(bool value = true)
        {
            this.LayoutGroupBehavior.childForceExpandHeight = value;
            return this;
        }

        public VerticalLayoutGroupWidget Spacing(float value)
        {
            this.LayoutGroupBehavior.spacing = value;
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int left, int top, int right, int bottom)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int x, int y)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int value)
        {
            this.LayoutGroupBehavior.padding = new RectOffset(value, value, value, value);
            return this;
        }

        public VerticalLayoutGroupWidget AddExpandingSpacer()
        {
            var spacer = new SizingLayoutWidget("Spacer")
                .ExpandHeight();

            this.AddContent(spacer);
            return this;
        }

        public VerticalLayoutGroupWidget AddSpacer(int height)
        {
            var spacer = new SizingLayoutWidget("Spacer");
            spacer.PreferredHeight(height);
            this.AddContent(spacer);
            return this;
        }

        public VerticalLayoutGroupWidget AddContent(GameObject gameObject)
        {
            gameObject.transform.SetParent(this.GameObject.transform, false);
            return this;
        }

        public VerticalLayoutGroupWidget AddContent(Action<WidgetMountPoint> factory)
        {
            factory(new WidgetMountPoint(this.GameObject.transform));
            return this;
        }
    }
}
