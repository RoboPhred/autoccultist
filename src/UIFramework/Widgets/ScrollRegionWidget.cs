namespace AutoccultistNS.UI
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScrollRegionWidget : SizingLayoutWidget<ScrollRegionWidget>
    {
        private static readonly ColorBlock ScrollbarColors = new ColorBlock
        {
            normalColor = new Color(0.3765f, 0.7255f, 0.7882f, 1),
            highlightedColor = Color.white,
            pressedColor = new Color(0.5804f, 0.8863f, 0.9373f, 1),
            selectedColor = new Color(0.3765f, 0.7255f, 0.7882f, 1),
            disabledColor = new Color(0.3738f, 0.4914f, 0.5138f, 0.502f),
            colorMultiplier = 1,
            fadeDuration = 0.1f,
        };

        public ScrollRegionWidget(string key)
            : this(new GameObject(key))
        {
        }

        public ScrollRegionWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.SetPivot(0, 1);

            this.ScrollRect = this.GameObject.GetOrAddComponent<ScrollRect>();
            this.ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            this.ScrollRect.horizontal = false;
            this.ScrollRect.vertical = false;
            this.ScrollRect.scrollSensitivity = 5;

            var viewport = new GameObject("Viewport");
            var viewportRt = viewport.AddComponent<RectTransform>();
            this.ScrollRect.viewport = viewportRt;
            viewportRt.SetParent(this.GameObject.transform, false);
            viewportRt.anchoredPosition = Vector2.zero;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.pivot = new Vector2(0, 1);
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            viewport.AddComponent<RectMask2D>();
        }

        public ScrollRect ScrollRect { get; private set; }

        public GameObject ContentGameObject
        {
            get
            {
                this.EnsureContent();
                return this.ScrollRect.content.gameObject;
            }
        }

        public override WidgetMountPoint MountPoint => new WidgetMountPoint(this.ContentGameObject.transform);

        public bool Horizontal
        {
            get
            {
                return this.ScrollRect.horizontal;
            }

            set
            {
                this.SetHorizontal(value);
            }
        }

        public bool Vertical
        {
            get
            {
                return this.ScrollRect.vertical;
            }

            set
            {
                this.SetVertical(value);
            }
        }

        public float Sensitivity
        {
            get
            {
                return this.ScrollRect.scrollSensitivity;
            }

            set
            {
                this.SetSensitivity(value);
            }
        }

        public ScrollRegionWidget SetHorizontal(bool value = true)
        {
            this.ScrollRect.horizontal = value;
            return this;
        }

        public ScrollRegionWidget SetVertical(bool value = true)
        {
            if (this.ScrollRect.vertical == value)
            {
                return this;
            }

            this.ScrollRect.vertical = value;

            if (value)
            {
                var verticalScrollbar = new GameObject("Scrollbar Vertical");

                var handle = new GameObject("Handle");
                var handleRt = handle.AddComponent<RectTransform>();
                handleRt.anchorMax = new Vector2(1, 1);
                handleRt.anchorMin = new Vector2(0, 0);
                handleRt.offsetMin = Vector2.zero;
                handleRt.offsetMax = Vector2.zero;
                handleRt.pivot = new Vector2(0.5f, 0.5f);
                handleRt.SetParent(verticalScrollbar.transform, false);
                var handleImage = handle.AddComponent<Image>();
                handleImage.sprite = ResourceHack.FindSprite("UISprite");
                handleImage.type = Image.Type.Sliced;

                verticalScrollbar.AddComponent<CanvasRenderer>();
                var scrollbarRt = verticalScrollbar.AddComponent<RectTransform>();
                scrollbarRt.anchorMax = new Vector2(1, 1);
                scrollbarRt.anchorMin = new Vector2(1, 0);
                scrollbarRt.offsetMin = new Vector2(-15, 5);
                scrollbarRt.offsetMax = new Vector2(-5, -5);
                scrollbarRt.pivot = new Vector2(1, 1);
                scrollbarRt.SetParent(this.GameObject.transform, false);
                var scrollbarImage = verticalScrollbar.AddComponent<Image>();
                scrollbarImage.sprite = ResourceHack.FindSprite("Background");
                scrollbarImage.type = Image.Type.Sliced;
                scrollbarImage.color = new Color(0, 0, 0, 0.1961f);

                this.ScrollRect.verticalScrollbar = verticalScrollbar.AddComponent<Scrollbar>();
                this.ScrollRect.verticalScrollbar.direction = Scrollbar.Direction.BottomToTop;
                this.ScrollRect.verticalScrollbar.handleRect = handleRt;
                this.ScrollRect.verticalScrollbar.targetGraphic = handleImage;
                this.ScrollRect.verticalScrollbar.colors = ScrollbarColors;

                var viewportRt = this.ScrollRect.viewport.GetComponent<RectTransform>();
                viewportRt.offsetMax = new Vector2(-15, viewportRt.offsetMax.y);

                // Expand the viewport to the size of the content we want to scroll.
                var content = this.ScrollRect.content?.gameObject;
                var sizer = content?.AddComponent<ContentSizeFitter>();
                if (sizer != null)
                {
                    sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
            else
            {
                UnityEngine.Object.Destroy(this.ScrollRect.verticalScrollbar.gameObject);
                this.ScrollRect.verticalScrollbar = null;
                var viewportRt = this.ScrollRect.viewport.GetComponent<RectTransform>();
                viewportRt.offsetMax = new Vector2(0, viewportRt.offsetMax.y);
            }

            return this;
        }

        public ScrollRegionWidget ScrollToHorizontal(float value)
        {
            this.ScrollRect.StartCoroutine(this.JankfestScrollHorizontal(value));
            return this;
        }

        public ScrollRegionWidget ScrollToVertical(float value)
        {
            this.ScrollRect.StartCoroutine(this.JankfestScrollVertical(value));
            return this;
        }

        public ScrollRegionWidget SetSensitivity(float value)
        {
            this.ScrollRect.scrollSensitivity = value;
            return this;
        }

        public override ScrollRegionWidget Clear()
        {
            foreach (Transform child in this.ContentGameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            return this;
        }

        /// <summary>
        /// Overrides the scrolling content directly
        /// This can be used to directly control the scrolled content object, without
        /// going through the default layout wrapper.
        /// </summary>
        public ScrollRegionWidget SetContent(GameObject newContent)
        {
            newContent.transform.SetParent(this.ScrollRect.viewport.gameObject.transform, false);
            this.ScrollRect.content = newContent.GetComponent<RectTransform>();
            return this;
        }

        private IEnumerator JankfestScrollHorizontal(float value)
        {
            // Unity's UI is a total jankfest, and it wants to scroll to a random position somewhere in the middle
            // whenever it opens.
            // We have to wait for that jank scroll to happen in order to override it.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Canvas.ForceUpdateCanvases();

            this.ScrollRect.StopMovement();
            this.ScrollRect.horizontalNormalizedPosition = value;
            if (this.ScrollRect.horizontalScrollbar != null)
            {
                this.ScrollRect.horizontalScrollbar.value = value;
            }

            Canvas.ForceUpdateCanvases();
        }

        private IEnumerator JankfestScrollVertical(float value)
        {
            // Unity's UI is a total jankfest, and it wants to scroll to a random position somewhere in the middle
            // whenever it opens.
            // We have to wait for that jank scroll to happen in order to override it.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Canvas.ForceUpdateCanvases();

            this.ScrollRect.StopMovement();
            this.ScrollRect.verticalNormalizedPosition = value;
            if (this.ScrollRect.verticalScrollbar != null)
            {
                this.ScrollRect.verticalScrollbar.value = value;
            }

            Canvas.ForceUpdateCanvases();
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

            var contentRt = content.GetOrAddComponent<RectTransform>();
            contentRt.anchoredPosition = new Vector2(0, 0);
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(0, 0);

            // Expand the viewport to the size of the content we want to scroll.
            var sizer = content.AddComponent<ContentSizeFitter>();

            // Shove a blank image in so we capture mouse events for mouse drag scrolling
            var image = content.GetOrAddComponent<Image>();
            image.sprite = ResourcesManager.GetSpriteForUI("empty_bg");
            image.color = new Color(1, 1, 1, 1);

            if (this.ScrollRect.horizontal && this.ScrollRect.vertical)
            {
                sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizer.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else if (this.ScrollRect.vertical)
            {
                sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // FIXME: Items are centering in this.  Implicit property of LayoutGroups?
                var group = content.AddComponent<VerticalLayoutGroup>();
                group.spacing = 0;
                group.childControlHeight = true;
                group.childControlWidth = true;
                group.childForceExpandHeight = false;
                group.childForceExpandWidth = true;
            }
            else if (this.ScrollRect.horizontal)
            {
                sizer.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                var group = content.AddComponent<HorizontalLayoutGroup>();
                group.spacing = 0;
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = false;
                group.childForceExpandHeight = true;
            }

            this.SetContent(content);
        }
    }
}
