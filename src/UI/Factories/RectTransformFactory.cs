namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class RectTransformFactory : UIGameObjectFactory<RectTransformFactory>
    {
        private readonly RectTransform rectTransform;

        public RectTransformFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.rectTransform = this.gameObject.GetOrAddComponent<RectTransform>();
        }

        public RectTransformFactory Anchor(Vector2 anchor)
        {
            this.rectTransform.anchoredPosition = anchor;
            return this;
        }

        public RectTransformFactory Anchor(float x, float y)
        {
            this.rectTransform.anchoredPosition = new Vector2(x, y);
            return this;
        }

        public RectTransformFactory AnchorRelativeToParent(float left, float top, float right, float bottom)
        {
            this.rectTransform.anchorMin = new Vector2(left, bottom);
            this.rectTransform.anchorMax = new Vector2(right, top);
            return this;
        }

        public RectTransformFactory AnchorRelativeToParent(Vector2 min, Vector2 max)
        {
            this.rectTransform.anchorMin = min;
            this.rectTransform.anchorMax = max;
            return this;
        }

        public RectTransformFactory Offset(float left, float top, float right, float bottom)
        {
            this.rectTransform.offsetMin = new Vector2(left, bottom);
            this.rectTransform.offsetMax = new Vector2(right, top);
            return this;
        }

        public RectTransformFactory Offset(Vector2 min, Vector2 max)
        {
            this.rectTransform.offsetMin = min;
            this.rectTransform.offsetMax = max;
            return this;
        }

        public RectTransformFactory Size(Vector2 size)
        {
            this.rectTransform.sizeDelta = size;
            return this;
        }

        public RectTransformFactory Size(float width, float height)
        {
            this.rectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public RectTransformFactory Pivot(Vector2 pivot)
        {
            this.rectTransform.pivot = pivot;
            return this;
        }

        public RectTransformFactory Pivot(float x, float y)
        {
            this.rectTransform.pivot = new Vector2(x, y);
            return this;
        }

        public new RectTransform Build()
        {
            return this.rectTransform;
        }
    }
}
