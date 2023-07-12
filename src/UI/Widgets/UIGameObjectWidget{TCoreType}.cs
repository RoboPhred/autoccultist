namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class UIGameObjectWidget<TCoreType>
        where TCoreType : UIGameObjectWidget<TCoreType>
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

        public CanvasRenderer CanvasRenderer { get; private set; }

        public static implicit operator GameObject(UIGameObjectWidget<TCoreType> widget)
        {
            return widget.GameObject;
        }

        public TCoreType SetActive(bool active)
        {
            this.GameObject.SetActive(active);
            return this as TCoreType;
        }

        public TCoreType Activate()
        {
            this.GameObject.SetActive(true);
            return this as TCoreType;
        }

        public TCoreType Deactivate()
        {
            this.GameObject.SetActive(false);
            return this as TCoreType;
        }

        public virtual TCoreType Clear()
        {
            foreach (Transform child in this.GameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            return this as TCoreType;
        }

        public TCoreType AttachTo(GameObject parent)
        {
            this.GameObject.transform.SetParent(parent.transform, false);
            return this as TCoreType;
        }
    }
}