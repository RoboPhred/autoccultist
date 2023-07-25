namespace AutoccultistNS.UI
{
    using Roost.Piebald;

    public class LockoutView : IWindowView<SituationAutomationWindow.IWindowContext>
    {
        public void Attach(SituationAutomationWindow.IWindowContext window)
        {
            window.Content.AddVerticalLayoutGroup("VerticalLayout")
                .SetPadding(10, 2)
                .SetExpandWidth()
                .SetSpreadChildrenHorizontally()
                .SetFitContentHeight()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("LockoutText")
                        .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .SetFontSize(32)
                        .SetText("Locked out");

                    mountPoint.AddSizingLayout("Spacer")
                        .SetPreferredHeight(10);

                    mountPoint.AddText("Explainer")
                        .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .SetFontSize(20)
                        .SetText("Autoccultist will not automate this verb.  You can use it for your own purposes.");

                    mountPoint.AddSizingLayout("Spacer")
                        .SetPreferredHeight(10);

                    mountPoint.AddTextButton("Unlock")
                        .SetText("Unlock")
                        .OnClick(() =>
                            {
                                window.ToggleLockout();
                            });
                });
        }

        public void Detatch()
        {
        }

        public void Update()
        {
        }
    }
}
