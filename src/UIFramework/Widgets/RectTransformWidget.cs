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

        public float AnchorTop
        {
            get
            {
                return this.RectTransform.anchorMax.y;
            }

            set
            {
                this.RectTransform.anchorMax = new Vector2(this.RectTransform.anchorMax.x, value);
            }
        }

        public float AnchorBottom
        {
            get
            {
                return this.RectTransform.anchorMin.y;
            }

            set
            {
                this.RectTransform.anchorMin = new Vector2(this.RectTransform.anchorMin.x, value);
            }
        }

        public float AnchorLeft
        {
            get
            {
                return this.RectTransform.anchorMin.x;
            }

            set
            {
                this.RectTransform.anchorMin = new Vector2(value, this.RectTransform.anchorMin.y);
            }
        }

        public float AnchorRight
        {
            get
            {
                return this.RectTransform.anchorMax.x;
            }

            set
            {
                this.RectTransform.anchorMax = new Vector2(value, this.RectTransform.anchorMax.y);
            }
        }

        public float OffsetTop
        {
            get
            {
                return this.RectTransform.offsetMax.y;
            }

            set
            {
                this.RectTransform.offsetMax = new Vector2(this.RectTransform.offsetMax.x, value);
            }
        }

        public float OffsetBottom
        {
            get
            {
                return this.RectTransform.offsetMin.y;
            }

            set
            {
                this.RectTransform.offsetMin = new Vector2(this.RectTransform.offsetMin.x, value);
            }
        }

        public float OffsetLeft
        {
            get
            {
                return this.RectTransform.offsetMin.x;
            }

            set
            {
                this.RectTransform.offsetMin = new Vector2(value, this.RectTransform.offsetMin.y);
            }
        }

        public float OffsetRight
        {
            get
            {
                return this.RectTransform.offsetMax.x;
            }

            set
            {
                this.RectTransform.offsetMax = new Vector2(value, this.RectTransform.offsetMax.y);
            }
        }

        public Vector2 Size
        {
            get
            {
                return this.RectTransform.sizeDelta;
            }

            set
            {
                this.RectTransform.sizeDelta = value;
            }
        }

        public RectTransformWidget SetAnchor(Vector3 anchor)
        {
            this.RectTransform.anchoredPosition3D = anchor;
            return this;
        }

        public RectTransformWidget SetAnchor(Vector2 anchor)
        {
            this.RectTransform.anchoredPosition = anchor;
            return this;
        }

        public RectTransformWidget SetAnchor(float x, float y)
        {
            this.RectTransform.anchoredPosition = new Vector2(x, y);
            return this;
        }

        public RectTransformWidget SetOffset(float left, float top, float right, float bottom)
        {
            this.RectTransform.offsetMin = new Vector2(left, bottom);
            this.RectTransform.offsetMax = new Vector2(right, top);
            return this;
        }

        public RectTransformWidget SetOffset(Vector2 min, Vector2 max)
        {
            this.RectTransform.offsetMin = min;
            this.RectTransform.offsetMax = max;
            return this;
        }

        public RectTransformWidget SetSize(Vector2 size)
        {
            this.RectTransform.sizeDelta = size;
            return this;
        }

        public RectTransformWidget SetSize(float width, float height)
        {
            this.RectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public RectTransformWidget SetPivot(Vector2 pivot)
        {
            this.RectTransform.pivot = pivot;
            return this;
        }

        public RectTransformWidget SetPivot(float x, float y)
        {
            this.RectTransform.pivot = new Vector2(x, y);
            return this;
        }

        public RectTransformWidget SetLeft(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(anchor, this.RectTransform.anchorMin.y);
            this.RectTransform.offsetMin = new Vector2(offset, this.RectTransform.offsetMin.y);
            return this;
        }

        public RectTransformWidget SetRight(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(anchor, this.RectTransform.anchorMax.y);
            this.RectTransform.offsetMax = new Vector2(offset, this.RectTransform.offsetMax.y);
            return this;
        }

        public RectTransformWidget SetTop(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(this.RectTransform.anchorMax.x, anchor);
            this.RectTransform.offsetMax = new Vector2(this.RectTransform.offsetMax.x, offset);
            return this;
        }

        public RectTransformWidget SetBottom(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(this.RectTransform.anchorMin.x, anchor);
            this.RectTransform.offsetMin = new Vector2(this.RectTransform.offsetMin.x, offset);
            return this;
        }
    }
}
