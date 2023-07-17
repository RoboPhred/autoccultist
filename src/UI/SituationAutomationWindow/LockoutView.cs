namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class LockoutView : IWindowView
    {
        public LockoutView(SituationAutomationWindow window, WidgetMountPoint content)
        {
            content.AddVerticalLayoutGroup("VerticalLayout")
                .Padding(10, 2)
                .ExpandWidth()
                .SpreadChildrenHorizontally()
                .FitContentHeight()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("LockoutText")
                        .TextAlignment(TMPro.TextAlignmentOptions.Center)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .FontSize(32)
                        .Text("Locked out");

                    mountPoint.AddSizingLayout("Spacer")
                        .PreferredHeight(10);

                    mountPoint.AddText("Explainer")
                        .TextAlignment(TMPro.TextAlignmentOptions.Center)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .FontSize(20)
                        .Text("Autoccultist will not automate this verb.  You can use it for your own purposes.");

                    mountPoint.AddSizingLayout("Spacer")
                        .PreferredHeight(10);

                    mountPoint.AddTextButton("Unlock")
                        .Text("Unlock")
                        .OnClick(() =>
                            {
                                window.ToggleLockout();
                            });
                });
        }

        public void UpdateContent()
        {
        }
    }
}
