namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TextButtonWidget : VerticalLayoutGroupWidget<TextButtonWidget>, ITextWidget<TextButtonWidget>
    {
        private static readonly Color FontColor = new Color(0.2392f, 0.1961f, 0.0667f, 1);

        public TextButtonWidget(string key)
            : this(new GameObject(key))
        {
        }

        public TextButtonWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.Pivot(.5f, .5f);

            this.FillContentWidth();
            this.FillContentHeight();

            // FIXME: While this looks generally good,
            // only our top padding is being applied.
            // The content text is not being squished for the 5 point padding
            // along the bottom.
            this.Padding(15, 5);

            this.Button = this.GameObject.GetOrAddComponent<Button>();
            this.Button.transition = Selectable.Transition.ColorTint;
            this.Button.colors = IconButtonWidget.ColorBlock;

            this.SoundTrigger = this.GameObject.gameObject.GetOrAddComponent<ButtonSoundTrigger>();

            var image = UIFactories.CreateImage("Image", this.GameObject.transform)
                .Pivot(.5f, .5f)
                .Left(0, 0)
                .Top(1, 0)
                .Right(1, 0)
                .Bottom(0, 0)
                .StretchImage()
                .Sprite("button")
                .IgnoreLayout();

            this.Button.image = image.Image;

            this.ChildControlHeight(true);
            this.ChildControlWidth(true);

            this.ChildForceExpandHeight(true);
            this.ChildForceExpandWidth(true);

            this.TextWidget = new TextWidget("Text")
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

            this.AddContentCentered(this.TextWidget);
        }

        public Button Button { get; private set; }

        public Image Image { get; private set; }

        public ButtonSoundTrigger SoundTrigger { get; private set; }

        public TextWidget TextWidget { get; private set; }

        public TextMeshProUGUI TextMesh => this.TextWidget.TextMesh;

        public TextButtonWidget Text(string value)
        {
            this.TextWidget.Text(value);
            return this as TextButtonWidget;
        }

        public TextButtonWidget TextColor(Color color)
        {
            this.TextWidget.TextColor(color);
            return this as TextButtonWidget;
        }

        public TextButtonWidget ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.SoundTrigger, "soundFXName", soundEffect);
            return this as TextButtonWidget;
        }

        public TextButtonWidget OnClick(UnityEngine.Events.UnityAction action)
        {
            this.Button.onClick.AddListener(action);
            return this as TextButtonWidget;
        }

        public TextButtonWidget Font(string resourceName)
        {
            this.TextWidget.Font(resourceName);
            return this;
        }

        public TextButtonWidget FontMaterial(string resourceName)
        {
            this.TextWidget.FontMaterial(resourceName);
            return this;
        }

        public TextButtonWidget FontSize(float size)
        {
            this.TextWidget.FontSize(size);
            return this;
        }

        public TextButtonWidget FontStyle(FontStyles style)
        {
            this.TextWidget.FontStyle(style);
            return this;
        }

        public TextButtonWidget FontWeight(FontWeight weight)
        {
            this.TextWidget.FontWeight(weight);
            return this;
        }

        public TextButtonWidget HorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.TextWidget.HorizontalAlignment(alignment);
            return this;
        }

        public TextButtonWidget MaxFontSize(float size)
        {
            this.TextWidget.MaxFontSize(size);
            return this;
        }

        public TextButtonWidget MinFontSize(float size)
        {
            this.TextWidget.MinFontSize(size);
            return this;
        }

        public TextButtonWidget TextAlignment(TextAlignmentOptions alignment)
        {
            this.TextWidget.TextAlignment(alignment);
            return this;
        }
    }
}
