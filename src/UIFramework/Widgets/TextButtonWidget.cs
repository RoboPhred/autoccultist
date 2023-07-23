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
            this.SetPivot(.5f, .5f);

            this.FitContentWidth();
            this.FitContentHeight();

            // FIXME: While this looks generally good,
            // only our top padding is being applied.
            // The content text is not being squished for the 5 point padding
            // along the bottom.
            this.SetPadding(15, 5);

            this.Button = this.GameObject.GetOrAddComponent<Button>();
            this.Button.transition = Selectable.Transition.ColorTint;
            this.Button.colors = IconButtonWidget.ColorBlock;

            this.SoundTrigger = this.GameObject.gameObject.GetOrAddComponent<ButtonSoundTrigger>();

            WidgetMountPoint.On(this.GameObject, mountPoint =>
            {
                var image = mountPoint.AddImage("Image")
                    .SetPivot(.5f, .5f)
                    .SetLeft(0, 0)
                    .SetTop(1, 0)
                    .SetRight(1, 0)
                    .SetBottom(0, 0)
                    .StretchImage()
                    .SetSprite("button")
                    .SetIgnoreLayout();
                this.Button.image = image.Image;
            });

            this.SetSpreadChildrenHorizontally(true);
            this.SetSpreadChildrenVertically(true);

            this.TextWidget = new TextWidget("Text")
                .SetPivot(.5f, .5f)
                .SetLeft(0, 0)
                .SetTop(1, 0)
                .SetRight(1, 0)
                .SetBottom(0, 0)
                .SetColor(FontColor)
                .SetTextAlignment(TextAlignmentOptions.Center)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                .SetFontStyle(FontStyles.Bold)
                .SetMinFontSize(12)
                .SetMaxFontSize(20);

            this.AddContent(this.TextWidget);
        }

        public TextMeshProUGUI TextMesh => this.TextWidget.TextMesh;

        public Button Button { get; }

        public ButtonSoundTrigger SoundTrigger { get; }

        public TextWidget TextWidget { get; }

        public bool Enabled
        {
            get
            {
                return this.Button.interactable;
            }

            set
            {
                this.Button.interactable = value;
            }
        }

        public string Text
        {
            get
            {
                return this.TextWidget.Text;
            }

            set
            {
                this.TextWidget.Text = value;
            }
        }

        public Color Color
        {
            get
            {
                return this.TextWidget.Color;
            }

            set
            {
                this.TextWidget.Color = value;
            }
        }

        public string Font
        {
            get
            {
                return this.TextWidget.Font;
            }

            set
            {
                this.TextWidget.Font = value;
            }
        }

        public string FontMaterial
        {
            get
            {
                return this.TextWidget.FontMaterial;
            }

            set
            {
                this.TextWidget.FontMaterial = value;
            }
        }

        public float FontSize
        {
            get
            {
                return this.TextWidget.FontSize;
            }

            set
            {
                this.TextWidget.FontSize = value;
            }
        }

        public FontStyles FontStyle
        {
            get
            {
                return this.TextWidget.FontStyle;
            }

            set
            {
                this.TextWidget.FontStyle = value;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this.TextWidget.FontWeight;
            }

            set
            {
                this.TextWidget.FontWeight = value;
            }
        }

        public HorizontalAlignmentOptions HorizontalAlignment
        {
            get
            {
                return this.TextWidget.HorizontalAlignment;
            }

            set
            {
                this.TextWidget.HorizontalAlignment = value;
            }
        }

        public VerticalAlignmentOptions VerticalAlignment
        {
            get
            {
                return this.TextWidget.VerticalAlignment;
            }

            set
            {
                this.TextWidget.VerticalAlignment = value;
            }
        }

        public TextAlignmentOptions TextAlignment
        {
            get
            {
                return this.TextWidget.TextAlignment;
            }

            set
            {
                this.TextWidget.TextAlignment = value;
            }
        }

        public bool WordWrapping
        {
            get
            {
                return this.TextWidget.WordWrapping;
            }

            set
            {
                this.TextWidget.WordWrapping = value;
            }
        }

        public TextOverflowModes OverflowMode
        {
            get
            {
                return this.TextWidget.OverflowMode;
            }

            set
            {
                this.TextWidget.OverflowMode = value;
            }
        }

        public float MaxFontSize
        {
            get
            {
                return this.TextWidget.MaxFontSize;
            }

            set
            {
                this.TextWidget.MaxFontSize = value;
            }
        }

        public float MinFontSize
        {
            get
            {
                return this.TextWidget.MinFontSize;
            }

            set
            {
                this.TextWidget.MinFontSize = value;
            }
        }

        public TextButtonWidget SetEnabled(bool enabled)
        {
            this.Button.interactable = enabled;
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

        public TextButtonWidget SetText(string value)
        {
            this.TextWidget.SetText(value);
            return this as TextButtonWidget;
        }

        public TextButtonWidget SetColor(Color color)
        {
            this.TextWidget.SetColor(color);
            return this as TextButtonWidget;
        }

        public TextButtonWidget SetClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.SoundTrigger, "soundFXName", soundEffect);
            return this as TextButtonWidget;
        }

        public TextButtonWidget SetFont(string resourceName)
        {
            this.TextWidget.SetFont(resourceName);
            return this;
        }

        public TextButtonWidget SetFontMaterial(string resourceName)
        {
            this.TextWidget.SetFontMaterial(resourceName);
            return this;
        }

        public TextButtonWidget SetFontSize(float size)
        {
            this.TextWidget.SetFontSize(size);
            return this;
        }

        public TextButtonWidget SetFontStyle(FontStyles style)
        {
            this.TextWidget.SetFontStyle(style);
            return this;
        }

        public TextButtonWidget SetFontWeight(FontWeight weight)
        {
            this.TextWidget.SetFontWeight(weight);
            return this;
        }

        public TextButtonWidget SetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.TextWidget.SetHorizontalAlignment(alignment);
            return this;
        }

        public TextButtonWidget SetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.TextWidget.SetVerticalAlignment(alignment);
            return this;
        }

        public TextButtonWidget SetTextAlignment(TextAlignmentOptions alignment)
        {
            this.TextWidget.SetTextAlignment(alignment);
            return this;
        }

        public TextButtonWidget SetMaxFontSize(float size)
        {
            this.TextWidget.SetMaxFontSize(size);
            return this;
        }

        public TextButtonWidget SetMinFontSize(float size)
        {
            this.TextWidget.SetMinFontSize(size);
            return this;
        }

        public TextButtonWidget OnClick(UnityEngine.Events.UnityAction action)
        {
            this.Button.onClick.AddListener(action);
            return this as TextButtonWidget;
        }

        public TextButtonWidget SetWordWrapping(bool enabled)
        {
            throw new System.NotImplementedException();
        }

        public TextButtonWidget SetOverflowMode(TextOverflowModes mode)
        {
            throw new System.NotImplementedException();
        }
    }
}
