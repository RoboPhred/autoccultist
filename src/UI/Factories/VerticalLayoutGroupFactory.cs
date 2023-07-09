namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class VerticalLayoutGroupFactory : SizingElementFactory<VerticalLayoutGroupFactory>
    {
        private readonly VerticalLayoutGroup layoutGroup;

        public VerticalLayoutGroupFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.layoutGroup = this.GameObject.GetOrAddComponent<VerticalLayoutGroup>();
        }

        public VerticalLayoutGroupFactory ChildControlHeight(bool value)
        {
            this.layoutGroup.childControlHeight = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildControlWidth(bool value)
        {
            this.layoutGroup.childControlWidth = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildForceExpandHeight(bool value)
        {
            this.layoutGroup.childForceExpandHeight = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildForceExpandWidth(bool value)
        {
            this.layoutGroup.childForceExpandWidth = value;
            return this;
        }

        public VerticalLayoutGroupFactory Padding(int left, int top, int right, int bottom)
        {
            this.layoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public VerticalLayoutGroupFactory Padding(int x, int y)
        {
            this.layoutGroup.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public VerticalLayoutGroupFactory Padding(int value)
        {
            this.layoutGroup.padding = new RectOffset(value, value, value, value);
            return this;
        }

        public VerticalLayoutGroupFactory AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.GameObject.transform);
            return this;
        }

        public VerticalLayoutGroupFactory AddContentCentered(Action<Transform> contentFactory)
        {
            var leftSpacer = new GameObject("TopSpacer");
            leftSpacer.transform.SetParent(this.GameObject.transform, false);
            var leftSpacerLayout = leftSpacer.AddComponent<LayoutElement>();
            leftSpacerLayout.flexibleHeight = 1;

            contentFactory(this.GameObject.transform);

            var bottomSpacer = new GameObject("BottomSpacer");
            bottomSpacer.transform.SetParent(this.GameObject.transform, false);
            var rightSpacerLayout = leftSpacer.AddComponent<LayoutElement>();
            rightSpacerLayout.flexibleHeight = 1;

            return this;
        }

        public new VerticalLayoutGroup Build()
        {
            return this.layoutGroup;
        }
    }
}