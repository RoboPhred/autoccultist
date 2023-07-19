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
        private TextWidget title;

        public event EventHandler Opened;
        public event EventHandler Closed;

        public AbstractWindow()
        {
            this.BuildWindowFrame();
        }

        public Vector3 Position
        {
            get => this.positioner.GetPosition();
            set => this.positioner.SetPosition(value);
        }

        public string Title
        {
            get => this.title.text;
            set => this.title.text = value;
        }

        public bool IsOpen => this.canvasGroupFader.IsFullyVisible();

        public bool IsVisible => this.canvasGroupFader.IsFullyVisible() || this.canvasGroupFader.IsAppearing();

        public bool IsClosed => this.canvasGroupFader.IsInvisible();

        protected WidgetMountPoint Icon { get; private set; }

        protected WidgetMountPoint Content { get; private set; }

        protected WidgetMountPoint Footer { get; private set; }

        public static T CreateTabletopWindow<T>(string key)
            where T : AbstractWindow
        {
            var windowSphere = GameObject.Find("TabletopWindowSphere");
            if (windowSphere == null)
            {
                throw new Exception("Cannot find TabletopWindowSphere.");
            }

            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(windowSphere.transform, false);
            return gameObject.AddComponent<T>();
        }

        public static T CreateMetaWindow<T>(string key)
            where T : AbstractWindow
        {
            var windowSphere = GameObject.Find("CanvasMeta");
            if (windowSphere == null)
            {
                throw new Exception("Cannot find CanvasMeta.");
            }

            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(windowSphere.transform, false);
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
            // Note: We should auto-size from our content.
            var rectTransform = UIFactories.AddRectTransform(this.gameObject)
                .AnchorRelativeToParent(.5f, .5f, .5f, .5f);

            // One day I will refactor this to use dynamic sizing, but until then...
            UIFactories.AddSizingLayout(this.gameObject)
                .MinWidth(500)
                .MinHeight(400);

            var canvasGroup = this.gameObject.AddComponent<CanvasGroup>();

            this.positioner = this.gameObject.AddComponent<WindowPositioner>();
            Reflection.SetPrivateField<RectTransform>(this.positioner, "rectTrans", rectTransform);
            Reflection.SetPrivateField<CanvasGroup>(this.positioner, "canvasGroup", canvasGroup);

            this.canvasGroupFader = this.gameObject.AddComponent<CanvasGroupFader>();
            this.canvasGroupFader.durationTurnOn = 0.3f;
            this.canvasGroupFader.durationTurnOff = 0.3f;
            this.canvasGroupFader.Hide();

            var fitter = this.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            WidgetMountPoint.On(this.gameObject, mountPoint =>
            {
                mountPoint.AddImage("BG_Top")
                    .Anchor(0, 0)
                    .Left(0, 0)
                    .Top(1, 0)
                    .Right(1, 0)
                    .Bottom(1, -50)
                    .Sprite("window_bg_top")
                    .SlicedImage()
                    .Color(BgColorHeader);

                this.Icon = mountPoint.AddSizingLayout("IconContainer")
                    .Top(1, 0)
                    .Bottom(1, -50)
                    .Left(0, 3)
                    .Right(0, 53);

                this.title = mountPoint.AddText("TitleText")
                    .Anchor(Vector2.zero)
                    .Left(0, 57.5f)
                    .Top(1, 0)
                    .Right(1, -57.5f)
                    .Bottom(1, -45)
                    .PreferredHeight(25)
                    .FontSize(30)
                    .MinFontSize(10)
                    .MaxFontSize(30)
                    .TextAlignment(TextAlignmentOptions.BottomLeft);

                mountPoint.AddImage("BG_Body")
                    .Anchor(0, -50)
                    .Left(0, 0)
                    .Top(1, -50)
                    .Right(1, 0)
                    .Bottom(0, 50)
                    .Sprite("window_bg_middle")
                    .SlicedImage()
                    .Color(BgColorBody);

                mountPoint.AddSizingLayout("Footer")
                    .Left(0, 0)
                    .Top(0, 50)
                    .Right(1, 0)
                    .Bottom(0, 0) // container was 77/27
                    .AddContent(mountPoint =>
                    {
                        mountPoint.AddImage("BG_Footer")
                            .Sprite("window_bg_bottom")
                            .SlicedImage()
                            .Color(BgColorFooter);

                        this.Footer = mountPoint.AddSizingLayout("FooterContent");
                    });

                mountPoint.AddIconButton("CloseButton")
                    .Anchor(-25, -25)
                    .Left(1, -37)
                    .Top(1, -13)
                    .Right(1, -13)
                    .Bottom(1, -37)
                    .Size(24, 24)
                    .Sprite("icon_close")
                    .Color(new Color(0.3804f, 0.7294f, 0.7922f, 1))
                    .HighlightedColor(Color.white)
                    .PressedColor(new Color(0.2671f, 0.6328f, 0.6985f, 1))
                    .SelectedColor(Color.white)
                    .DisabledColor(new Color(0.5368f, 0.5368f, 0.5368f, 0.502f))
                    .CenterImage()
                    .ClickSound("UIBUttonClose")
                    .OnClick(() => this.Close());

                this.Content = mountPoint.AddSizingLayout("Content")
                    .Anchor(0, -50)
                    .Left(0, 0)
                    .Top(1, -50)
                    .Right(1, 0)
                    .Bottom(0, 77);
            });
        }
    }
}
