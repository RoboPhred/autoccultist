namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class RectTransformFactory<TFactory> : UIGameObjectFactory<TFactory>
        where TFactory : RectTransformFactory<TFactory>
    {
        private readonly RectTransform rectTransform;

        public RectTransformFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.rectTransform = this.gameObject.AddComponent<RectTransform>();
        }

        public TFactory Anchor(Vector2 anchor)
        {
            this.rectTransform.anchoredPosition = anchor;
            return this as TFactory;
        }

        public TFactory Anchor(float x, float y)
        {
            this.rectTransform.anchoredPosition = new Vector2(x, y);
            return this as TFactory;
        }

        public TFactory AnchorRelativeToParent(float left, float top, float right, float bottom)
        {
            this.rectTransform.anchorMin = new Vector2(left, bottom);
            this.rectTransform.anchorMax = new Vector2(right, top);
            return this as TFactory;
        }

        public TFactory AnchorRelativeToParent(Vector2 min, Vector2 max)
        {
            this.rectTransform.anchorMin = min;
            this.rectTransform.anchorMax = max;
            return this as TFactory;
        }

        public TFactory Offset(float left, float top, float right, float bottom)
        {
            this.rectTransform.offsetMin = new Vector2(left, bottom);
            this.rectTransform.offsetMax = new Vector2(right, top);
            return this as TFactory;
        }

        public TFactory Offset(Vector2 min, Vector2 max)
        {
            this.rectTransform.offsetMin = min;
            this.rectTransform.offsetMax = max;
            return this as TFactory;
        }

        public TFactory Size(Vector2 size)
        {
            this.rectTransform.sizeDelta = size;
            return this as TFactory;
        }

        public TFactory Size(float width, float height)
        {
            this.rectTransform.sizeDelta = new Vector2(width, height);
            return this as TFactory;
        }

        public TFactory Pivot(Vector2 pivot)
        {
            this.rectTransform.pivot = pivot;
            return this as TFactory;
        }

        public TFactory Pivot(float x, float y)
        {
            this.rectTransform.pivot = new Vector2(x, y);
            return this as TFactory;
        }

        public new RectTransform Build()
        {
            return this.rectTransform;
        }
    }
}
