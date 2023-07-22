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

        private GameObject background;
        private GameObject content;

        public IconButtonWidget(string key)
            : this(new GameObject(key))
        {
        }

        public IconButtonWidget(GameObject gameObject)
            : base(gameObject)
        {
            this.ButtonBehavior = this.GameObject.GetOrAddComponent<Button>();
            this.ButtonBehavior.transition = Selectable.Transition.ColorTint;
            this.ButtonBehavior.colors = ColorBlock;

            this.SoundTriggerBehavior = this.GameObject.GetOrAddComponent<ButtonSoundTrigger>();

            this.content = new GameObject("Content");
            var contentRt = this.content.AddComponent<RectTransform>();
            this.content.transform.SetParent(this.GameObject.transform, false);
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            this.ButtonBehavior.image = this.ImageBehavior;
        }

        public Button ButtonBehavior { get; private set; }

        public ButtonSoundTrigger SoundTriggerBehavior { get; private set; }

        public override WidgetMountPoint MountPoint => new WidgetMountPoint(this.content.transform);

        public override Image ImageBehavior => this.content.GetOrAddComponent<Image>();

        public IconButtonWidget SetEnabled(bool enabled)
        {
            this.ButtonBehavior.interactable = enabled;
            return this;
        }

        public IconButtonWidget Enable()
        {
            return this.SetEnabled(true);
        }

        public IconButtonWidget Disable()
        {
            return this.SetEnabled(false);
        }

        public IconButtonWidget Background(Sprite sprite)
        {
            this.background = new GameObject("Background");
            var backgroundRt = this.background.AddComponent<RectTransform>();
            backgroundRt.SetParent(this.GameObject.transform, false);
            backgroundRt.SetAsFirstSibling();
            backgroundRt.anchorMin = Vector2.zero;
            backgroundRt.anchorMax = Vector2.one;
            backgroundRt.offsetMin = Vector2.zero;
            backgroundRt.offsetMax = Vector2.zero;
            var backgroundImage = this.background.AddComponent<Image>();
            backgroundImage.sprite = sprite;

            this.ButtonBehavior.image = backgroundImage;
            this.content.AddComponent<CopyColorFromBehavior>().CopyFrom = backgroundImage;

            return this;
        }

        public IconButtonWidget Background()
        {
            return this.Background(ResourcesManager.GetSpriteForUI("button_blank"));
        }

        public IconButtonWidget ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.SoundTriggerBehavior, "soundFXName", soundEffect);
            return this as IconButtonWidget;
        }

        public new IconButtonWidget Color(Color color)
        {
            var newValue = this.ButtonBehavior.colors.Clone();
            newValue.normalColor = color;
            this.ButtonBehavior.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget HighlightedColor(Color color)
        {
            var newValue = this.ButtonBehavior.colors.Clone();
            newValue.highlightedColor = color;
            this.ButtonBehavior.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget PressedColor(Color color)
        {
            var newValue = this.ButtonBehavior.colors.Clone();
            newValue.pressedColor = color;
            this.ButtonBehavior.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget SelectedColor(Color color)
        {
            var newValue = this.ButtonBehavior.colors.Clone();
            newValue.selectedColor = color;
            this.ButtonBehavior.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget DisabledColor(Color color)
        {
            var newValue = this.ButtonBehavior.colors.Clone();
            newValue.disabledColor = color;
            this.ButtonBehavior.colors = newValue;
            return this as IconButtonWidget;
        }

        public IconButtonWidget OnClick(UnityEngine.Events.UnityAction action)
        {
            this.ButtonBehavior.onClick.AddListener(action);
            return this as IconButtonWidget;
        }
    }
}
