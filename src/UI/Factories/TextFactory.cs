namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public class TextFactory : LayoutElementFactory<TextFactory>
    {
        private readonly TextMeshProUGUI text;

        public TextFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.text = this.gameObject.AddComponent<TextMeshProUGUI>();
            this.text.fontMaterial = ResourceHack.FindMaterial("Philosopher-Regular optimum Material");
            this.text.font = ResourceHack.FindFont("Text_Philosopher");
        }

        public TextFactory TextAlignment(TextAlignmentOptions alignment)
        {
            this.text.alignment = alignment;
            return this;
        }

        public TextFactory MinFontSize(float size)
        {
            this.text.fontSizeMin = size;
            this.text.enableAutoSizing = this.text.fontSizeMin != this.text.fontSizeMax;
            return this;
        }

        public TextFactory MaxFontSize(float size)
        {
            this.text.fontSizeMax = size;
            this.text.enableAutoSizing = this.text.fontSizeMin != this.text.fontSizeMax;
            return this;
        }

        public TextFactory FontSize(float size)
        {
            this.text.fontSize = size;
            this.text.enableAutoSizing = this.text.fontSizeMin != this.text.fontSizeMax;
            return this;
        }

        public TextFactory Text(string value)
        {
            this.text.text = value;
            return this;
        }

        public TextFactory TextColor(Color color)
        {
            this.text.color = color;
            return this;
        }

        public new TextMeshProUGUI Build()
        {
            return this.text;
        }
    }
}
