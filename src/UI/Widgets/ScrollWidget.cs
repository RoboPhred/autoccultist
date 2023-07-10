namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScrollWidget : SizingLayoutWidget<ScrollWidget>
    {
        public ScrollWidget(string key)
            : this(new GameObject(key))
        {
        }

        public ScrollWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.ScrollRect = this.GameObject.GetOrAddComponent<ScrollRect>();
            this.ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            this.ScrollRect.horizontal = false;
            this.ScrollRect.vertical = false;
            this.ScrollRect.scrollSensitivity = 20;

            var viewport = new GameObject("Viewport");
            var viewportRt = viewport.AddComponent<RectTransform>();
            this.ScrollRect.viewport = viewportRt;
            viewportRt.SetParent(this.GameObject.transform, false);
            viewportRt.anchoredPosition = new Vector2(0, 0);
            viewportRt.anchorMin = new Vector2(0, 0);
            viewportRt.anchorMax = new Vector2(1, 1);
            viewportRt.pivot = new Vector2(0, 0);
            viewportRt.offsetMin = new Vector2(0, 0);
            viewportRt.offsetMax = new Vector2(0, 0);

            viewport.AddComponent<RectMask2D>();

            // Size ourselves to fit our contents.
            var fitter = this.Content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public ScrollRect ScrollRect { get; private set; }

        public GameObject Content
        {
            get
            {
                this.EnsureContent();
                return this.ScrollRect.content.gameObject;
            }
        }

        public ScrollWidget Horizontal()
        {
            this.ScrollRect.horizontal = true;
            return this;
        }

        public ScrollWidget Vertical()
        {
            this.ScrollRect.vertical = true;
            return this;
        }

        public ScrollWidget Sensitivity(float value)
        {
            this.ScrollRect.scrollSensitivity = value;
            return this;
        }

        /// <summary>
        /// Allows adding children to the scrolled area.
        /// For best results, Horizontal() or Vertical() should be called before this.
        /// </summary>
        public ScrollWidget AddContent(Action<Transform> contentFactory)
        {
            contentFactory(this.Content.transform);
            return this;
        }

        /// <summary>
        /// Overrides the scrolling content directly
        /// This can be used to directly control the scrolled content object, without
        /// going through the default layout wrapper.
        /// </summary>
        public ScrollWidget SetContent(GameObject newContent)
        {
            newContent.transform.SetParent(this.ScrollRect.viewport.gameObject.transform, false);
            this.ScrollRect.content = newContent.GetComponent<RectTransform>();
            return this;
        }

        private void EnsureContent()
        {
            var content = this.ScrollRect.content?.gameObject;
            if (content != null)
            {
                return;
            }

            // Assume vertical scrolling by default.
            if (!this.ScrollRect.vertical && !this.ScrollRect.horizontal)
            {
                this.ScrollRect.vertical = true;
            }

            content = new GameObject("Content");
            if (this.ScrollRect.vertical)
            {
                var group = content.AddComponent<VerticalLayoutGroup>();
                group.childControlHeight = true;
                group.childForceExpandWidth = true;
            }
            else if (this.ScrollRect.horizontal)
            {
                var group = content.AddComponent<HorizontalLayoutGroup>();
                group.childControlWidth = true;
                group.childForceExpandHeight = true;
            }

            var contentRt = content.GetOrAddComponent<RectTransform>();
            contentRt.anchoredPosition = new Vector2(0, 0);
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 0);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(0, 0);

            this.SetContent(content);
        }
    }
}
