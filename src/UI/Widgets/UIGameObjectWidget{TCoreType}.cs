namespace AutoccultistNS.UI
{
    using UnityEngine;

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
