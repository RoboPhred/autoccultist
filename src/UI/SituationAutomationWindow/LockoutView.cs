namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class LockoutView : IWindowView
    {
        public LockoutView(SituationAutomationWindow window, Transform contentRoot)
        {
            UIFactories.CreateVeritcalLayoutGroup("VerticalLayout", contentRoot)
                .FillContentHeight()
                .AddContent(transform =>
                {
                    UIFactories.CreateText("LockoutText", transform)
                        .Text("Locked out");
                    UIFactories.CreateTextButton("Unlock", transform)
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
