namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class UIGameObjectWidget
    {
        public UIGameObjectWidget(string key)
            : this(new GameObject(key))
        {
        }

        public UIGameObjectWidget(GameObject target)
        {
            this.GameObject = target;
            this.CanvasRenderer = this.GameObject.GetOrAddComponent<CanvasRenderer>();
        }

        public GameObject GameObject { get; private set; }

        public virtual WidgetMountPoint MountPoint => new WidgetMountPoint(this.GameObject.transform);

        public CanvasRenderer CanvasRenderer { get; private set; }

        public static implicit operator GameObject(UIGameObjectWidget widget)
        {
            return widget.GameObject;
        }

        public static implicit operator WidgetMountPoint(UIGameObjectWidget widget)
        {
            return widget.MountPoint;
        }

        public UIGameObjectWidget SetActive(bool active)
        {
            this.GameObject.SetActive(active);
            return this;
        }

        public UIGameObjectWidget Activate()
        {
            this.GameObject.SetActive(true);
            return this;
        }

        public UIGameObjectWidget Deactivate()
        {
            this.GameObject.SetActive(false);
            return this;
        }

        public UIGameObjectWidget OnPointerEnter(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerEnter += (sender, e) => action(e);
            return this;
        }

        public UIGameObjectWidget OnPointerExit(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerExit += (sender, e) => action(e);
            return this;
        }

        public UIGameObjectWidget OnPointerClick(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerClick += (sender, e) => action(e);
            return this;
        }

        public UIGameObjectWidget OnBeginDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().BeginDrag += (sender, e) => action(e);
            return this;
        }

        public UIGameObjectWidget OnDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().Drag += (sender, e) => action(e);
            return this;
        }

        public UIGameObjectWidget OnEndDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().EndDrag += (sender, e) => action(e);
            return this;
        }

        public virtual UIGameObjectWidget Clear()
        {
            foreach (Transform child in this.GameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            return this;
        }
    }
}
