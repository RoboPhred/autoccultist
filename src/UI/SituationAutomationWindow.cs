namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using SecretHistories.Abstract;
    using SecretHistories.Entities;
    using SecretHistories.Events;
    using SecretHistories.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class SituationAutomationWindow : MonoBehaviour, IPayloadWindow
    {
        private static readonly Color BgColorHeader = new Color(0.0314f, 0.0706f, 0.1059f, 0.902f);
        private static readonly Color BgColorBody = new Color(0.0627f, 0.1333f, 0.1922f, 0.902f);
        private static readonly Color BgColorFooter = new Color(0.0029f, 0.0246f, 0.0441f, 0.902f);

        private Situation situation;

        private CanvasGroupFader canvasGroupFader;
        private WindowPositioner positioner;
        private TextMeshProUGUI title;

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

        public static SituationAutomationWindow CreateWindow(string name)
        {
            var windowSphere = GameObject.Find("TabletopWindowSphere");
            if (!windowSphere)
            {
                throw new Exception("Cannot find TabletopWindowSphere.");
            }

            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(windowSphere.transform, false);

            var window = gameObject.AddComponent<SituationAutomationWindow>();
            NoonUtility.LogWarning($"Creating window {name}.  Our rect transform parent is {window.GetComponentInParent<RectTransform>()}");
            window.BuildWindow();

            return window;
        }

        public void Show(Vector3 position)
        {
            SoundManager.PlaySfx("SituationWindowShow");
            this.canvasGroupFader.Show();
            this.positioner.Show(this.canvasGroupFader.durationTurnOn, position);
        }

        public void Close()
        {
            SoundManager.PlaySfx("SituationWindowHide");
            this.canvasGroupFader.Hide();
        }

        public void Attach(ElementStack elementStack)
        {
            throw new NotSupportedException();
        }

        public void Attach(Situation situation)
        {
            this.situation = situation;
            this.Title = $"{situation.VerbId.Capitalize()} Automations";
        }

        public void ContentsDisplayChanged(ContentsDisplayChangedArgs args)
        {
        }

        public void NotifySpheresChanged(Context context)
        {
        }

        private void BuildWindow()
        {
            this.BuildWindowFrame();
            this.BuildWindowContent();
        }

        private void BuildWindowContent()
        {
            UIFactories.CreateScrollRect("ScrollRect", this.gameObject.transform)
                .Anchor(0, -50)
                .AnchorRelativeToParent(0, 1, 1, 0)
                // Note: This bottom should be 52 to coincide with our BG_Body, but that overlaps us with the footer.
                .Offset(0, -50, 0, 77)
                .Vertical()
                .AddContent(
                    transform =>
                    {
                        UIFactories.CreateText("DumpText", transform)
                        .Pivot(0, 0)
                        .FontSize(12)
                        .Text(string.Join("\n", Enumerable.Repeat("Testing", 50)))
                        .Build();
                    })
                .Build();
        }

        private void BuildWindowFrame()
        {
            // TODO: A lot of these RectTransform properties are interdependent.
            // What do we need to set, what can we ignore?

            // Note: We should auto-size from our content.
            var rectTransform = UIFactories.AddRectTransform(this.gameObject)
                .AnchorRelativeToParent(.5f, .5f, .5f, .5f)
                .Size(700, 420)
                .Build();
            UIFactories.AddLayoutElement(this.gameObject)
                .MinWidth(650)
                .MinHeight(600)
                .Build();

            var canvasGroup = this.gameObject.AddComponent<CanvasGroup>();

            this.positioner = this.gameObject.AddComponent<WindowPositioner>();
            Reflection.SetPrivateField(this.positioner, "rectTrans", rectTransform);
            Reflection.SetPrivateField(this.positioner, "canvasGroup", canvasGroup);

            this.canvasGroupFader = this.gameObject.AddComponent<CanvasGroupFader>();
            this.canvasGroupFader.durationTurnOn = 0.3f;
            this.canvasGroupFader.durationTurnOff = 0.3f;
            this.canvasGroupFader.Hide();

            var fitter = this.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            UIFactories.CreateImage("BG_Top", this.gameObject.transform)
                .Anchor(0, 0)
                .AnchorRelativeToParent(0, 1, 1, 1)
                .Offset(0, 0, 0, -50)
                .Sprite("window_bg_top")
                .SlicedImage()
                .Color(BgColorHeader)
                .Build();

            UIFactories.CreateImage("BG_Body", this.gameObject.transform)
                .Anchor(0, -50)
                .AnchorRelativeToParent(0, 1, 1, 0)
                .Offset(0, -50, 0, 77)
                .Sprite("window_bg_middle")
                .SlicedImage()
                .Color(BgColorBody)
                .Build();

            var footerContainer = UIFactories.CreateVeritcalLayoutGroup("FooterContainer", this.gameObject.transform)
                .AnchorRelativeToParent(0, 0, 1, 0)
                .Offset(0, 77, 0, 27)
                .ChildControlHeight(true)
                .ChildControlWidth(true)
                .ChildForceExpandHeight(true)
                .ChildForceExpandWidth(true)
                .Build();

            UIFactories.CreateImage("Footer", footerContainer.gameObject.transform)
                .Anchor(350, -90)
                .AnchorRelativeToParent(0, 1, 0, 1)
                .Offset(0, -40, 700, -90)
                .MinHeight(5)
                .Sprite("window_bg_bottom")
                .SlicedImage()
                .Color(BgColorFooter)
                .Build();

            this.title = UIFactories.CreateText("TitleText", this.gameObject.transform)
                .Anchor(Vector2.zero)
                .AnchorRelativeToParent(0, 1, 1, 1)
                .Offset(57.5f, 0, -57.5f, -45)
                .PreferredHeight(25)
                .FontSize(30)
                .MinFontSize(10)
                .MaxFontSize(30)
                .TextAlignment(TextAlignmentOptions.BottomLeft)
                .TextColor(new Color(0.5765f, 0.8824f, 0.9373f, 1))
                .Build();

            UIFactories.CreateIconButton("CloseButton", this.gameObject.transform)
                .Anchor(-25, -25)
                .AnchorRelativeToParent(1, 1, 1, 1)
                .Offset(-37, -13, -13, -37)
                .Size(24, 24)
                .Sprite("icon_close")
                .CenterImage()
                .ClickSound("UIBUttonClose")
                .OnClick(this.Close)
                .Build();
        }
    }
}
