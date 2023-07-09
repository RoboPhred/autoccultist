namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class VerticalLayoutGroupFactory : RectTransformFactory<VerticalLayoutGroupFactory>
    {
        private readonly VerticalLayoutGroup layoutGroup;

        public VerticalLayoutGroupFactory(GameObject gameObject)
            : base(gameObject)
        {
            this.layoutGroup = this.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
        }

        public VerticalLayoutGroupFactory ChildControlHeight(bool value)
        {
            this.layoutGroup.childControlHeight = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildControlWidth(bool value)
        {
            this.layoutGroup.childControlWidth = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildForceExpandHeight(bool value)
        {
            this.layoutGroup.childForceExpandHeight = value;
            return this;
        }

        public VerticalLayoutGroupFactory ChildForceExpandWidth(bool value)
        {
            this.layoutGroup.childForceExpandWidth = value;
            return this;
        }

        public new VerticalLayoutGroup Build()
        {
            return this.layoutGroup;
        }
    }
}
