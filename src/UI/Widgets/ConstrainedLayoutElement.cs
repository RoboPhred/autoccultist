namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConstrainedLayoutElement : MonoBehaviour, ILayoutSelfController
    {
        public float MinWidth { get; set; } = -1;

        public float MinHeight { get; set; } = -1;

        public float MaxWidth { get; set; } = -1;

        public float MaxHeight { get; set; } = -1;

        public void SetLayoutHorizontal()
        {
            var bounds = this.GetChildBounds();
            var rt = this.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Math.Max(this.MinWidth, Math.Min(this.MaxWidth, bounds.width)));
        }

        public void SetLayoutVertical()
        {
            var bounds = this.GetChildBounds();
            var rt = this.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(this.MinHeight, Math.Min(this.MaxHeight, bounds.height)));
        }

        private Rect GetChildBounds()
        {
            var bounds = Rect.zero;
            for (var i = 0; i < this.transform.childCount; i++)
            {
                var child = this.transform.GetChild(i);
                var rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var childBounds = new Rect(rect.localPosition.x, rect.localPosition.y, rect.rect.width, rect.rect.height);
                    bounds = Rect.MinMaxRect(
                        Math.Min(bounds.xMin, childBounds.xMin),
                        Math.Min(bounds.yMin, childBounds.yMin),
                        Math.Max(bounds.xMax, childBounds.xMax),
                        Math.Max(bounds.yMax, childBounds.yMax));
                }
            }

            return bounds;
        }
    }
}
