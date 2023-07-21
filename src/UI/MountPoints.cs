namespace AutoccultistNS.UI
{
    using UnityEngine;

    public static class MountPoints
    {
        public static WidgetMountPoint TabletopWindowLayer
        {
            get
            {
                var windowSphere = GameObject.Find("TabletopWindowSphere");
                if (windowSphere == null)
                {
                    return null;
                }

                return new WidgetMountPoint(windowSphere.transform);
            }
        }

        public static WidgetMountPoint MetaWindowLayer
        {
            get
            {
                var windowSphere = GameObject.Find("CanvasMeta");
                if (windowSphere == null)
                {
                    return null;
                }

                return new WidgetMountPoint(windowSphere.transform);
            }
        }
    }
}
