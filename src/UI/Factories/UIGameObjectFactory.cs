namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class UIGameObjectFactory<TFactory>
        where TFactory : UIGameObjectFactory<TFactory>
    {
        public UIGameObjectFactory(GameObject target)
        {
            this.GameObject = target;
            this.GameObject.GetOrAddComponent<CanvasRenderer>();
        }

        protected GameObject GameObject { get; private set; }

        public GameObject Build()
        {
            return this.GameObject;
        }
    }
}
