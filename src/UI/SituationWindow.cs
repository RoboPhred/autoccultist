namespace AutoccultistNS.UI
{
    using System;
    using SecretHistories.Abstract;
    using SecretHistories.Entities;
    using SecretHistories.Events;
    using SecretHistories.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class SituationWindow : MonoBehaviour, IPayloadWindow
    {
        private static readonly Color BgColorHeader = new Color(0.0314f, 0.0706f, 0.1059f, 0.902f);
        private static readonly Color BgColorBody = new Color(0.0627f, 0.1333f, 0.1922f, 0.902f);
        private static readonly Color BgColorFooter = new Color(0.0029f, 0.0246f, 0.0441f, 0.902f);

        private CanvasGroupFader canvasGroupFader;
        private WindowPositioner positioner;
        private TextWidget title;
        private SizingLayoutWidget content;

        public SituationWindow()
        {
            this.BuildWindowFrame();
        }

        public Situation Situation { get; private set; }

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

        public bool IsVisible => this.canvasGroupFader.IsFullyVisible() || this.canvasGroupFader.IsAppearing();

        public static T CreateWindow<T>(string key)
            where T : SituationWindow
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

        void IPayloadWindow.NotifySpheresChanged(Context context)
        {
        }

        void IPayloadWindow.ContentsDisplayChanged(ContentsDisplayChangedArgs args)
        {
        }

        void IPayloadWindow.Attach(ElementStack elementStack)
        {
            throw new NotSupportedException();
        }

        public void Attach(Situation situation)
        {
            this.Situation = situation;
            this.Title = $"{situation.VerbId.Capitalize()} Automations";
            this.content.Clear();
            this.BuildContent(this.content.GameObject.transform);
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
        }

        public void Close()
        {
            if (!this.IsVisible)
            {
                return;
            }

            SoundManager.PlaySfx("SituationWindowHide");
            this.canvasGroupFader.Hide();

            this.OnClose();
        }

        protected abstract void BuildContent(Transform parent);

        protected virtual void OnClose()
        {
        }

        private void BuildWindowFrame()
        {
            // Note: We should auto-size from our content.
            var rectTransform = UIFactories.AddRectTransform(this.gameObject)
                .AnchorRelativeToParent(.5f, .5f, .5f, .5f);
            UIFactories.AddSizingLayout(this.gameObject)
                .MinWidth(450)
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

            UIFactories.CreateImage("BG_Top", this.gameObject)
                .Anchor(0, 0)
                .Left(0, 0)
                .Top(1, 0)
                .Right(1, 0)
                .Bottom(1, -50)
                .Sprite("window_bg_top")
                .SlicedImage()
                .Color(BgColorHeader);

            UIFactories.CreateImage("BG_Body", this.gameObject)
                .Anchor(0, -50)
                .Left(0, 0)
                .Top(1, -50)
                .Right(1, 0)
                .Bottom(0, 77)
                .Sprite("window_bg_middle")
                .SlicedImage()
                .Color(BgColorBody);

            var footerContainer = UIFactories.CreateVeritcalLayoutGroup("FooterContainer", this.gameObject)
                .Left(0, 0)
                .Top(0, 77)
                .Right(1, 0)
                .Bottom(0, 27)
                .ChildForceExpandHeight(true)
                .ChildForceExpandWidth(true);

            UIFactories.CreateImage("Footer", footerContainer)
                .Anchor(350, -90)
                .Left(0, 0)
                .Top(1, -40)
                .Right(0, 700)
                .Bottom(1, -90)
                .MinHeight(5)
                .Sprite("window_bg_bottom")
                .SlicedImage()
                .Color(BgColorFooter);

            this.title = UIFactories.CreateText("TitleText", this.gameObject)
                .Anchor(Vector2.zero)
                .Left(0, 57.5f)
                .Top(1, 0)
                .Right(1, -57.5f)
                .Bottom(1, -45)
                .PreferredHeight(25)
                .FontSize(30)
                .MinFontSize(10)
                .MaxFontSize(30)
                .TextAlignment(TextAlignmentOptions.BottomLeft)
                .TextColor(new Color(0.5765f, 0.8824f, 0.9373f, 1));

            UIFactories.CreateIconButton("CloseButton", this.gameObject)
                .Anchor(-25, -25)
                .Left(1, -37)
                .Top(1, -13)
                .Right(1, -13)
                .Bottom(1, -37)
                .Size(24, 24)
                .Sprite("icon_close")
                .CenterImage()
                .ClickSound("UIBUttonClose")
                .OnClick(this.Close);

            this.content = UIFactories.CreateSizingLayout("Content", this.gameObject)
                .Anchor(0, -50)
                .Left(0, 0)
                .Top(1, -50)
                .Right(1, 0)
                .Bottom(0, 77);
        }
    }
}
