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
            this.ChildControlHeight(true);
            this.ChildControlWidth(true);
        }

        public HorizontalLayoutGroup LayoutGroup { get; private set; }

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

        public HorizontalLayoutGroupWidget AddContentCentered(Action<Transform> contentFactory)
        {
            var leftSpacer = new GameObject("LeftSpacer");
            leftSpacer.transform.SetParent(this.GameObject.transform, false);
            var leftSpacerLayout = leftSpacer.AddComponent<LayoutElement>();
            leftSpacerLayout.flexibleWidth = 1;

            contentFactory(this.GameObject.transform);

            var rightSpacer = new GameObject("RightSpacer");
            rightSpacer.transform.SetParent(this.GameObject.transform, false);
            var rightSpacerLayout = leftSpacer.AddComponent<LayoutElement>();
            rightSpacerLayout.flexibleWidth = 1;

            return this;
        }
    }
}
