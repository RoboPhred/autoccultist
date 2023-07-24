namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public class TextWidget : SizingLayoutWidget<TextWidget>, ITextWidget<TextWidget>
    {
        private static readonly Color DefaultFontColor = new Color(0.5765f, 0.8824f, 0.9373f, 1);

        private TextMeshProUGUI textMesh;

        public TextWidget(string key)
            : this(new GameObject(key))
        {
        }

        public TextWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.textMesh = this.GameObject.GetOrAddComponent<TextMeshProUGUI>();
            this.textMesh.fontMaterial = ResourceHack.FindMaterial("Philosopher-Regular optimum Material");
            this.textMesh.font = ResourceHack.FindFont("Text_Philosopher");
            this.textMesh.color = DefaultFontColor;
        }

        public TextMeshProUGUI TextMesh => this.textMesh;

        public string Font
        {
            get
            {
                return this.textMesh.font.name;
            }

            set
            {
                this.SetFont(value);
            }
        }

        public string FontMaterial
        {
            get
            {
                return this.textMesh.fontMaterial.name;
            }

            set
            {
                this.SetFontMaterial(value);
            }
        }

        public float FontSize
        {
            get
            {
                return this.textMesh.fontSize;
            }

            set
            {
                this.SetFontSize(value);
            }
        }

        public FontStyles FontStyle
        {
            get
            {
                return this.textMesh.fontStyle;
            }

            set
            {
                this.SetFontStyle(value);
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this.textMesh.fontWeight;
            }

            set
            {
                this.SetFontWeight(value);
            }
        }

        public HorizontalAlignmentOptions HorizontalAlignment
        {
            get
            {
                return this.textMesh.horizontalAlignment;
            }

            set
            {
                this.SetHorizontalAlignment(value);
            }
        }

        public VerticalAlignmentOptions VerticalAlignment
        {
            get
            {
                return this.textMesh.verticalAlignment;
            }

            set
            {
                this.SetVerticalAlignment(value);
            }
        }

        public TextAlignmentOptions TextAlignment
        {
            get
            {
                return this.textMesh.alignment;
            }

            set
            {
                this.SetTextAlignment(value);
            }
        }

        public bool WordWrapping
        {
            get
            {
                return this.textMesh.enableWordWrapping;
            }

            set
            {
                this.SetWordWrapping(value);
            }
        }

        public TextOverflowModes OverflowMode
        {
            get
            {
                return this.textMesh.overflowMode;
            }

            set
            {
                this.SetOverflowMode(value);
            }
        }

        public float MaxFontSize
        {
            get
            {
                return this.textMesh.fontSizeMax;
            }

            set
            {
                this.SetMaxFontSize(value);
            }
        }

        public float MinFontSize
        {
            get
            {
                return this.textMesh.fontSizeMin;
            }

            set
            {
                this.SetMinFontSize(value);
            }
        }

        public string Text
        {
            get
            {
                return this.textMesh.text;
            }

            set
            {
                this.SetText(value);
            }
        }

        public Color Color
        {
            get
            {
                return this.textMesh.color;
            }

            set
            {
                this.SetColor(value);
            }
        }

        public TextWidget SetFontMaterial(string resourceName)
        {
            this.textMesh.fontMaterial = ResourceHack.FindMaterial(resourceName);
            return this;
        }

        public TextWidget SetFont(string resourceName)
        {
            this.textMesh.font = ResourceHack.FindFont(resourceName);
            return this;
        }

        public TextWidget SetFontStyle(FontStyles style)
        {
            this.textMesh.fontStyle = style;
            return this;
        }

        public TextWidget SetFontWeight(FontWeight weight)
        {
            this.textMesh.fontWeight = weight;
            return this;
        }

        public TextWidget SetTextAlignment(TextAlignmentOptions alignment)
        {
            this.textMesh.alignment = alignment;
            return this;
        }

        public TextWidget SetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.textMesh.horizontalAlignment = alignment;
            return this;
        }

        public TextWidget SetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.textMesh.verticalAlignment = alignment;
            return this;
        }

        public TextWidget SetWordWrapping(bool enabled)
        {
            this.textMesh.enableWordWrapping = enabled;
            return this;
        }

        public TextWidget SetMinFontSize(float size)
        {
            this.textMesh.fontSizeMin = size;
            this.textMesh.enableAutoSizing = this.textMesh.fontSizeMin != this.textMesh.fontSizeMax;
            return this;
        }

        public TextWidget SetMaxFontSize(float size)
        {
            this.textMesh.fontSizeMax = size;
            this.textMesh.enableAutoSizing = this.textMesh.fontSizeMin != this.textMesh.fontSizeMax;
            return this;
        }

        public TextWidget SetFontSize(float size)
        {
            this.textMesh.fontSize = size;
            this.textMesh.enableAutoSizing = false;
            return this;
        }

        public TextWidget SetOverflowMode(TextOverflowModes mode)
        {
            this.textMesh.overflowMode = mode;
            return this;
        }

        public TextWidget SetText(string value)
        {
            this.textMesh.text = value;
            return this;
        }

        public TextWidget SetColor(Color color)
        {
            this.textMesh.color = color;
            return this;
        }
    }
}
