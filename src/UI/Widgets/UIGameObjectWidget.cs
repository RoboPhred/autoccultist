namespace AutoccultistNS.UI
{
    using UnityEngine;

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

        public CanvasRenderer CanvasRenderer { get; private set; }

        public static implicit operator GameObject(UIGameObjectWidget widget)
        {
            return widget.GameObject;
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
