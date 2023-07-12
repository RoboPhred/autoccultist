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

        public static implicit operator Transform(WidgetMountPoint mountPoint) => mountPoint.Transform;

        public static implicit operator GameObject(WidgetMountPoint mountPoint) => mountPoint.Transform.gameObject;

        public static void On(Transform transform, System.Action<WidgetMountPoint> action)
        {
            action(new WidgetMountPoint(transform));
        }

        public static void On(GameObject gameObject, System.Action<WidgetMountPoint> action)
        {
            action(new WidgetMountPoint(gameObject.transform));
        }

        public void Clear()
        {
            foreach (Transform child in this.Transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void AddWidget(UIGameObjectWidget widget)
        {
            widget.GameObject.transform.SetParent(this.Transform, false);
        }
    }
}
