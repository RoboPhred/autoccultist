namespace AutoccultistNS.UI
{
    public static class WidgetMountPointExtensions
    {
        public static RectTransformWidget AddRectTransform(this WidgetMountPoint mountPoint, string key = "RectTransform")
        {
            var widget = new RectTransformWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static SizingLayoutWidget AddSizingLayout(this WidgetMountPoint mountPoint, string key = "SizingLayout")
        {
            var widget = new SizingLayoutWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static ImageWidget AddImage(this WidgetMountPoint mountPoint, string key = "Image")
        {
            var widget = new ImageWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static TextWidget AddText(this WidgetMountPoint mountPoint, string key = "Text")
        {
            var widget = new TextWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static IconButtonWidget AddIconButton(this WidgetMountPoint mountPoint, string key = "IconButton")
        {
            var widget = new IconButtonWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static TextButtonWidget AddTextButton(this WidgetMountPoint mountPoint, string key = "TextButton")
        {
            var widget = new TextButtonWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static VerticalLayoutGroupWidget AddVerticalLayoutGroup(this WidgetMountPoint mountPoint, string key = "VerticalLayoutGroup")
        {
            var widget = new VerticalLayoutGroupWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static HorizontalLayoutGroupWidget AddHorizontalLayoutGroup(this WidgetMountPoint mountPoint, string key = "HorizontalLayoutGroup")
        {
            var widget = new HorizontalLayoutGroupWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }

        public static ScrollRegionWidget AddScrollRegion(this WidgetMountPoint mountPoint, string key = "ScrollRegion")
        {
            var widget = new ScrollRegionWidget(key);
            mountPoint.AddWidget(widget);
            return widget;
        }
    }
}
