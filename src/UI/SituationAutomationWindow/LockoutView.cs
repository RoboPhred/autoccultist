namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class LockoutView : IWindowView
    {
        public LockoutView(SituationAutomationWindow window, Transform contentRoot)
        {
            var test = UIFactories.CreateVeritcalLayoutGroup("VerticalLayout", contentRoot)
                .Padding(10, 2)
                .ExpandWidth()
                .FitContentHeight()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("LockoutText")
                        .ExpandWidth()
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
                        .Left(0, 0)
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
