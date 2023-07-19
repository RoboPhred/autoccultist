namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class IconButtonWidget : ImageWidget<IconButtonWidget>
    {
        public static readonly ColorBlock ColorBlock = new ColorBlock
        {
            normalColor = new Color(1, 1, 1, 1),
            highlightedColor = new Color(0.5765f, 0.8824f, 0.9373f, 1),
            pressedColor = new Color(0.1255f, 0.4471f, 0.4824f, 1),
            selectedColor = new Color(0.5765f, 0.8824f, 0.9373f, 1),
            disabledColor = new Color(0.502f, 0.502f, 0.502f, 1),
            colorMultiplier = 1,
            fadeDuration = 0.1f,
        };

        public IconButtonWidget(string key)
            : this(new GameObject(key))
        {
        }

        public IconButtonWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.Button = this.GameObject.GetOrAddComponent<Button>();
            this.Button.transition = Selectable.Transition.ColorTint;
            this.Button.colors = ColorBlock;

            this.SoundTrigger = this.GameObject.GetOrAddComponent<ButtonSoundTrigger>();
        }

        public Button Button { get; private set; }

        public ButtonSoundTrigger SoundTrigger { get; private set; }

        public IconButtonWidget ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.SoundTrigger, "soundFXName", soundEffect);
            return this as IconButtonWidget;
        }

        public new IconButtonWidget Color(Color color)
        {
            var newValue = this.Button.colors.Clone();
            newValue.normalColor = color;
            this.Button.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget HighlightedColor(Color color)
        {
            var newValue = this.Button.colors.Clone();
            newValue.highlightedColor = color;
            this.Button.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget PressedColor(Color color)
        {
            var newValue = this.Button.colors.Clone();
            newValue.pressedColor = color;
            this.Button.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget SelectedColor(Color color)
        {
            var newValue = this.Button.colors.Clone();
            newValue.selectedColor = color;
            this.Button.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget DisabledColor(Color color)
        {
            var newValue = this.Button.colors.Clone();
            newValue.disabledColor = color;
            this.Button.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget OnClick(UnityEngine.Events.UnityAction action)
        {
            this.Button.onClick.AddListener(action);
            return this as IconButtonWidget;
        }
    }
}
