namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TextButtonWidget : VerticalLayoutGroupWidget<TextButtonWidget>, ITextWidget<TextButtonWidget>
    {
        private static readonly Color FontColor = new Color(0.2392f, 0.1961f, 0.0667f, 1);

        private Button button;

        private ButtonSoundTrigger soundTrigger;

        private TextWidget textWidget;

        public TextButtonWidget(string key)
            : this(new GameObject(key))
        {
        }

        public TextButtonWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.Pivot(.5f, .5f);

            this.FitContentWidth();
            this.FitContentHeight();

            // FIXME: While this looks generally good,
            // only our top padding is being applied.
            // The content text is not being squished for the 5 point padding
            // along the bottom.
            this.Padding(15, 5);

            this.button = this.GameObject.GetOrAddComponent<Button>();
            this.button.transition = Selectable.Transition.ColorTint;
            this.button.colors = IconButtonWidget.ColorBlock;

            this.soundTrigger = this.GameObject.gameObject.GetOrAddComponent<ButtonSoundTrigger>();

            var image = UIFactories.CreateImage("Image", this.GameObject.transform)
                .Pivot(.5f, .5f)
                .Left(0, 0)
                .Top(1, 0)
                .Right(1, 0)
                .Bottom(0, 0)
                .StretchImage()
                .Sprite("button")
                .IgnoreLayout();

            this.button.image = image.ImageBehavior;

            this.ChildForceExpandHeight(true);
            this.ChildForceExpandWidth(true);

            this.textWidget = new TextWidget("Text")
                .Pivot(.5f, .5f)
                .Left(0, 0)
                .Top(1, 0)
                .Right(1, 0)
                .Bottom(0, 0)
                .TextColor(FontColor)
                .TextAlignment(TextAlignmentOptions.Center)
                .HorizontalAlignment(HorizontalAlignmentOptions.Center)
                .FontStyle(FontStyles.Bold)
                .MinFontSize(12)
                .MaxFontSize(20);

            this.AddExpandingSpacer();
            this.AddContent(this.textWidget);
            this.AddExpandingSpacer();
        }

        public TextMeshProUGUI TextMeshBehavior => this.textWidget.TextMeshBehavior;

        public TextButtonWidget SetEnabled(bool enabled)
        {
            this.button.interactable = enabled;
            return this;
        }

        public TextButtonWidget Enable()
        {
            return this.SetEnabled(true);
        }

        public TextButtonWidget Disable()
        {
            return this.SetEnabled(false);
        }

        public TextButtonWidget Text(string value)
        {
            this.textWidget.Text(value);
            return this as TextButtonWidget;
        }

        public TextButtonWidget TextColor(Color color)
        {
            this.textWidget.TextColor(color);
            return this as TextButtonWidget;
        }

        public TextButtonWidget ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.soundTrigger, "soundFXName", soundEffect);
            return this as TextButtonWidget;
        }

        public TextButtonWidget OnClick(UnityEngine.Events.UnityAction action)
        {
            this.button.onClick.AddListener(action);
            return this as TextButtonWidget;
        }

        public TextButtonWidget Font(string resourceName)
        {
            this.textWidget.Font(resourceName);
            return this;
        }

        public TextButtonWidget FontMaterial(string resourceName)
        {
            this.textWidget.FontMaterial(resourceName);
            return this;
        }

        public TextButtonWidget FontSize(float size)
        {
            this.textWidget.FontSize(size);
            return this;
        }

        public TextButtonWidget FontStyle(FontStyles style)
        {
            this.textWidget.FontStyle(style);
            return this;
        }

        public TextButtonWidget FontWeight(FontWeight weight)
        {
            this.textWidget.FontWeight(weight);
            return this;
        }

        public TextButtonWidget HorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.textWidget.HorizontalAlignment(alignment);
            return this;
        }

        public TextButtonWidget MaxFontSize(float size)
        {
            this.textWidget.MaxFontSize(size);
            return this;
        }

        public TextButtonWidget MinFontSize(float size)
        {
            this.textWidget.MinFontSize(size);
            return this;
        }

        public TextButtonWidget TextAlignment(TextAlignmentOptions alignment)
        {
            this.textWidget.TextAlignment(alignment);
            return this;
        }
    }
}
