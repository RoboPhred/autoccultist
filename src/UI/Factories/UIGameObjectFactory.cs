namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class UIGameObjectFactory<TFactory>
        where TFactory : UIGameObjectFactory<TFactory>
    {
        protected readonly GameObject gameObject;

        public UIGameObjectFactory(GameObject target)
        {
            this.gameObject = target;
            this.gameObject.GetOrAddComponent<CanvasRenderer>();
        }

        public GameObject Build()
        {
            return this.gameObject;
        }
    }
}
