namespace AutoccultistNS.UI
{
    using UnityEngine;

    public static class UIFactories
    {
        public static RectTransformFactory AddRectTransform(GameObject gameObject)
        {
            return new RectTransformFactory(gameObject);
        }

        public static SizingElementFactory AddSizingElement(GameObject gameObject)
        {
            return new SizingElementFactory(gameObject);
        }

        public static ImageFactory CreateImage(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new ImageFactory(gameObject);
        }

        public static TextFactory CreateText(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new TextFactory(gameObject);
        }

        public static IconButtonFactory CreateIconButton(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new IconButtonFactory(gameObject);
        }

        public static TextButtonFactory CreateTextButton(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new TextButtonFactory(gameObject);
        }

        public static VerticalLayoutGroupFactory CreateVeritcalLayoutGroup(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new VerticalLayoutGroupFactory(gameObject);
        }

        public static HorizontalLayoutGroupFactory CreateHorizontalLayoutGroup(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new HorizontalLayoutGroupFactory(gameObject);
        }

        public static ScrollRectFactory CreateScrollRect(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new ScrollRectFactory(gameObject);
        }
    }
}
