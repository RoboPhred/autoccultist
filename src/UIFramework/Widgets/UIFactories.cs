namespace AutoccultistNS.UI
{
    using UnityEngine;

    public static class UIFactories
    {
        public static RectTransformWidget AddRectTransform(GameObject gameObject)
        {
            return new RectTransformWidget(gameObject);
        }

        public static SizingLayoutWidget AddSizingLayout(GameObject gameObject)
        {
            return new SizingLayoutWidget(gameObject);
        }

        public static SizingLayoutWidget CreateSizingLayout(string key, GameObject parent)
        {
            return CreateSizingLayout(key, parent.transform);
        }

        public static SizingLayoutWidget CreateSizingLayout(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new SizingLayoutWidget(gameObject);
        }

        public static ImageWidget CreateImage(string key, GameObject parent)
        {
            return CreateImage(key, parent.transform);
        }

        public static ImageWidget CreateImage(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new ImageWidget(gameObject);
        }

        public static TextWidget CreateText(string key, GameObject parent)
        {
            return CreateText(key, parent.transform);
        }

        public static TextWidget CreateText(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new TextWidget(gameObject);
        }

        public static IconButtonWidget CreateIconButton(string key, GameObject parent)
        {
            return CreateIconButton(key, parent.transform);
        }

        public static IconButtonWidget CreateIconButton(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new IconButtonWidget(gameObject);
        }

        public static TextButtonWidget CreateTextButton(string key, GameObject parent)
        {
            return CreateTextButton(key, parent.transform);
        }

        public static TextButtonWidget CreateTextButton(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new TextButtonWidget(gameObject);
        }

        public static VerticalLayoutGroupWidget CreateVeritcalLayoutGroup(string key, GameObject parent)
        {
            return CreateVeritcalLayoutGroup(key, parent.transform);
        }

        public static VerticalLayoutGroupWidget CreateVeritcalLayoutGroup(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new VerticalLayoutGroupWidget(gameObject);
        }

        public static HorizontalLayoutGroupWidget CreateHorizontalLayoutGroup(string key, GameObject parent)
        {
            return CreateHorizontalLayoutGroup(key, parent.transform);
        }

        public static HorizontalLayoutGroupWidget CreateHorizontalLayoutGroup(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new HorizontalLayoutGroupWidget(gameObject);
        }

        public static ScrollRegionWidget CreateScroll(string key, GameObject parent)
        {
            return CreateScroll(key, parent.transform);
        }

        public static ScrollRegionWidget CreateScroll(string key, Transform parent)
        {
            var gameObject = new GameObject(key);
            gameObject.transform.SetParent(parent, false);
            return new ScrollRegionWidget(gameObject);
        }
    }
}
