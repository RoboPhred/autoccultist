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
            this.LayoutGroup = this.GameObject.GetOrAddComponent<HorizontalLayoutGroup>();
            this.ChildAlignment(TextAnchor.UpperLeft);
            this.Spacing(0);
            this.ChildControlWidth(true);
            this.ChildControlHeight(true);
            this.ChildForceExpandWidth(false);
            this.ChildForceExpandHeight(false);
        }

        public HorizontalLayoutGroup LayoutGroup { get; private set; }

        public HorizontalLayoutGroupWidget ChildAlignment(TextAnchor value)
        {
            this.LayoutGroup.childAlignment = value;
            return this;
        }

        public HorizontalLayoutGroupWidget ChildControlHeight(bool value)
        {
            this.LayoutGroup.childControlHeight = value;
            return this;
        }

        public HorizontalLayoutGroupWidget ChildControlWidth(bool value)
        {
            this.LayoutGroup.childControlWidth = value;
            return this;
        }

        public HorizontalLayoutGroupWidget ChildForceExpandHeight(bool value)
        {
            this.LayoutGroup.childForceExpandHeight = value;
            return this;
        }

        public HorizontalLayoutGroupWidget ChildForceExpandWidth(bool value)
        {
            this.LayoutGroup.childForceExpandWidth = value;
            return this;
        }

        public HorizontalLayoutGroupWidget Spacing(float value)
        {
            this.LayoutGroup.spacing = value;
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int left, int top, int right, int bottom)
        {
            this.LayoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int x, int y)
        {
            this.LayoutGroup.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public HorizontalLayoutGroupWidget Padding(int value)
        {
            this.LayoutGroup.padding = new RectOffset(value, value, value, value);
            return this;
        }

        public HorizontalLayoutGroupWidget AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.GameObject.transform);
            return this;
        }
    }
}
