namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class RectTransformWidget : UIGameObjectWidget<RectTransformWidget>
    {
        public RectTransformWidget(string key)
            : this(new GameObject(key))
        {
        }

        public RectTransformWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.RectTransform = this.GameObject.GetOrAddComponent<RectTransform>();
        }

        public RectTransform RectTransform { get; private set; }

        public static implicit operator RectTransform(RectTransformWidget widget)
        {
            return widget.RectTransform;
        }

        public RectTransformWidget Anchor(Vector2 anchor)
        {
            this.RectTransform.anchoredPosition = anchor;
            return this;
        }

        public RectTransformWidget Anchor(float x, float y)
        {
            this.RectTransform.anchoredPosition = new Vector2(x, y);
            return this;
        }

        public RectTransformWidget AnchorRelativeToParent(float left, float top, float right, float bottom)
        {
            this.RectTransform.anchorMin = new Vector2(left, bottom);
            this.RectTransform.anchorMax = new Vector2(right, top);
            return this;
        }

        public RectTransformWidget AnchorRelativeToParent(Vector2 min, Vector2 max)
        {
            this.RectTransform.anchorMin = min;
            this.RectTransform.anchorMax = max;
            return this;
        }

        public RectTransformWidget Offset(float left, float top, float right, float bottom)
        {
            this.RectTransform.offsetMin = new Vector2(left, bottom);
            this.RectTransform.offsetMax = new Vector2(right, top);
            return this;
        }

        public RectTransformWidget Offset(Vector2 min, Vector2 max)
        {
            this.RectTransform.offsetMin = min;
            this.RectTransform.offsetMax = max;
            return this;
        }

        public RectTransformWidget Size(Vector2 size)
        {
            this.RectTransform.sizeDelta = size;
            return this;
        }

        public RectTransformWidget Size(float width, float height)
        {
            this.RectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public RectTransformWidget Pivot(Vector2 pivot)
        {
            this.RectTransform.pivot = pivot;
            return this;
        }

        public RectTransformWidget Pivot(float x, float y)
        {
            this.RectTransform.pivot = new Vector2(x, y);
            return this;
        }
    }
}
