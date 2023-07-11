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
            this.Spacing(0);
            this.ChildControlHeight(true);
            this.ChildControlWidth(true);
            this.ChildForceExpandWidth(false);
            this.ChildForceExpandHeight(false);
        }

        public VerticalLayoutGroup LayoutGroup { get; private set; }

        public VerticalLayoutGroupWidget ChildControlHeight(bool value)
        {
            this.LayoutGroup.childControlHeight = value;
            return this;
        }

        public VerticalLayoutGroupWidget ChildControlWidth(bool value)
        {
            this.LayoutGroup.childControlWidth = value;
            return this;
        }

        public VerticalLayoutGroupWidget ChildForceExpandHeight(bool value)
        {
            this.LayoutGroup.childForceExpandHeight = value;
            return this;
        }

        public VerticalLayoutGroupWidget ChildForceExpandWidth(bool value)
        {
            this.LayoutGroup.childForceExpandWidth = value;
            return this;
        }

        public VerticalLayoutGroupWidget Spacing(float value)
        {
            this.LayoutGroup.spacing = value;
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int left, int top, int right, int bottom)
        {
            this.LayoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int x, int y)
        {
            this.LayoutGroup.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public VerticalLayoutGroupWidget Padding(int value)
        {
            this.LayoutGroup.padding = new RectOffset(value, value, value, value);
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
