namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public class TextWidget : SizingLayoutWidget<TextWidget>, ITextWidget<TextWidget>
    {
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
            this.textMesh.color = new Color(0.5765f, 0.8824f, 0.9373f, 1);
        }

        public TextMeshProUGUI TextMeshBehavior => this.textMesh;

#pragma warning disable SA1300
        public string text
        {
            get => this.textMesh.text;
            set => this.textMesh.text = value;
        }
#pragma warning restore SA1300

        public TextWidget FontMaterial(string resourceName)
        {
            this.textMesh.fontMaterial = ResourceHack.FindMaterial(resourceName);
            return this;
        }

        public TextWidget Font(string resourceName)
        {
            this.textMesh.font = ResourceHack.FindFont(resourceName);
            return this;
        }

        public TextWidget FontStyle(FontStyles style)
        {
            this.textMesh.fontStyle = style;
            return this;
        }

        public TextWidget FontWeight(FontWeight weight)
        {
            this.textMesh.fontWeight = weight;
            return this;
        }

        public TextWidget TextAlignment(TextAlignmentOptions alignment)
        {
            this.textMesh.alignment = alignment;
            return this;
        }

        public TextWidget HorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.textMesh.horizontalAlignment = alignment;
            return this;
        }

        public TextWidget VerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.textMesh.verticalAlignment = alignment;
            return this;
        }

        public TextWidget WordWrapping(bool enabled)
        {
            this.textMesh.enableWordWrapping = enabled;
            return this;
        }

        public TextWidget MinFontSize(float size)
        {
            this.textMesh.fontSizeMin = size;
            this.textMesh.enableAutoSizing = this.textMesh.fontSizeMin != this.textMesh.fontSizeMax;
            return this;
        }

        public TextWidget MaxFontSize(float size)
        {
            this.textMesh.fontSizeMax = size;
            this.textMesh.enableAutoSizing = this.textMesh.fontSizeMin != this.textMesh.fontSizeMax;
            return this;
        }

        public TextWidget FontSize(float size)
        {
            this.textMesh.fontSize = size;
            this.textMesh.enableAutoSizing = false;
            return this;
        }

        public TextWidget Text(string value)
        {
            this.textMesh.text = value;
            return this;
        }

        public TextWidget TextColor(Color color)
        {
            this.textMesh.color = color;
            return this;
        }
    }
}
