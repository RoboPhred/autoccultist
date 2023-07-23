namespace AutoccultistNS.UI
{
    using System;
    using SecretHistories.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class AbstractWindow : MonoBehaviour
    {
        private static readonly Color BgColorHeader = new Color(0.0314f, 0.0706f, 0.1059f, 0.902f);
        private static readonly Color BgColorBody = new Color(0.0627f, 0.1333f, 0.1922f, 0.902f);
        private static readonly Color BgColorFooter = new Color(0.0029f, 0.0246f, 0.0441f, 0.902f);

        private CanvasGroupFader canvasGroupFader;
        private WindowPositioner positioner;
        private SizingLayoutWidget root;
        private TextWidget title;

        public AbstractWindow()
        {
            this.BuildWindowFrame();
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public Vector3 Position
        {
            get => this.positioner.GetPosition();
            set => this.positioner.SetPosition(value);
        }

        public string Title
        {
            get => this.title.Text;
            set => this.title.Text = value;
        }

        public bool IsOpen => this.canvasGroupFader.IsFullyVisible();

        public bool IsVisible => this.canvasGroupFader.IsFullyVisible() || this.canvasGroupFader.IsAppearing();

        public bool IsClosed => this.canvasGroupFader.IsInvisible();

        protected virtual int DefaultWidth => 600;

        protected virtual int DefaultHeight => 400;

        protected WidgetMountPoint Icon { get; private set; }

        protected WidgetMountPoint Content { get; private set; }

        protected WidgetMountPoint Footer { get; private set; }

        public static T CreateTabletopWindow<T>(string key)
            where T : AbstractWindow
        {
            var mountPoint = MountPoints.TabletopWindowLayer;
            if (mountPoint == null)
            {
                throw new Exception("Cannot find Tabletop window mount point.");
            }

            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(mountPoint, false);
            return gameObject.AddComponent<T>();
        }

        public static T CreateMetaWindow<T>(string key)
            where T : AbstractWindow
        {
            var mountPoint = MountPoints.MetaWindowLayer;
            if (mountPoint == null)
            {
                throw new Exception("Cannot find CanvasMeta.");
            }

            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(mountPoint, false);
            return gameObject.AddComponent<T>();
        }

        public void Awake()
        {
            this.canvasGroupFader.HideImmediately();

            this.OnAwake();
        }

        public void Update()
        {
            this.OnUpdate();
        }

        public void OpenAt(Vector3 position)
        {
            if (this.IsVisible)
            {
                return;
            }

            SoundManager.PlaySfx("SituationWindowShow");
            this.canvasGroupFader.Show();
            this.positioner.Show(this.canvasGroupFader.durationTurnOn, position);
            this.OnOpen();

            this.Opened?.Invoke(this, EventArgs.Empty);
        }

        public void Close(bool immediately = false)
        {
            if (immediately)
            {
                this.canvasGroupFader.HideImmediately();
                this.OnClose();
                this.Closed?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!this.IsVisible)
            {
                return;
            }

            SoundManager.PlaySfx("SituationWindowHide");
            this.canvasGroupFader.Hide();

            this.OnClose();
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        public void Retire()
        {
            Destroy(this.gameObject);
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }

        protected void Clear()
        {
            this.Content.Clear();
            this.Footer.Clear();
        }

        private void BuildWindowFrame()
        {
            this.root = new SizingLayoutWidget(this.gameObject)
                .SetPivot(0.5f, 0.5f)
                .SetLeft(.5f, 0)
                .SetTop(.5f, 0)
                .SetRight(.5f, 0)
                .SetBottom(.5f, 0)
                .SetMinWidth(this.DefaultWidth)
                .SetMinHeight(this.DefaultHeight);

            var canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
            this.positioner = this.gameObject.AddComponent<WindowPositioner>();
            Reflection.SetPrivateField<CanvasGroup>(this.positioner, "canvasGroup", canvasGroup);
            Reflection.SetPrivateField<RectTransform>(this.positioner, "rectTrans", this.root.RectTransform);

            // FIXME: Auto size not really working.
            // Although this might be sizing to the "wrong" rect transform we are inserting.
            var fitter = this.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            this.canvasGroupFader = this.gameObject.AddComponent<CanvasGroupFader>();
            this.canvasGroupFader.durationTurnOn = 0.3f;
            this.canvasGroupFader.durationTurnOff = 0.3f;
            this.canvasGroupFader.Hide();

            WidgetMountPoint.On(this.gameObject, mountPoint =>
            {
                // Note: We should auto-size from our content.
                // One day I will refactor this to use dynamic sizing, but until then...
                // FIXME: This also isnt used at all!
                this.root = mountPoint.AddSizingLayout("Root")
                    .SetMinWidth(600)
                    .SetMinHeight(400);

                mountPoint.AddImage("BG_Top")
                    .SetAnchor(0, 0)
                    .SetLeft(0, 0)
                    .SetTop(1, 0)
                    .SetRight(1, 0)
                    .SetBottom(1, -50)
                    .SetSprite("window_bg_top")
                    .SlicedImage()
                    .SetColor(BgColorHeader);

                var iconSize = 65;
                var iconOffsetX = -10;
                var iconOffsetY = 10;
                this.Icon = mountPoint.AddSizingLayout("IconContainer")
                    .SetTop(1, iconOffsetY)
                    .SetBottom(1, iconOffsetY - iconSize)
                    .SetLeft(0, iconOffsetX)
                    .SetRight(0, iconOffsetX + iconSize);

                this.title = mountPoint.AddText("TitleText")
                    .SetAnchor(Vector2.zero)
                    .SetLeft(0, 57.5f)
                    .SetTop(1, 0)
                    .SetRight(1, -57.5f)
                    .SetBottom(1, -45)
                    .SetPreferredHeight(25)
                    .SetMinFontSize(10)
                    .SetMaxFontSize(30)
                    .SetOverflowMode(TextOverflowModes.Ellipsis)
                    .SetTextAlignment(TextAlignmentOptions.BottomLeft);

                mountPoint.AddImage("BG_Body")
                    .SetAnchor(0, -50)
                    .SetLeft(0, 0)
                    .SetTop(1, -50)
                    .SetRight(1, 0)
                    .SetBottom(0, 50)
                    .SetSprite("window_bg_middle")
                    .SlicedImage()
                    .SetColor(BgColorBody);

                mountPoint.AddSizingLayout("Footer")
                    .SetLeft(0, 0)
                    .SetTop(0, 50)
                    .SetRight(1, 0)
                    .SetBottom(0, 0) // container was 77/27
                    .AddContent(mountPoint =>
                    {
                        mountPoint.AddImage("BG_Footer")
                            .SetSprite("window_bg_bottom")
                            .SlicedImage()
                            .SetColor(BgColorFooter);

                        this.Footer = mountPoint.AddSizingLayout("FooterContent");
                    });

                mountPoint.AddIconButton("CloseButton")
                    .SetAnchor(-25, -25)
                    .SetLeft(1, -37)
                    .SetTop(1, -13)
                    .SetRight(1, -13)
                    .SetBottom(1, -37)
                    .SetSize(24, 24)
                    .SetSprite("icon_close")
                    .CenterImage()
                    .SetColor(new Color(0.3804f, 0.7294f, 0.7922f, 1))
                    .SetHighlightedColor(Color.white)
                    .SetPressedColor(new Color(0.2671f, 0.6328f, 0.6985f, 1))
                    .SetSelectedColor(Color.white)
                    .SetDisabledColor(new Color(0.5368f, 0.5368f, 0.5368f, 0.502f))
                    .SetClickSound("UIButtonClose")
                    .OnClick(() => this.Close());

                this.Content = mountPoint.AddSizingLayout("Content")
                    .SetAnchor(0, -50)
                    .SetLeft(0, 0)
                    .SetTop(1, -50)
                    .SetRight(1, 0)
                    .SetBottom(0, 77)
                    .SetPivot(0.5f, 0.5f);
            });
        }
    }
}
