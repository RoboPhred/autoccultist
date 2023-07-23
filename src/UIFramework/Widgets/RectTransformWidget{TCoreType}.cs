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

        public TCoreType SetAnchor(Vector3 anchor)
        {
            this.RectTransform.anchoredPosition3D = anchor;
            return this as TCoreType;
        }

        public TCoreType SetAnchor(Vector2 anchor)
        {
            this.RectTransform.anchoredPosition = anchor;
            return this as TCoreType;
        }

        public TCoreType SetAnchor(float x, float y)
        {
            this.RectTransform.anchoredPosition = new Vector2(x, y);
            return this as TCoreType;
        }

        public TCoreType SetOffset(float left, float top, float right, float bottom)
        {
            this.RectTransform.offsetMin = new Vector2(left, bottom);
            this.RectTransform.offsetMax = new Vector2(right, top);
            return this as TCoreType;
        }

        public TCoreType SetOffset(Vector2 min, Vector2 max)
        {
            this.RectTransform.offsetMin = min;
            this.RectTransform.offsetMax = max;
            return this as TCoreType;
        }

        public TCoreType SetSize(Vector2 size)
        {
            this.RectTransform.sizeDelta = size;
            return this as TCoreType;
        }

        public TCoreType SetSize(float width, float height)
        {
            this.RectTransform.sizeDelta = new Vector2(width, height);
            return this as TCoreType;
        }

        public TCoreType SetPivot(Vector2 pivot)
        {
            this.RectTransform.pivot = pivot;
            return this as TCoreType;
        }

        public TCoreType SetPivot(float x, float y)
        {
            this.RectTransform.pivot = new Vector2(x, y);
            return this as TCoreType;
        }

        public TCoreType SetLeft(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(anchor, this.RectTransform.anchorMin.y);
            this.RectTransform.offsetMin = new Vector2(offset, this.RectTransform.offsetMin.y);
            return this as TCoreType;
        }

        public TCoreType SetRight(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(anchor, this.RectTransform.anchorMax.y);
            this.RectTransform.offsetMax = new Vector2(offset, this.RectTransform.offsetMax.y);
            return this as TCoreType;
        }

        public TCoreType SetTop(float anchor, float offset)
        {
            this.RectTransform.anchorMax = new Vector2(this.RectTransform.anchorMax.x, anchor);
            this.RectTransform.offsetMax = new Vector2(this.RectTransform.offsetMax.x, offset);
            return this as TCoreType;
        }

        public TCoreType SetBottom(float anchor, float offset)
        {
            this.RectTransform.anchorMin = new Vector2(this.RectTransform.anchorMin.x, anchor);
            this.RectTransform.offsetMin = new Vector2(this.RectTransform.offsetMin.x, offset);
            return this as TCoreType;
        }
    }
}
