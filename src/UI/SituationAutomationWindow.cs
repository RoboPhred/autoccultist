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
            // TODO: A lot of these RectTransform properties are interdependent.
            // What do we need to set, what can we ignore?

            var rectTransform = this.gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(.5f, .5f);
            rectTransform.anchorMax = new Vector2(.5f, .5f);
            rectTransform.sizeDelta = new Vector2(700, 420);

            var canvasGroup = this.gameObject.AddComponent<CanvasGroup>();

            this.positioner = this.gameObject.AddComponent<WindowPositioner>();
            Reflection.SetPrivateField(this.positioner, "rectTrans", rectTransform);
            Reflection.SetPrivateField(this.positioner, "canvasGroup", canvasGroup);

            this.canvasGroupFader = this.gameObject.AddComponent<CanvasGroupFader>();
            this.canvasGroupFader.durationTurnOn = 0.3f;
            this.canvasGroupFader.durationTurnOff = 0.3f;
            this.canvasGroupFader.Hide();

            var layout = this.gameObject.AddComponent<LayoutElement>();
            // Note: Ideally we would let this auto-size from its contents.
            layout.minWidth = 650; // set to -1 in situation window
            layout.minHeight = 600;

            var fitter = this.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var top = new GameObject("BG_Top");
            top.transform.SetParent(this.gameObject.transform, false);
            var topRt = top.AddComponent<RectTransform>();
            topRt.anchoredPosition = new Vector2(0, 0);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.anchorMin = new Vector2(0, 1);
            topRt.offsetMax = new Vector2(0, 0);
            topRt.offsetMin = new Vector2(0, -50);
            top.AddComponent<LayoutElement>().preferredHeight = 45;
            top.AddComponent<CanvasRenderer>();
            var topImg = top.AddComponent<Image>();
            topImg.sprite = ResourceHack.FindSprite("window_bg_top");
            topImg.color = BgColorHeader;
            topImg.type = Image.Type.Sliced;

            var body = new GameObject("BG_Body");
            body.transform.SetParent(this.gameObject.transform, false);
            var bodyRt = body.AddComponent<RectTransform>();
            bodyRt.anchoredPosition = new Vector2(0, -50);
            bodyRt.anchorMax = new Vector2(1, 1);
            bodyRt.anchorMin = new Vector2(0, 0);
            bodyRt.offsetMax = new Vector2(0, -50);
            bodyRt.offsetMin = new Vector2(0, 52);
            body.AddComponent<CanvasRenderer>();
            var bodyImg = body.AddComponent<Image>();
            bodyImg.sprite = ResourceHack.FindSprite("window_bg_middle");
            bodyImg.color = BgColorBody;
            bodyImg.type = Image.Type.Sliced;

            var footerContainer = new GameObject("FooterContainer");
            footerContainer.transform.SetParent(this.gameObject.transform, false);
            var footerContainerRt = footerContainer.AddComponent<RectTransform>();
            footerContainerRt.anchorMax = new Vector2(1, 0);
            footerContainerRt.anchorMin = new Vector2(0, 0);
            footerContainerRt.offsetMax = new Vector2(0, 77);
            footerContainerRt.offsetMin = new Vector2(0, 27);
            // This seems to override offsetMax/offsetMin.
            // However, if offsetMax/offsetMin arent present, this breaks sizing.
            // This still may be needed for dynamic sizing?
            // footerContainerRt.anchoredPosition = new Vector2(0, 52);
            footerContainer.AddComponent<CanvasRenderer>();
            var footerContainerLg = footerContainer.AddComponent<VerticalLayoutGroup>();
            footerContainerLg.childControlHeight = true;
            footerContainerLg.childControlWidth = true;
            footerContainerLg.childForceExpandHeight = true;
            footerContainerLg.childForceExpandWidth = true;

            var footer = new GameObject("Footer");
            footer.transform.SetParent(footerContainer.transform, false);
            footer.AddComponent<CanvasRenderer>();
            var footerRt = footer.AddComponent<RectTransform>();
            footerRt.anchoredPosition = new Vector2(350, -90);
            footerRt.anchorMax = new Vector2(0, 1);
            footerRt.anchorMin = new Vector2(0, 1);
            footerRt.offsetMax = new Vector2(700, -40);
            footerRt.offsetMin = new Vector2(0, -90);
            var footerLe = footer.AddComponent<LayoutElement>();
            footerLe.minHeight = 50;
            var footerImg = footer.AddComponent<Image>();
            footerImg.sprite = ResourceHack.FindSprite("window_bg_bottom");
            footerImg.color = BgColorFooter;
            footerImg.type = Image.Type.Sliced;

            var title = new GameObject("TitleText");
            title.transform.SetParent(this.gameObject.transform, false);
            title.AddComponent<CanvasRenderer>();
            var titleRt = title.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.offsetMax = new Vector2(-57.5f, 0);
            titleRt.offsetMin = new Vector2(57.5f, -45);
            var titleLe = title.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 25;
            var titleText = title.AddComponent<TextMeshProUGUI>();
            this.title = titleText;
            titleText.alignment = TextAlignmentOptions.BottomLeft;
            titleText.color = new Color(0.5765f, 0.8824f, 0.9373f, 1);
            titleText.fontMaterial = ResourceHack.FindMaterial("Philosopher-Regular optimum Material");
            titleText.font = ResourceHack.FindFont("Text_Philosopher");

            var closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(this.gameObject.transform, false);
            closeButton.AddComponent<CanvasRenderer>();
            var closeButtonRt = closeButton.AddComponent<RectTransform>();
            closeButtonRt.anchoredPosition = new Vector2(-25, -25);
            closeButtonRt.anchorMax = new Vector2(1, 1);
            closeButtonRt.anchorMin = new Vector2(1, 1);
            closeButtonRt.offsetMax = new Vector2(-13, -13);
            closeButtonRt.offsetMin = new Vector2(-37, -37);
            closeButtonRt.sizeDelta = new Vector2(24, 24);
            var closeButtonImg = closeButton.AddComponent<Image>();
            closeButtonImg.sprite = ResourceHack.FindSprite("icon_close");
            var button = closeButton.AddComponent<Button>();
            var soundTrigger = closeButton.AddComponent<ButtonSoundTrigger>();
            Reflection.SetPrivateField(soundTrigger, "soundFXName", "UIBUttonClose");
            button.onClick.AddListener(this.Close);
        }
    }
}
