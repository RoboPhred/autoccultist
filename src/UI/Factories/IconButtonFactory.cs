namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class IconButtonFactory : ImageFactory<IconButtonFactory>
    {
        private readonly Button button;
        private readonly ButtonSoundTrigger soundTrigger;

        public IconButtonFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.button = this.gameObject.AddComponent<Button>();
            this.soundTrigger = this.gameObject.AddComponent<ButtonSoundTrigger>();
        }

        public IconButtonFactory ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.soundTrigger, "soundFXName", soundEffect);
            return this as IconButtonFactory;
        }

        public IconButtonFactory OnClick(UnityEngine.Events.UnityAction action)
        {
            this.button.onClick.AddListener(action);
            return this as IconButtonFactory;
        }

        public new Button Build()
        {
            return this.button;
        }
    }
}
