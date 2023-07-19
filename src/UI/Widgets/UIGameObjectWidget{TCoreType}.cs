namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class UIGameObjectWidget<TCoreType> : UIGameObjectWidget
        where TCoreType : UIGameObjectWidget<TCoreType>
    {
        public UIGameObjectWidget(string key)
            : this(new GameObject(key))
        {
        }

        public UIGameObjectWidget(GameObject target)
            : base(target)
        {
        }

        public static implicit operator GameObject(UIGameObjectWidget<TCoreType> widget)
        {
            return widget.GameObject;
        }

        public new TCoreType SetActive(bool active)
        {
            this.GameObject.SetActive(active);
            return this as TCoreType;
        }

        public new TCoreType Activate()
        {
            this.GameObject.SetActive(true);
            return this as TCoreType;
        }

        public new TCoreType Deactivate()
        {
            this.GameObject.SetActive(false);
            return this as TCoreType;
        }

        public new TCoreType OnPointerEnter(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerEnter += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new TCoreType OnPointerExit(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerExit += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new TCoreType OnPointerClick(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<PointerEventAdapter>().PointerClick += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new TCoreType OnBeginDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().BeginDrag += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new TCoreType OnDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().Drag += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new TCoreType OnEndDrag(Action<PointerEventData> action)
        {
            this.GameObject.GetOrAddComponent<DragAdapter>().EndDrag += (sender, e) => action(e);
            return this as TCoreType;
        }

        public new virtual TCoreType Clear()
        {
            foreach (Transform child in this.GameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            return this as TCoreType;
        }
    }
}
