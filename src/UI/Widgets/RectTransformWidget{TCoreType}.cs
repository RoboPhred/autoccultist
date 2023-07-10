namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class RectTransformWidget<TCoreType> : UIGameObjectWidget<TCoreType>
        where TCoreType : RectTransformWidget<TCoreType>
    {
        public RectTransformWidget(string key)
            : this(new GameObject(key))
        {
        }

        public RectTransformWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.RectTransform = this.GameObject.GetOrAddComponent<RectTransform>();
            this.RectTransform.anchorMin = Vector2.zero;
            this.RectTransform.anchorMax = Vector2.one;
            this.RectTransform.offsetMin = Vector2.zero;
            this.RectTransform.offsetMax = Vector2.zero;
        }

        public RectTransform RectTransform { get; private set; }

        public TCoreType Anchor(Vector2 anchor)
        {
            this.RectTransform.anchoredPosition = anchor;
            return this as TCoreType;
        }

        public TCoreType Anchor(float x, float y)
        {
            this.RectTransform.anchoredPosition = new Vector2(x, y);
            return this as TCoreType;
        }

        public TCoreType Left(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(anchor, this.RectTransform.anchorMin.y);
            this.RectTransform.offsetMin = new Vector2(offset, this.RectTransform.offsetMin.y);
            return this as TCoreType;
        }

        public TCoreType Right(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(anchor, this.RectTransform.anchorMax.y);
            this.RectTransform.offsetMax = new Vector2(offset, this.RectTransform.offsetMax.y);
            return this as TCoreType;
        }

        public TCoreType Top(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(this.RectTransform.anchorMax.x, anchor);
            this.RectTransform.offsetMax = new Vector2(this.RectTransform.offsetMax.x, offset);
            return this as TCoreType;
        }

        public TCoreType Bottom(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(this.RectTransform.anchorMin.x, anchor);
            this.RectTransform.offsetMin = new Vector2(this.RectTransform.offsetMin.x, offset);
            return this as TCoreType;
        }

        public TCoreType AnchorRelativeToParent(float left, float top, float right, float bottom)
        {
            this.RectTransform.anchorMin = new Vector2(left, bottom);
            this.RectTransform.anchorMax = new Vector2(right, top);
            return this as TCoreType;
        }

        public TCoreType AnchorRelativeToParent(Vector2 min, Vector2 max)
        {
            this.RectTransform.anchorMin = min;
            this.RectTransform.anchorMax = max;
            return this as TCoreType;
        }

        public TCoreType Offset(float left, float top, float right, float bottom)
        {
            this.RectTransform.offsetMin = new Vector2(left, bottom);
            this.RectTransform.offsetMax = new Vector2(right, top);
            return this as TCoreType;
        }

        public TCoreType Offset(Vector2 min, Vector2 max)
        {
            this.RectTransform.offsetMin = min;
            this.RectTransform.offsetMax = max;
            return this as TCoreType;
        }

        public TCoreType Size(Vector2 size)
        {
            this.RectTransform.sizeDelta = size;
            return this as TCoreType;
        }

        public TCoreType Size(float width, float height)
        {
            this.RectTransform.sizeDelta = new Vector2(width, height);
            return this as TCoreType;
        }

        public TCoreType Pivot(Vector2 pivot)
        {
            this.RectTransform.pivot = pivot;
            return this as TCoreType;
        }

        public TCoreType Pivot(float x, float y)
        {
            this.RectTransform.pivot = new Vector2(x, y);
            return this as TCoreType;
        }
    }
}
