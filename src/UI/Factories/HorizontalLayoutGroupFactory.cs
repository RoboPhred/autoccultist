namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class HorizontalLayoutGroupFactory : SizingElementFactory<HorizontalLayoutGroupFactory>
    {
        private readonly HorizontalLayoutGroup layoutGroup;

        public HorizontalLayoutGroupFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.layoutGroup = this.GameObject.GetOrAddComponent<HorizontalLayoutGroup>();
        }

        public HorizontalLayoutGroupFactory ChildControlHeight(bool value)
        {
            this.layoutGroup.childControlHeight = value;
            return this;
        }

        public HorizontalLayoutGroupFactory ChildControlWidth(bool value)
        {
            this.layoutGroup.childControlWidth = value;
            return this;
        }

        public HorizontalLayoutGroupFactory ChildForceExpandHeight(bool value)
        {
            this.layoutGroup.childForceExpandHeight = value;
            return this;
        }

        public HorizontalLayoutGroupFactory ChildForceExpandWidth(bool value)
        {
            this.layoutGroup.childForceExpandWidth = value;
            return this;
        }
        public HorizontalLayoutGroupFactory Padding(int left, int top, int right, int bottom)
        {
            this.layoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public HorizontalLayoutGroupFactory Padding(int x, int y)
        {
            this.layoutGroup.padding = new RectOffset(x, x, y, y);
            return this;
        }

        public HorizontalLayoutGroupFactory Padding(int value)
        {
            this.layoutGroup.padding = new RectOffset(value, value, value, value);
            return this;
        }

        public HorizontalLayoutGroupFactory AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.GameObject.transform);
            return this;
        }

        public HorizontalLayoutGroupFactory AddContentCentered(Action<Transform> contentFactory)
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

        public new HorizontalLayoutGroup Build()
        {
            return this.layoutGroup;
        }
    }
}
