namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class WidgetMountPoint
    {
        public WidgetMountPoint(Transform transform)
        {
            this.Transform = transform;
        }

        public Transform Transform { get; private set; }

        public static void On(Transform transform, System.Action<WidgetMountPoint> action)
        {
            action(new WidgetMountPoint(transform));
        }

        public static void On(GameObject gameObject, System.Action<WidgetMountPoint> action)
        {
            action(new WidgetMountPoint(gameObject.transform));
        }

        public static implicit operator Transform(WidgetMountPoint mountPoint) => mountPoint.Transform;

        public static implicit operator GameObject(WidgetMountPoint mountPoint) => mountPoint.Transform.gameObject;

        public SizingLayoutWidget AddSizingLayout(string key)
        {
            return new SizingLayoutWidget(key).AttachTo(this);
        }

        public ImageWidget AddImage(string key)
        {
            return new ImageWidget(key).AttachTo(this);
        }

        public TextWidget AddText(string key)
        {
            return new TextWidget(key).AttachTo(this);
        }

        public IconButtonWidget AddIconButton(string key)
        {
            return new IconButtonWidget(key).AttachTo(this);
        }

        public TextButtonWidget AddTextButton(string key)
        {
            return new TextButtonWidget(key).AttachTo(this);
        }

        public VerticalLayoutGroupWidget AddVeritcalLayoutGroup(string key)
        {
            return new VerticalLayoutGroupWidget(key).AttachTo(this);
        }

        public HorizontalLayoutGroupWidget AddHorizontalLayoutGroup(string key)
        {
            return new HorizontalLayoutGroupWidget(key).AttachTo(this);
        }

        public ScrollWidget AddScroll(string key)
        {
            return new ScrollWidget(key).AttachTo(this);
        }
    }
}
