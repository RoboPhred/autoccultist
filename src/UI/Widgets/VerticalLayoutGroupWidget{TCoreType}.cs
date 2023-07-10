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
        }

        public VerticalLayoutGroup LayoutGroup { get; private set; }

        public TCoreType ChildControlHeight(bool value)
        {
            this.LayoutGroup.childControlHeight = value;
            return this as TCoreType;
        }

        public TCoreType ChildControlWidth(bool value)
        {
            this.LayoutGroup.childControlWidth = value;
            return this as TCoreType;
        }

        public TCoreType ChildForceExpandHeight(bool value)
        {
            this.LayoutGroup.childForceExpandHeight = value;
            return this as TCoreType;
        }

        public TCoreType ChildForceExpandWidth(bool value)
        {
            this.LayoutGroup.childForceExpandWidth = value;
            return this as TCoreType;
        }

        public TCoreType Padding(int left, int top, int right, int bottom)
        {
            this.LayoutGroup.padding = new RectOffset(left, right, top, bottom);
            return this as TCoreType;
        }

        public TCoreType Padding(int x, int y)
        {
            this.LayoutGroup.padding = new RectOffset(x, x, y, y);
            return this as TCoreType;
        }

        public TCoreType Padding(int value)
        {
            this.LayoutGroup.padding = new RectOffset(value, value, value, value);
            return this as TCoreType;
        }

        public TCoreType AddContent(GameObject gameObject)
        {
            return this.AddContent(transform => gameObject.transform.SetParent(transform, false));
        }

        public TCoreType AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.GameObject.transform);
            return this as TCoreType;
        }

        public TCoreType AddContentCentered(GameObject gameObject)
        {
            return this.AddContentCentered(transform => gameObject.transform.SetParent(transform, false));
        }

        public TCoreType AddContentCentered(Action<Transform> contentFactory)
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

            return this as TCoreType;
        }
    }
}
