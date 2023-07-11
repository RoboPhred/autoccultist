namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public class TextWidget : SizingLayoutWidget<TextWidget>, ITextWidget<TextWidget>
    {
        public TextWidget(string key)
            : this(new GameObject(key))
        {
        }

        public TextWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.TextMesh = this.GameObject.GetOrAddComponent<TextMeshProUGUI>();
            this.TextMesh.fontMaterial = ResourceHack.FindMaterial("Philosopher-Regular optimum Material");
            this.TextMesh.font = ResourceHack.FindFont("Text_Philosopher");
        }

        public TextMeshProUGUI TextMesh { get; private set; }

        // Unity likes its lower case properties.
#pragma warning disable SA1300
        public string text
        {
            get => this.TextMesh.text;
            set => this.TextMesh.text = value;
        }
#pragma warning restore SA1300

        public TextWidget FontMaterial(string resourceName)
        {
            this.TextMesh.fontMaterial = ResourceHack.FindMaterial(resourceName);
            return this;
        }

        public TextWidget Font(string resourceName)
        {
            this.TextMesh.font = ResourceHack.FindFont(resourceName);
            return this;
        }

        public TextWidget FontStyle(FontStyles style)
        {
            this.TextMesh.fontStyle = style;
            return this;
        }

        public TextWidget FontWeight(FontWeight weight)
        {
            this.TextMesh.fontWeight = weight;
            return this;
        }

        public TextWidget TextAlignment(TextAlignmentOptions alignment)
        {
            this.TextMesh.alignment = alignment;
            return this;
        }

        public TextWidget HorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.TextMesh.horizontalAlignment = alignment;
            return this;
        }

        public TextWidget VerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.TextMesh.verticalAlignment = alignment;
            return this;
        }

        public TextWidget MinFontSize(float size)
        {
            this.TextMesh.fontSizeMin = size;
            this.TextMesh.enableAutoSizing = this.TextMesh.fontSizeMin != this.TextMesh.fontSizeMax;
            return this;
        }

        public TextWidget MaxFontSize(float size)
        {
            this.TextMesh.fontSizeMax = size;
            this.TextMesh.enableAutoSizing = this.TextMesh.fontSizeMin != this.TextMesh.fontSizeMax;
            return this;
        }

        public TextWidget FontSize(float size)
        {
            this.TextMesh.fontSize = size;
            this.TextMesh.enableAutoSizing = false;
            return this;
        }

        public TextWidget Text(string value)
        {
            this.TextMesh.text = value;
            return this;
        }

        public TextWidget TextColor(Color color)
        {
            this.TextMesh.color = color;
            return this;
        }
    }
}
