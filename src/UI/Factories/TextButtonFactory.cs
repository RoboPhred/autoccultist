namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TextButtonFactory : SizingElementFactory<TextButtonFactory>
    {
        private static readonly Color FontColor = new Color(0.2392f, 0.1961f, 0.0667f, 1);
        private Button button;
        private Image image;
        private ButtonSoundTrigger soundTrigger;
        private TextMeshProUGUI text;

        public TextButtonFactory(GameObject gameObject)
            : base(gameObject)
        {
            // FIXME: Size dynamically to content.
            // In theory we can use FillContentHeight/FillContentHeight,
            // but that takes into account our own image and makes us huge...


            // this.PreferredWidth(100);
            // this.PreferredHeight(50);

            // FIXME: Now nothing appears at all???
            this.FillContentWidth();
            this.FillContentHeight();

            this.button = this.GameObject.GetOrAddComponent<Button>();
            this.soundTrigger = GameObject.gameObject.GetOrAddComponent<ButtonSoundTrigger>();

            // Use layout to center the text.
            UIFactories.CreateVeritcalLayoutGroup("VerticalCenter", this.GameObject.transform)
                .Left(0, 0)
                .Top(1, 0)
                .Right(1, 0)
                .Bottom(0, 0)
                .FillContentWidth()
                .FillContentHeight()
                .ChildControlHeight(true)
                .ChildControlWidth(true)
                .AddContent(transform =>
                {
                    var image = UIFactories.CreateImage("Image", transform)
                    .Pivot(.5f, .5f)
                    .Left(0, 0)
                    .Top(1, 0)
                    .Right(1, 0)
                    .Bottom(0, 0)
                    .StretchImage()
                    .MinWidth(50)
                    .MinHeight(50)
                    .Sprite("button")
                    .Build();
                    image.GetComponent<LayoutElement>().ignoreLayout = true;
                })
                .AddContentCentered(transform =>
                {
                    UIFactories.CreateHorizontalLayoutGroup("HorizontalCenter", transform)
                        .Left(0, 0)
                        .Top(1, 0)
                        .Right(1, 0)
                        .Bottom(0, 0)
                        .FillContentWidth()
                        .FillContentHeight()
                        .ChildControlHeight(true)
                        .ChildControlWidth(true)
                        .Padding(15, 10)
                        .AddContentCentered(transform =>
                        {
                            this.text = UIFactories.CreateText("Text", transform)
                                .Pivot(.5f, .5f)
                                .Left(0, 0)
                                .Top(1, 0)
                                .Right(1, 0)
                                .Bottom(0, 0)
                                .TextColor(FontColor)
                                .MinFontSize(12)
                                .MaxFontSize(20)
                                .Build();
                        })
                        .Build();
                }).Build();
        }

        public TextButtonFactory Text(string value)
        {
            this.text.text = value;
            return this as TextButtonFactory;
        }

        public TextButtonFactory TextColor(Color color)
        {
            this.text.color = color;
            return this as TextButtonFactory;
        }

        public TextButtonFactory ClickSound(string soundEffect)
        {
            Reflection.SetPrivateField(this.soundTrigger, "soundFXName", soundEffect);
            return this as TextButtonFactory;
        }

        public TextButtonFactory OnClick(UnityEngine.Events.UnityAction action)
        {
            this.button.onClick.AddListener(action);
            return this as TextButtonFactory;
        }

        public new TextButton Build()
        {
            return new TextButton(this.GameObject, this.button, this.text);
        }

        public sealed class TextButton
        {
            public TextButton(GameObject gameObject, Button button, TextMeshProUGUI text)
            {
                this.GameObject = gameObject;
                this.OnClick = button.onClick;
                this.Text = text;
            }

            public GameObject GameObject
            {
                get; private set;
            }

            public TextMeshProUGUI Text
            {
                get; private set;
            }

            public Button.ButtonClickedEvent OnClick
            {
                get; private set;
            }
        }
    }
}
