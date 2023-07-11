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
            this.Spacing(0);
            this.ChildControlHeight(true);
            this.ChildControlWidth(true);
            this.ChildForceExpandWidth(false);
            this.ChildForceExpandHeight(false);
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

        public TCoreType Spacing(float value)
        {
            this.LayoutGroup.spacing = value;
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

        public TCoreType AddExpandingSpacer()
        {
            var spacer = new SizingLayoutWidget("Spacer")
                .ExpandHeight();

            this.AddContent(spacer);
            return this as TCoreType;
        }

        public TCoreType AddSpacer(int height)
        {
            var spacer = new SizingLayoutWidget("Spacer");
            spacer.PreferredHeight(height);
            this.AddContent(spacer);
            return this as TCoreType;
        }

        public TCoreType AddContent(GameObject gameObject)
        {
            gameObject.transform.SetParent(this.GameObject.transform, false);
            return this as TCoreType;
        }

        public TCoreType AddContent(Action<Transform> transform)
        {
            transform(this.GameObject.transform);
            return this as TCoreType;
        }
    }
}
