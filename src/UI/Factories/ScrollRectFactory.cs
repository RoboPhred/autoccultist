namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScrollRectFactory : SizingElementFactory<ScrollRectFactory>
    {
        private readonly ScrollRect scrollRect;

        private readonly GameObject content;

        private readonly VerticalLayoutGroup group;

        public ScrollRectFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.scrollRect = this.GameObject.GetOrAddComponent<ScrollRect>();
            this.scrollRect.movementType = ScrollRect.MovementType.Elastic;
            this.scrollRect.horizontal = false;
            this.scrollRect.vertical = false;
            this.scrollRect.scrollSensitivity = 10;

            var viewport = new GameObject("Viewport");
            var viewportRt = viewport.AddComponent<RectTransform>();
            this.scrollRect.viewport = viewportRt;
            viewportRt.SetParent(this.GameObject.transform, false);
            viewportRt.anchoredPosition = new Vector2(0, 0);
            viewportRt.anchorMin = new Vector2(0, 0);
            viewportRt.anchorMax = new Vector2(1, 1);
            viewportRt.pivot = new Vector2(0, 0);
            viewportRt.offsetMin = new Vector2(0, 0);
            viewportRt.offsetMax = new Vector2(0, 0);

            viewport.AddComponent<RectMask2D>();

            this.content = new GameObject("Content");
            this.content.transform.SetParent(viewport.transform, false);

            this.scrollRect.content = this.content.AddComponent<RectTransform>();
            this.scrollRect.content.anchoredPosition = new Vector2(0, 0);
            this.scrollRect.content.anchorMin = new Vector2(0, 0);
            this.scrollRect.content.anchorMax = new Vector2(1, 1);
            this.scrollRect.content.pivot = new Vector2(0, 0);
            this.scrollRect.content.offsetMin = new Vector2(0, 0);
            this.scrollRect.content.offsetMax = new Vector2(0, 0);

            // Stack items in the viewport vertically.
            this.group = this.content.AddComponent<VerticalLayoutGroup>();

            // Size ourselves to fit our contents.
            var fitter = this.content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public ScrollRectFactory Horizontal()
        {
            this.group.childControlWidth = true;
            this.group.childForceExpandWidth = true;
            this.scrollRect.horizontal = true;
            return this;
        }

        public ScrollRectFactory Vertical()
        {
            this.group.childControlHeight = true;
            this.group.childForceExpandHeight = true;
            this.scrollRect.vertical = true;
            return this;
        }

        public ScrollRectFactory Sensitivity(float value)
        {
            this.scrollRect.scrollSensitivity = value;
            return this;
        }

        public ScrollRectFactory AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.content.transform);
            return this;
        }

        public new ScrollRect Build()
        {
            return this.scrollRect;
        }
    }
}
